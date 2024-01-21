﻿using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

[assembly:GeneratorRpcServerRegister]//生成注册

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigureContainer(a =>
                   {
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
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"//设定连接口令，作用类似账号密码
                   });

            service.Setup(config);
            service.Start();


            service.Logger.Info($"{service.GetType().Name}已启动");

            service.Logger.Info($"输入客户端Id，空格输入消息，将通知客户端方法");
            while (true)
            {
                var str = Console.ReadLine();
                if (service.TryGetSocketClient(str.Split(' ')[0], out var socketClient))
                {
                    var result = socketClient.GetDmtpRpcActor().InvokeT<bool>("Notice", DmtpInvokeOption.WaitInvoke, str.Split(' ')[1]);

                    service.Logger.Info($"调用结果{result}");
                }
            }

        }
    }

    [GeneratorRpcServer]
    [MyRpcActionFilter]
    public partial class MyRpcServer : RpcServer
    {
        private readonly ILog m_logger;

        public MyRpcServer(ILog logger)
        {
            this.m_logger = logger;
        }

        /// <summary>
        /// 将两个数相加
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [DmtpRpc(true)]//使用函数名直接调用
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
        public int RpcPullChannel(IDmtpRpcCallContext callContext, int channelID)
        {
            var size = 0;
            var package = 1024 * 64;
            if (callContext.Caller is TcpDmtpSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out var channel))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        size += package;
                        channel.Write(new byte[package]);
                    }
                    channel.Complete();//必须调用指令函数，如Complete，Cancel，Dispose
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
        public int RpcPushChannel(ICallContext callContext, int channelID)
        {
            var size = 0;

            if (callContext.Caller is TcpDmtpSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out var channel))
                {
                    foreach (var item in channel)
                    {
                        size += item.Len;//此处处理流数据
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
            if (callContext.Caller is ISocketClient client)
            {
                client.Logger.Info($"即将执行Rpc-{callContext.MethodInstance.Name}");
            }
            return Task.FromResult(invokeResult);
        }

        public override Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
        {
            if (callContext.Caller is ISocketClient client)
            {
                client.Logger.Info($"执行RPC-{callContext.MethodInstance.Name}完成，状态={invokeResult.Status}");
            }
            return Task.FromResult(invokeResult);
        }

        public override Task<InvokeResult> ExecutExceptionAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
        {
            if (callContext.Caller is ISocketClient client)
            {
                client.Logger.Info($"执行RPC-{callContext.MethodInstance.Name}异常，信息={invokeResult.Message}");
            }
            return Task.FromResult(invokeResult);
        }
    }
}