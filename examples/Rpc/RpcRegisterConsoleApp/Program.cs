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

[assembly: GeneratorRpcServerRegister]//启用源生成注册

namespace RpcRegisterConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("RPC注册服务示例");
        Console.WriteLine("1. 注册实例服务");
        Console.WriteLine("2. 注册接口服务");
        Console.WriteLine("3. 注册瞬态服务");
        Console.WriteLine("4. 注册区域服务");
        Console.WriteLine("5. 注册指定程序集的所有服务");
        Console.WriteLine("6. 使用源生成注册");

        await RegisterInstanceServiceAsync();
        await RegisterInterfaceServiceAsync();
        await RegisterTransientServiceAsync();
        await RegisterScopedServiceAsync();
        await RegisterAllServerAsync();
        await RegisterWithSourceGeneratorAsync();
        
        Console.ReadKey();
    }

    #region Rpc注册实例服务
    /// <summary>
    /// 注册实例服务示例
    /// </summary>
    private static async Task RegisterInstanceServiceAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7790)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    //注册实例服务
                    store.RegisterServer<MyRpcServer>();

                    //或者按照类型注册
                    //store.RegisterServer(typeof(MyRpcServer));
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("实例服务已启动");
    }
    #endregion

    #region Rpc注册接口服务
    /// <summary>
    /// 注册接口服务示例
    /// </summary>
    private static async Task RegisterInterfaceServiceAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7791)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    //注册接口与实现服务
                    store.RegisterServer<IMyRpcServer2, MyRpcServer2>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("接口服务已启动");
    }
    #endregion

    #region Rpc注册瞬态服务
    /// <summary>
    /// 注册瞬态服务示例
    /// </summary>
    private static async Task RegisterTransientServiceAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7792)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    //注册瞬态服务，每次调用都会创建新实例
                    store.RegisterServer<MyTransientRpcServer>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("瞬态服务已启动");
    }
    #endregion

    #region Rpc注册区域服务
    /// <summary>
    /// 注册区域服务示例
    /// </summary>
    private static async Task RegisterScopedServiceAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7793)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    //注册区域服务
                    store.RegisterServer<MyScopedRpcServer>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("区域服务已启动");
    }
    #endregion

    #region Rpc注册程序集所有服务
    /// <summary>
    /// 注册程序集中的所有服务示例
    /// </summary>
    private static async Task RegisterAllServerAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7794)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    //注册指定程序集的所有服务
                    store.RegisterAllServer(typeof(Program).Assembly);

                    //或者注册已加载程序集中的所有服务
                    //store.RegisterAllServer();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("程序集所有服务已启动");
    }
    #endregion

    #region Rpc源生成注册
    /// <summary>
    /// 使用源生成注册服务示例
    /// </summary>
    private static async Task RegisterWithSourceGeneratorAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7795)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    //使用源生成的注册方法（方法名根据程序集名称生成）
                    store.RegisterAllFromRpcRegisterConsoleApp();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        Console.WriteLine("源生成服务已启动");
    }
    #endregion
}

#region Rpc服务类定义单例服务
/// <summary>
/// 单例RPC服务
/// </summary>
public partial class MyRpcServer : SingletonRpcServer
{
    /// <summary>
    /// 将两个数相加
    /// </summary>
    [DmtpRpc(MethodInvoke = true)]
    [Description("将两个数相加")]
    public int Add(int a, int b)
    {
        var sum = a + b;
        return sum;
    }
}
#endregion

#region Rpc服务类定义接口服务
/// <summary>
/// RPC服务接口
/// </summary>
public interface IMyRpcServer2 : ISingletonRpcServer
{
    [DmtpRpc(MethodInvoke = true)]
    int Add(int a, int b);
}

/// <summary>
/// 实现接口的RPC服务
/// </summary>
public partial class MyRpcServer2 : SingletonRpcServer, IMyRpcServer2
{
    public int Add(int a, int b)
    {
        var sum = a + b;
        return sum;
    }
}
#endregion

#region Rpc服务类定义瞬态服务
/// <summary>
/// 瞬态RPC服务，每次调用都会创建新实例
/// </summary>
public partial class MyTransientRpcServer : TransientRpcServer
{
    [DmtpRpc(MethodInvoke = true)]
    [Description("瞬态服务中的方法")]
    public int Multiply(int a, int b)
    {
        //可以直接访问CallContext
        var callContext = this.CallContext;
        return a * b;
    }
}
#endregion

#region Rpc服务类定义区域服务
/// <summary>
/// 区域RPC服务，在每次调用时创建新实例，并在区域内单例
/// </summary>
public partial class MyScopedRpcServer : ScopedRpcServer
{
    [DmtpRpc(MethodInvoke = true)]
    [Description("区域服务中的方法")]
    public int Subtract(int a, int b)
    {
        //可以直接访问CallContext
        var callContext = this.CallContext;
        return a - b;
    }
}
#endregion
