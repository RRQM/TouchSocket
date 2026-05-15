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

using System.Reflection;
using TouchSocket.Core;
using TouchSocket.Mqtt;
using TouchSocket.Mqtt.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.MqttRpc.Generators;
using TouchSocket.Sockets;

namespace MqttRpcConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // 第一步：创建 Mqtt Broker，用于中转所有 Mqtt 消息（包括 RPC 请求/响应）
        var service = await CreateMqttBroker();

        // 第二步：创建充当 RPC 服务端的 Mqtt 客户端
        var serverClient = await CreateRpcServerClient();

        // 第三步：创建充当 RPC 调用端的 Mqtt 客户端
        var callerClient = await CreateRpcCallerClient();

        // 第四步：获取 IMqttRpcClient 接口并发起调用
        var rpcClient = callerClient.GetMqttRpcClient();

        #region MqttRpc直接调用
        // 使用 InvokeAsync 调用远程方法
        // invokeKey 默认为 "命名空间.类名.方法名" 全小写
        var addResult = (int)await rpcClient.InvokeAsync(
            "mqttrpcconsoleapp.imyrpcserver.add",
            typeof(int),
            InvokeOption.WaitInvoke,
            10, 20);
        Console.WriteLine($"Add(10, 20) = {addResult}");

        var dateResult = (string)await rpcClient.InvokeAsync(
            "mqttrpcconsoleapp.imyrpcserver.getcallerinfo",
            typeof(string),
            InvokeOption.WaitInvoke);
        Console.WriteLine($"GetCallerInfo() = {dateResult}");
        #endregion

        #region MqttRpc代理调用
        var proxyResult = await rpcClient.AddAsync(100, 200);
        Console.WriteLine($"代理调用 Add(100, 200) = {proxyResult}");
        #endregion

        Console.ReadKey();
    }

    #region MqttRpc创建Broker
    private static async Task<MqttTcpService> CreateMqttBroker()
    {
        var service = new MqttTcpService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(1883)
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));

        await service.StartAsync();
        Console.WriteLine("Mqtt Broker 已启动，监听端口：1883");
        return service;
    }
    #endregion

    #region MqttRpc创建服务端客户端
    private static async Task<MqttTcpClient> CreateRpcServerClient()
    {
        var client = new MqttTcpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:1883")
            .SetMqttConnectOptions(o =>
            {
                o.ClientId = "RpcServer";
                o.CleanSession = true;
            })
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
                // 注册 RPC 服务到容器
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<IMyRpcServer, MyRpcServer>();
                });
            })
            .ConfigurePlugins(a =>
            {
                // 服务端+客户端模式：此客户端既能接收 RPC 请求，也能发起 RPC 调用
                // UseMqttRpc 会从容器中解析 IRpcServerProvider，自动注册已有的 RPC 服务
                a.UseMqttRpc(o =>
                {
                    o.RequestTopic = "mqttrpc/req";         // 请求主题
                    o.ResponseTopicPrefix = "mqttrpc/res";  // 响应主题前缀
                    o.QosLevel = QosLevel.AtLeastOnce;      // QoS 等级
                });
            }));

        await client.ConnectAsync();
        Console.WriteLine("RPC 服务端客户端已连接");
        return client;
    }
    #endregion

    #region MqttRpc创建调用端客户端
    private static async Task<MqttTcpClient> CreateRpcCallerClient()
    {
        var client = new MqttTcpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:1883")
            .SetMqttConnectOptions(o =>
            {
                o.ClientId = "RpcCaller";
                o.CleanSession = true;
            })
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                // 纯客户端模式：只发起 RPC 调用，不注册任何 RPC 服务
                a.UseMqttRpcClient(o =>
                {
                    o.RequestTopic = "mqttrpc/req";
                    o.ResponseTopicPrefix = "mqttrpc/res";
                    o.QosLevel = QosLevel.AtLeastOnce;
                });
            }));

        await client.ConnectAsync();
        Console.WriteLine("RPC 调用端客户端已连接");
        return client;
    }
    #endregion

    #region MqttRpc自定义调用选项
    private static async Task DemoCustomInvokeOption(IMqttRpcClient rpcClient)
    {
        // 自定义调用选项：设置 10s 超时，并绑定取消令牌
        using var cts = new CancellationTokenSource();
        var option = new InvokeOption(10000)  // 设置 10s 超时
        {
            FeedbackType = FeedbackType.WaitInvoke,
            Token = cts.Token
        };

        var result = (int)await rpcClient.InvokeAsync(
            "mqttrpcconsoleapp.imyrpcserver.add",
            typeof(int),
            option,
            10, 20);
        Console.WriteLine($"自定义选项调用结果：{result}");
    }
    #endregion

    #region MqttRpc序列化配置
    private static async Task<MqttTcpClient> CreateCallerClientWithCustomSerializer()
    {
        var client = new MqttTcpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:1883")
            .SetMqttConnectOptions(o =>
            {
                o.ClientId = "RpcCallerCustom";
                o.CleanSession = true;
            })
            .ConfigurePlugins(a =>
            {
                a.UseMqttRpcClient(o =>
                {
                    // 自定义 JSON 序列化选项
                    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    o.SerializerOptions.WriteIndented = false;
                });
            }));

        await client.ConnectAsync();
        return client;
    }
    #endregion
}

#region MqttRpc定义服务
/// <summary>
/// RPC 服务实现，继承 SingletonRpcServer 以单例方式运行。
/// 标记 [MqttRpc] 的公共方法将被注册为 RPC 方法。
/// </summary>
public class MyRpcServer : SingletonRpcServer, IMyRpcServer
{
    /// <summary>
    /// 两数相加。
    /// </summary>
    public int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>
    /// 使用调用上下文获取调用方信息。
    /// IMqttRpcCallContext 参数由框架自动注入，不需要客户端传递。
    /// </summary>
    public string GetCallerInfo(IMqttRpcCallContext callContext)
    {
        return $"调用来自 Mqtt 客户端，服务器时间：{DateTime.Now:HH:mm:ss}";
    }
}
#endregion

#region MqttRpc调用上下文
/// <summary>
/// 演示在 RPC 方法中使用 IMqttRpcCallContext 获取调用方信息。
/// IMqttRpcCallContext 参数由框架自动注入，客户端无需传递。
/// </summary>
public class MyRpcServerWithContext : SingletonRpcServer
{
    [MqttRpc]
    public string GetCallerInfo(IMqttRpcCallContext callContext)
    {
        // 通过 callContext.Caller 可访问发起调用的 Mqtt 会话对象
        // 通过 callContext.Token 可获取与会话生命周期绑定的取消令牌
        return $"调用来自 Mqtt 客户端，服务器时间：{DateTime.Now:HH:mm:ss}";
    }
}
#endregion

#region MqttRpc定义代理接口
/// <summary>
/// 定义 RPC 代理接口，方法签名需与服务端保持一致（带 Async 后缀的异步版本）。
/// </summary>
[GeneratorRpcProxy]
public interface IMyRpcServer : ISingletonRpcServer
{
    [MqttRpc]
    int Add(int a, int b);

    [MqttRpc]
    string GetCallerInfo(IMqttRpcCallContext callContext);
}
#endregion