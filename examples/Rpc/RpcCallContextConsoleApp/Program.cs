// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcCallContextConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("RPC调用上下文示例");
        Console.WriteLine("1. 单例服务 - 通过参数获取调用上下文");
        Console.WriteLine("2. 瞬态服务 - 通过属性获取调用上下文");
        Console.WriteLine("3. 使用IRpcCallContextAccessor服务获取调用上下文");
        Console.WriteLine("4. 取消任务示例");
        Console.WriteLine();

        await CreateSingletonServerAsync();
        await CreateTransientServerAsync();
        await CreateServerWithAccessorAsync();
        await CreateServerWithCancellationAsync();

        Console.WriteLine("所有示例服务已启动，按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 创建单例服务示例
    /// </summary>
    private static async Task CreateSingletonServerAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7790)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<SingletonRpcServerExample>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("单例服务已启动，端口: 7790");
    }

    /// <summary>
    /// 创建瞬态服务示例
    /// </summary>
    private static async Task CreateTransientServerAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7791)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<TransientRpcServerExample>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("瞬态服务已启动，端口: 7791");
    }

    /// <summary>
    /// 创建带IRpcCallContextAccessor的服务示例
    /// </summary>
    private static async Task CreateServerWithAccessorAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7792)
            .ConfigureContainer(a =>
            {
                a.AddRpcCallContextAccessor();
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<ServerWithAccessor>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("IRpcCallContextAccessor服务已启动，端口: 7792");
    }

    /// <summary>
    /// 创建任务取消示例服务
    /// </summary>
    private static async Task CreateServerWithCancellationAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7793)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<ServerWithCancellation>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("任务取消服务已启动，端口: 7793");
    }
}

#region Rpc调用上下文单例服务参数获取
/// <summary>
/// 单例RPC服务示例 - 通过参数获取调用上下文
/// </summary>
public partial class SingletonRpcServerExample : SingletonRpcServer
{
    [Description("登录")]
    [DmtpRpc]
    public bool Login(ICallContext callContext, string account, string password)
    {
        if (callContext.Caller is TcpDmtpSessionClient sessionClient)
        {
            Console.WriteLine($"TcpDmtpRpc请求，客户端ID: {sessionClient.Id}");
            Console.WriteLine($"客户端IP: {sessionClient.IP}:{sessionClient.Port}");
        }

        if (account == "123" && password == "abc")
        {
            Console.WriteLine($"用户 {account} 登录成功");
            return true;
        }

        Console.WriteLine($"用户 {account} 登录失败");
        return false;
    }

    [Description("获取调用者信息")]
    [DmtpRpc]
    public string GetCallerInfo(ICallContext callContext)
    {
        if (callContext.Caller is TcpDmtpSessionClient sessionClient)
        {
            return $"调用者ID: {sessionClient.Id}, IP: {sessionClient.IP}:{sessionClient.Port}";
        }
        return "未知调用者";
    }
}
#endregion

#region Rpc调用上下文瞬态服务属性获取
/// <summary>
/// 瞬态RPC服务示例 - 通过属性获取调用上下文
/// </summary>
public partial class TransientRpcServerExample : TransientRpcServer
{
    [Description("登录")]
    [DmtpRpc]
    public bool Login(string account, string password)
    {
        if (this.CallContext.Caller is TcpDmtpSessionClient sessionClient)
        {
            Console.WriteLine($"TcpDmtpRpc请求，客户端ID: {sessionClient.Id}");
        }

        if (account == "123" && password == "abc")
        {
            return true;
        }

        return false;
    }

    [Description("获取调用方法信息")]
    [DmtpRpc]
    public string GetMethodInfo()
    {
        var methodName = this.CallContext.RpcMethod.Name;
        var callerType = this.CallContext.Caller?.GetType().Name ?? "未知";
        return $"方法: {methodName}, 调用者类型: {callerType}";
    }
}
#endregion

#region Rpc调用上下文泛型瞬态服务
/// <summary>
/// 使用泛型上下文的瞬态RPC服务示例
/// </summary>
public partial class GenericTransientRpcServerExample : TransientRpcServer<IDmtpRpcCallContext>
{
    [Description("登录")]
    [DmtpRpc]
    public bool Login(string account, string password)
    {
        if (this.CallContext.Caller is TcpDmtpSessionClient sessionClient)
        {
            Console.WriteLine($"TcpDmtpRpc请求，客户端ID: {sessionClient.Id}");
        }

        if (account == "123" && password == "abc")
        {
            return true;
        }

        return false;
    }
}
#endregion

#region Rpc调用上下文Accessor服务
/// <summary>
/// 使用IRpcCallContextAccessor服务的RPC服务示例
/// </summary>
public partial class ServerWithAccessor : SingletonRpcServer
{
    private readonly IRpcCallContextAccessor m_rpcCallContextAccessor;

    public ServerWithAccessor(IRpcCallContextAccessor rpcCallContextAccessor)
    {
        this.m_rpcCallContextAccessor = rpcCallContextAccessor;
    }

    [Description("测试从CallContextAccessor中获取当前关联的CallContext")]
    [DmtpRpc]
    public async Task<string> TestGetCallContextFromCallContextAccessor()
    {
        //通过CallContextAccessor获取当前关联的CallContext
        //此处即使m_rpcCallContextAccessor与当前RpcServer均为单例，也能获取到正确的CallContext
        var callContext = this.m_rpcCallContextAccessor.CallContext;

        if (callContext?.Caller is TcpDmtpSessionClient sessionClient)
        {
            var info = $"通过Accessor获取 - 客户端ID: {sessionClient.Id}, IP: {sessionClient.IP}";
            Console.WriteLine(info);
            await Task.CompletedTask;
            return info;
        }

        await Task.CompletedTask;
        return "无法获取调用上下文";
    }

    [Description("测试在异步方法中使用CallContextAccessor")]
    [DmtpRpc]
    public async Task<string> TestAsyncCallContextAccessor()
    {
        //即使在异步方法中，也能正确获取CallContext
        await Task.Delay(100);

        var callContext = this.m_rpcCallContextAccessor.CallContext;
        if (callContext?.Caller is TcpDmtpSessionClient sessionClient)
        {
            return $"异步方法中获取成功 - 客户端ID: {sessionClient.Id}";
        }

        return "异步方法中获取失败";
    }
}
#endregion

#region Rpc调用上下文取消任务
/// <summary>
/// 任务取消示例RPC服务
/// </summary>
public partial class ServerWithCancellation : SingletonRpcServer
{
    /// <summary>
    /// 测试取消调用
    /// </summary>
    /// <param name="callContext"></param>
    /// <returns></returns>
    [Description("测试取消调用")]
    [DmtpRpc]
    public async Task<int> TestCancellationToken(ICallContext callContext)
    {
        Console.WriteLine("开始执行长时间任务...");

        //模拟一个耗时操作
        for (var i = 0; i < 10; i++)
        {
            //判断任务是否已被取消
            if (callContext.Token.IsCancellationRequested)
            {
                Console.WriteLine($"执行已取消，已完成 {i} 次迭代");
                return i;
            }

            Console.WriteLine($"执行第 {i + 1} 次迭代");
            await Task.Delay(1000, callContext.Token);
        }

        Console.WriteLine("任务正常完成");
        return 10;
    }

    [Description("测试手动取消任务")]
    [DmtpRpc]
    public async Task<string> TestManualCancel(ICallContext callContext)
    {
        //延迟后手动取消任务
        await Task.Delay(2000);

       
        //后续操作将被取消
        await Task.Delay(1000, callContext.Token);

        return "此消息不应该返回";
    }
}
#endregion
