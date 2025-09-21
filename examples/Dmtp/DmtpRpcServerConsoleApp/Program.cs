//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

[assembly: GeneratorRpcServerRegister]//生成注册

namespace ConsoleApp2;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddRpcCallContextAccessor();

                   a.AddDmtpRouteService();
                   a.AddConsoleLogger();

                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<MyRpcServer>();
#if DEBUG
                       File.WriteAllText("../../../RpcProxy.cs", store.GetProxyCodes("RpcProxy", new Type[] { typeof(DmtpRpcAttribute) }));
                       ConsoleLogger.Default.Info("成功生成代理");
#endif
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();

                   a.Add<MyRpcPlugin>();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Dmtp";//设定连接口令，作用类似账号密码
               });

        await service.SetupAsync(config);
        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");

        service.Logger.Info($"输入客户端Id，空格输入消息，将通知客户端方法");
        while (true)
        {
            var str = Console.ReadLine();
            if (service.TryGetClient(str.Split(' ')[0], out var socketClient))
            {
                var result = await socketClient.GetDmtpRpcActor().InvokeTAsync<bool>("Notice", DmtpInvokeOption.WaitInvoke, str.Split(' ')[1]);

                service.Logger.Info($"调用结果{result}");
            }
        }
    }
}


[MyRpcActionFilter]
public partial class MyRpcServer : SingletonRpcServer
{
    private readonly ILog m_logger;
    private readonly IRpcCallContextAccessor m_rpcCallContextAccessor;

    public MyRpcServer(ILog logger, IRpcCallContextAccessor rpcCallContextAccessor)
    {
        this.m_logger = logger;
        this.m_rpcCallContextAccessor = rpcCallContextAccessor;
    }

    /// <summary>
    /// 将两个数相加
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [DmtpRpc(MethodInvoke = true)]//使用函数名直接调用
    [Description("将两个数相加")]//其作用是生成代理时，作为注释。
    [MyRpcActionFilter]
    public int Add(int a, int b)
    {
        this.m_logger.Info("调用Add");
        var sum = a + b;
        return sum;
    }

    /// <summary>
    /// 测试客户端请求，服务器响应大量流数据
    /// </summary>
    /// <param name="callContext"></param>
    /// <param name="channelID"></param>
    [Description("测试客户端请求，服务器响应大量流数据")]
    [DmtpRpc]
    public async Task<int> RpcPullChannel(IDmtpRpcCallContext callContext, int channelID)
    {
        var size = 0;
        var package = 1024 * 64;
        if (callContext.Caller is TcpDmtpSessionClient socketClient)
        {
            if (socketClient.TrySubscribeChannel(channelID, out var channel))
            {
                for (var i = 0; i < 10; i++)
                {
                    size += package;
                    await channel.SendAsync(new byte[package]);
                }
                await channel.CompleteAsync();//必须调用指令函数，如Complete，Cancel，Dispose
            }
        }
        return size;
    }

    /// <summary>
    /// "测试推送"
    /// </summary>
    /// <param name="callContext"></param>
    /// <param name="channelID"></param>
    [Description("测试客户端推送流数据")]
    [DmtpRpc]
    public async Task<int> RpcPushChannel(ICallContext callContext, int channelID)
    {
        var size = 0;

        if (callContext.Caller is TcpDmtpSessionClient socketClient)
        {
            if (socketClient.TrySubscribeChannel(channelID, out var channel))
            {
                while (channel.CanRead)
                {
                    using var cts = new CancellationTokenSource(10 * 1000);
                    var memory = await channel.ReadAsync(cts.Token);
                    //这里处理数据
                    size += memory.Length;
                }
            }
        }
        return size;
    }

    /// <summary>
    /// 测试取消调用
    /// </summary>
    /// <param name="callContext"></param>
    /// <returns></returns>
    [Description("测试取消调用")]
    [DmtpRpc]
    public async Task<int> TestCancellationToken(ICallContext callContext)
    {
        //模拟一个耗时操作
        for (var i = 0; i < 10; i++)
        {
            //判断任务是否已被取消
            if (callContext.Token.IsCancellationRequested)
            {
                Console.WriteLine("执行已取消");
                return i;
            }
            Console.WriteLine($"执行{i}次");
            await Task.Delay(1000);
        }

        return -1;
    }

    [Description("测试从CallContextAccessor中获取当前关联的CallContext")]
    [DmtpRpc]
    public async Task TestGetCallContextFromCallContextAccessor()
    {
        //通过CallContextAccessor获取当前关联的CallContext
        //此处即使m_rpcCallContextAccessor与当前SingletonRpcServer均为单例，也能获取到正确的CallContext
        var callContext = this.m_rpcCallContextAccessor.CallContext;
        await Task.CompletedTask;
    }
}

internal class MyRpcPlugin : PluginBase, IDmtpRoutingPlugin
{
    public async Task OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
    {
        if (e.RouterType == RouteType.Rpc)
        {
            e.IsPermitOperation = true;
            return;
        }

        await e.InvokeNext();
    }
}

public class MyRpcActionFilterAttribute : RpcActionFilterAttribute
{
    public override Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
    {
        //invokeResult = new InvokeResult()
        //{
        //    Status = InvokeStatus.UnEnable,
        //    Message = "不允许执行",
        //    Result = default
        //};
        if (callContext.Caller is ISessionClient client)
        {
            client.Logger.Info($"即将执行Rpc-{callContext.RpcMethod.Name}");
        }
        return Task.FromResult(invokeResult);
    }

    public override Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
    {
        if (callContext.Caller is ISessionClient client)
        {
            if (exception == null)
            {
                //无异常
                client.Logger.Info($"执行RPC-{callContext.RpcMethod.Name}完成，状态={invokeResult.Status}");
            }
            else
            {
                //有异常
                client.Logger.Info($"执行RPC-{callContext.RpcMethod.Name}异常，信息={invokeResult.Message}");
            }

        }
        return Task.FromResult(invokeResult);
    }
}