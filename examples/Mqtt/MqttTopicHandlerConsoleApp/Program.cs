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

using System.Text;
using TouchSocket.Core;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;

namespace MqttTopicHandlerConsoleApp;

internal class Program
{
    private const int Port = 7791;

    private static async Task Main(string[] args)
    {
        // 启动内嵌 Mqtt Broker（TCP 协议）
        var service = new MqttTcpService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(Port)
            .ConfigureContainer(a => a.AddConsoleLogger()));
        await service.StartAsync();

        Console.WriteLine($"Mqtt Broker 已启动，监听端口 {Port}");

        await BasicReceiveAsync();
        await WildcardPlusAsync();
        await WildcardHashAsync();
        await MultipleTopicsAsync();

        Console.WriteLine("所有示例运行完毕，按任意键退出...");
        Console.ReadKey();

        await service.StopAsync();
        service.Dispose();
    }

    // 将 ReadOnlySequence<byte> 解码为字符串的帮助方法
    private static string DecodePayload(System.Buffers.ReadOnlySequence<byte> payload)
        => Encoding.UTF8.GetString(payload.FirstSpan);

    /// <summary>
    /// 演示 UseMqttTopicHandler 基本用法：精确主题订阅与消息接收。
    /// </summary>
    private static async Task BasicReceiveAsync()
    {
        #region Mqtt客户端UseMqttTopicHandler基本使用
        // 创建发布者客户端
        var publisher = new MqttTcpClient();
        await publisher.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "publisher-basic-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            }));

        // 创建订阅者客户端，并使用 UseMqttTopicHandler 配置主题处理器
        var subscriber = new MqttTcpClient();
        await subscriber.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "subscriber-basic-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            })
            .ConfigurePlugins(a =>
            {
                // UseMqttTopicHandler 会在连接成功后自动完成主题订阅，
                // 并将收到的消息路由到对应的处理器。
                a.UseMqttTopicHandler<MqttTcpClient>(opt =>
                {
                    opt.AddSubscription("home/temperature", async (client, msg) =>
                    {
                        var text = DecodePayload(msg.Payload);
                        Console.WriteLine("收到消息 topic=" + msg.TopicName + " payload=" + text);
                        await Task.CompletedTask;
                    });
                });
            }));

        // 先连接订阅者，再连接发布者
        await subscriber.ConnectAsync();
        await publisher.ConnectAsync();

        // 稍作等待，让订阅请求完成
        await Task.Delay(300);

        // 发布消息
        var payload = Encoding.UTF8.GetBytes("25.6C");
        await publisher.PublishAsync(new MqttPublishMessage("home/temperature", false, QosLevel.AtMostOnce, payload));

        await Task.Delay(300);
        #endregion

        await subscriber.CloseAsync();
        await publisher.CloseAsync();
    }

    /// <summary>
    /// 演示 UseMqttTopicHandler 单级通配符（+）用法。
    /// </summary>
    private static async Task WildcardPlusAsync()
    {
        #region Mqtt客户端UseMqttTopicHandler单级通配符
        // 过滤主题使用 "+" 单级通配符，可匹配该层级的任意单个节点。
        // 例如 "home/+/temperature" 能匹配 "home/room1/temperature"、"home/room2/temperature" 等。
        var publisher = new MqttTcpClient();
        await publisher.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "publisher-plus-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            }));

        var subscriber = new MqttTcpClient();
        await subscriber.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "subscriber-plus-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            })
            .ConfigurePlugins(a =>
            {
                a.UseMqttTopicHandler<MqttTcpClient>(opt =>
                {
                    // "+" 匹配单级通配符
                    opt.AddSubscription("home/+/temperature", async (client, msg) =>
                    {
                        var text = DecodePayload(msg.Payload);
                        Console.WriteLine("收到消息 topic=" + msg.TopicName + " payload=" + text);
                        await Task.CompletedTask;
                    });
                });
            }));

        await subscriber.ConnectAsync();
        await publisher.ConnectAsync();
        await Task.Delay(300);

        // "home/room1/temperature" 符合 "home/+/temperature" 过滤规则，可以被接收
        await publisher.PublishAsync(new MqttPublishMessage(
            "home/room1/temperature", false, QosLevel.AtMostOnce,
            Encoding.UTF8.GetBytes("22.1C")));

        // "home/room2/temperature" 同样符合过滤规则
        await publisher.PublishAsync(new MqttPublishMessage(
            "home/room2/temperature", false, QosLevel.AtMostOnce,
            Encoding.UTF8.GetBytes("23.5C")));

        await Task.Delay(300);
        #endregion

        await subscriber.CloseAsync();
        await publisher.CloseAsync();
    }

    /// <summary>
    /// 演示 UseMqttTopicHandler 多级通配符（#）用法。
    /// </summary>
    private static async Task WildcardHashAsync()
    {
        #region Mqtt客户端UseMqttTopicHandler多级通配符
        // 过滤主题使用 "#" 多级通配符，可匹配任意层级的主题后缀。
        // 例如 "home/#" 能匹配 "home/temperature"、"home/room1/humidity"、"home/a/b/c" 等。
        var publisher = new MqttTcpClient();
        await publisher.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "publisher-hash-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            }));

        var subscriber = new MqttTcpClient();
        await subscriber.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "subscriber-hash-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            })
            .ConfigurePlugins(a =>
            {
                a.UseMqttTopicHandler<MqttTcpClient>(opt =>
                {
                    // "#" 匹配多级通配符，必须置于主题末尾
                    opt.AddSubscription("home/#", async (client, msg) =>
                    {
                        var text = DecodePayload(msg.Payload);
                        Console.WriteLine("收到消息 topic=" + msg.TopicName + " payload=" + text);
                        await Task.CompletedTask;
                    });
                });
            }));

        await subscriber.ConnectAsync();
        await publisher.ConnectAsync();
        await Task.Delay(300);

        // 以下三条消息的主题均能匹配 "home/#"
        await publisher.PublishAsync(new MqttPublishMessage(
            "home/temperature", false, QosLevel.AtMostOnce,
            Encoding.UTF8.GetBytes("26C")));

        await publisher.PublishAsync(new MqttPublishMessage(
            "home/room1/humidity", false, QosLevel.AtMostOnce,
            Encoding.UTF8.GetBytes("60%")));

        await publisher.PublishAsync(new MqttPublishMessage(
            "home/floor1/room2/light", false, QosLevel.AtMostOnce,
            Encoding.UTF8.GetBytes("ON")));

        await Task.Delay(300);
        #endregion

        await subscriber.CloseAsync();
        await publisher.CloseAsync();
    }

    /// <summary>
    /// 演示 UseMqttTopicHandler 同时订阅多个主题。
    /// </summary>
    private static async Task MultipleTopicsAsync()
    {
        #region Mqtt客户端UseMqttTopicHandler多主题订阅
        // 可以链式调用 AddSubscription，为不同的主题绑定独立的处理器。
        var publisher = new MqttTcpClient();
        await publisher.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "publisher-multi-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            }));

        var subscriber = new MqttTcpClient();
        await subscriber.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{Port}")
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "subscriber-multi-" + Guid.NewGuid().ToString("N");
                options.CleanSession = true;
                options.KeepAlive = 60;
                options.Version = MqttProtocolVersion.V311;
            })
            .ConfigurePlugins(a =>
            {
                a.UseMqttTopicHandler<MqttTcpClient>(opt =>
                {
                    // 链式调用，为每个主题独立注册处理器
                    opt.AddSubscription("sensor/temperature", async (client, msg) =>
                    {
                        Console.WriteLine("温度数据: " + DecodePayload(msg.Payload));
                        await Task.CompletedTask;
                    })
                    .AddSubscription("sensor/humidity", async (client, msg) =>
                    {
                        Console.WriteLine("湿度数据: " + DecodePayload(msg.Payload));
                        await Task.CompletedTask;
                    })
                    .AddSubscription("sensor/pressure", QosLevel.AtLeastOnce, async (client, msg) =>
                    {
                        // 也可以为单个主题指定 QoS 等级
                        Console.WriteLine("气压数据(QoS1): " + DecodePayload(msg.Payload));
                        await Task.CompletedTask;
                    });
                });
            }));

        await subscriber.ConnectAsync();
        await publisher.ConnectAsync();
        await Task.Delay(300);

        await publisher.PublishAsync(new MqttPublishMessage(
            "sensor/temperature", false, QosLevel.AtMostOnce, Encoding.UTF8.GetBytes("27C")));
        await publisher.PublishAsync(new MqttPublishMessage(
            "sensor/humidity", false, QosLevel.AtMostOnce, Encoding.UTF8.GetBytes("55%")));
        await publisher.PublishAsync(new MqttPublishMessage(
            "sensor/pressure", false, QosLevel.AtLeastOnce, Encoding.UTF8.GetBytes("1013hPa")));

        await Task.Delay(300);
        #endregion

        await subscriber.CloseAsync();
        await publisher.CloseAsync();
    }
}
