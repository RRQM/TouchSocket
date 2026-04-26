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

namespace MqttConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();

        Console.ReadKey();

        var client = await CreateClient();

        try
        {
            #region Mqtt客户端订阅主题
            var topic1 = "topic1";
            var topic2 = "topic2";
            var subscribeRequest1 = new SubscribeRequest(topic1, QosLevel.AtLeastOnce);//订阅请求
            var subscribeRequest2 = new SubscribeRequest(topic2, QosLevel.AtMostOnce);//可以设置不同的Qos级别

            //多个订阅请求
            var mqttSubscribeMessage = new MqttSubscribeMessage(subscribeRequest1, subscribeRequest2);

            //执行订阅
            var mqttSubAckMessage = await client.SubscribeAsync(mqttSubscribeMessage);

            //输出订阅结果
            foreach (var item in mqttSubAckMessage.ReturnCodes)
            {
                Console.WriteLine($"ReturnCode:{item}");
            }
            #endregion

            //client.su
            ValueCounter counter = new()
            {
                OnPeriod = (c) =>
                {
                    Console.WriteLine($"Sent:{c}");
                },
                Period = TimeSpan.FromSeconds(1)
            };

            long i = 0;

            var message = new MqttPublishMessage(topic1, false, QosLevel.AtLeastOnce, Encoding.UTF8.GetBytes(
                   $"Hello World{i}"));

            while (true)
            {

                #region Mqtt客户端检查连接状态
                if (!client.Online)
                {
                    Console.WriteLine("客户端已掉线");
                    return;
                }
                #endregion

                i++;
                counter.Increment();


                await client.PublishAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (i <= 10000000)
                {
                    //Console.WriteLine($"continue"+i);
                    continue;
                }

                await client.CloseAsync();
                break;
                //await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("program" + ex.Message);
            //Console.WriteLine($"ForwardMessageCount={MqttBroker.m_ForwardMessageCount}");
            //Console.WriteLine($"DistributeMessagesCount={MqttSessionActor.DistributeMessagesCount}");
            //Console.WriteLine($"WaitForReadCount={MqttSessionActor.WaitForReadCount}");
        }

        while (true)
        {
            Console.ReadKey();
        }

        #region Mqtt服务器向所有客户端发布消息
        var broker = service.MqttBroker;
        await broker.ForwardMessageAsync(new MqttArrivedMessage("topicName", QosLevel.AtLeastOnce, false, new System.Buffers.ReadOnlySequence<byte>(Encoding.UTF8.GetBytes("hello"))));
        #endregion

        #region Mqtt客户端断开连接
        await client.CloseAsync("close msg");
        #endregion

        #region Mqtt客户端释放资源
        client.Dispose();
        #endregion

        #region Mqtt服务器停止服务器
        await service.StopAsync();
        #endregion

        #region Mqtt服务器释放资源
        service.Dispose();
        #endregion
    }
    public static async Task RunPublishAsync(IMqttClient client)
    {
        #region Mqtt客户端发布消息
        var payload = Encoding.UTF8.GetBytes("Hello World");

        //发布最多一次消息
        var message1 = new MqttPublishMessage("topic1", false, QosLevel.AtMostOnce, payload);
        await client.PublishAsync(message1);

        //发布至少一次消息
        var message2 = new MqttPublishMessage("topic1", false, QosLevel.AtLeastOnce, payload);
        await client.PublishAsync(message2);

        //发布只有一次消息
        var message3 = new MqttPublishMessage("topic1", false, QosLevel.ExactlyOnce, payload);
        await client.PublishAsync(message3);
        #endregion

        #region Mqtt客户端高性能发布消息

        //高性能发布消息需要自己管理内存，TouchSocket.Core中有相关的内存池可以使用。
        var byteBlock = new ValueByteBlock(1024);
        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, "Hello World", Encoding.UTF8);
            var message4 = new MqttPublishMessage("topic1", false, QosLevel.AtMostOnce, byteBlock.Memory);
            await client.PublishAsync(message1);
        }
        finally
        {
            byteBlock.Dispose();
        }
        #endregion
    }

    public static async Task RunUnsubscribeAsync(IMqttClient client)
    {
        #region Mqtt客户端取消订阅
        var mqttUnsubAck = await client.UnsubscribeAsync(new MqttUnsubscribeMessage("topic1", "topic2"));
        #endregion
    }

    private static async Task<MqttTcpService> CreateService()
    {
        #region 创建Tcp协议的Mqtt服务器
        var service = new MqttTcpService();

        //配置
        var config = new TouchSocketConfig();

        config.SetListenIPHosts(7789);
        config.ConfigureContainer(a =>
        {
            a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
        });

        //载入配置
        await service.SetupAsync(config);

        await service.StartAsync();//启动
        #endregion

        #region Mqtt服务器获取所有连接的客户端
        var clients = service.Clients;
        foreach (var client in clients)
        {

        }
        #endregion

        #region Mqtt服务器断开指定客户端
        foreach (var client in service.Clients)
        {
            if (client.Id == "123")
            {
                await client.CloseAsync("normal close");
            }
        }
        #endregion

        #region Mqtt服务器查询所有主题
        // 获取当前 Broker 中已注册的所有主题名称快照
        var broker1 = service.MqttBroker;
        var allTopics = broker1.GetAllTopics();
        foreach (var topic in allTopics)
        {
            Console.WriteLine($"Topic: {topic}");
        }
        #endregion

        #region Mqtt服务器查询主题的订阅者
        // GetSubscribers 支持通配符匹配，返回与指定主题匹配的所有订阅者
        var broker2 = service.MqttBroker;
        var subscribers = broker2.GetSubscribers("home/temperature");
        foreach (var sub in subscribers)
        {
            Console.WriteLine($"ClientId: {sub.ClientId}, QoS: {sub.QosLevel}");
        }
        #endregion

        #region Mqtt服务器检查订阅者
        // 精确判断某个客户端是否订阅了指定主题
        var broker3 = service.MqttBroker;
        var subscription = new Subscription("client-001", QosLevel.AtLeastOnce);
        var exists = broker3.ContainsSubscriber("home/temperature", subscription);
        Console.WriteLine($"订阅关系是否存在: {exists}");
        #endregion

        #region Mqtt服务器手动添加订阅
        // 服务端主动为客户端注册订阅关系，无需客户端发送 Subscribe 指令
        var broker4 = service.MqttBroker;
        broker4.RegisterActor("client-001", "home/temperature", QosLevel.AtLeastOnce);
        broker4.RegisterActor("client-001", "home/humidity", QosLevel.AtMostOnce);
        #endregion

        #region Mqtt服务器手动移除订阅
        // 从指定主题中移除某个客户端的订阅关系
        var broker5 = service.MqttBroker;
        broker5.UnregisterActor("client-001", "home/temperature");
        #endregion

        #region Mqtt服务器清空主题订阅
        var broker6 = service.MqttBroker;

        // 移除指定主题下的所有订阅者（保留空主题）
        broker6.ClearTopic("home/temperature");

        // 清空所有主题及订阅关系
        broker6.Clear();
        #endregion

        return service;
    }

    private static async Task<MqttTcpClient> CreateClient()
    {
        #region 创建Mqtt客户端
        var client = new MqttTcpClient();

        //配置
        var config = new TouchSocketConfig();
        config.SetRemoteIPHost("tcp://127.0.0.1:7789");

        //配置Mqtt连接参数
        #region Mqtt客户端配置连接选项
        config.SetMqttConnectOptions(options =>
        {
            options.ClientId = Guid.NewGuid().ToString("N");
            options.UserName = "TestUser";
            options.Password = "TestPassword";
            options.ProtocolName = "MQTT";
            options.Version = MqttProtocolVersion.V311;
            options.KeepAlive = 60;//此处的KeepAlive仅仅会将数值传递给服务器，客户端并不会发送心跳数据，如有需要请启用mqtt断线重连兼心跳插件
            options.CleanSession = true;
            options.UserProperties = new[]
            {
                    new MqttUserProperty("key1","value1"),
                    new MqttUserProperty("key2","value2")
            };
        });
        #endregion

        //配置容器
        config.ConfigureContainer(a =>
        {
            a.AddConsoleLogger();//添加一个日志注入
        });

        //配置插件
        #region Mqtt客户端配置插件
        config.ConfigurePlugins(a =>
        {
            //添加自定义插件
            //...

            a.Add<MqttServerReceivedPlugin>();//添加接收消息插件
            a.Add<MqttConnectionPlugin>();//添加连接插件
        });
        #endregion


        #region Mqtt客户端自动重连配置
        config.ConfigurePlugins(a =>
        {
            //此处如果是Tcp协议的Mqtt客户端，则使用MqttTcpClient
            //如果是WebSocket协议的Mqtt客户端，则使用MqttWebSocketClient
            a.UseReconnection<MqttTcpClient>(options =>
            {
                options.PollingInterval = TimeSpan.FromSeconds(5);//轮询检验间隔为5秒

                //启用断线重连兼心跳插件后，Mqtt客户端会自动发送心跳包以维持连接
                //使用的大概逻辑是：
                //1.每隔PollingInterval时间检验一次连接状态。
                //2.如果连接断开，则执行重连操作。
                //3.每隔pingInterval的时间，发送一次ping报文。
                //4.如果在activeTimeSpan（30秒）内，有数据收发，则也会认为在活。
                options.UseMqttCheckAction(activeTimeSpan: TimeSpan.FromSeconds(10), pingInterval: TimeSpan.FromSeconds(3));
            });
        });
        #endregion

        //载入配置
        await client.SetupAsync(config);

        await client.ConnectAsync();//连接
        #endregion

        return client;
    }
}

#region Mqtt连接插件
internal class MqttConnectionPlugin : PluginBase, IMqttConnectingPlugin, IMqttConnectedPlugin
{
    public async Task OnMqttConnected(IMqttSession client, MqttConnectedEventArgs e)
    {
        Console.WriteLine("Client Connected");
        await e.InvokeNext();
    }

    public async Task OnMqttConnecting(IMqttSession client, MqttConnectingEventArgs e)
    {
        Console.WriteLine("Client Connecting");
        await e.InvokeNext();
    }
}
#endregion

#region Mqtt客户端接收发布消息
internal class MqttClientReceivedPlugin : PluginBase, IMqttReceivedPlugin
{
    public async Task OnMqttReceived(IMqttSession client, MqttReceivedEventArgs e)
    {
        var mqttMessage = e.MqttMessage;
        Console.WriteLine("Client Reved:" + mqttMessage);

        //订阅消息的主题
        var topicName = mqttMessage.TopicName;

        //订阅消息的Qos级别
        var qosLevel = mqttMessage.QosLevel;

        //订阅消息的Payload
        var payload = mqttMessage.Payload;
        await e.InvokeNext();
    }
}
#endregion

#region Mqtt客户端通过插件接收所有消息
internal class MqttClientReceivingPlugin : PluginBase, IMqttReceivingPlugin
{
    public async Task OnMqttReceiving(IMqttSession client, MqttReceivingEventArgs e)
    {
        Console.WriteLine("Client Reving:" + e.MqttMessage.MessageType);
        await e.InvokeNext();
    }
}
#endregion

#region Mqtt服务器通过插件接收所有消息
internal class MqttServerReceivingPlugin : PluginBase, IMqttReceivingPlugin
{
    public async Task OnMqttReceiving(IMqttSession client, MqttReceivingEventArgs e)
    {
        switch (e.MqttMessage)
        {
            case MqttSubscribeMessage message:
                {
                    //订阅消息
                    Console.WriteLine("Reving:" + e.MqttMessage.MessageType);

                    foreach (var subscribeRequest in message.SubscribeRequests)
                    {
                        var topic = subscribeRequest.Topic;
                        var qosLevel = subscribeRequest.QosLevel;
                        //或者其他属性
                        Console.WriteLine($"Subscribe Topic:{topic},QosLevel:{qosLevel}");
                    }
                    break;
                }
            case MqttUnsubscribeMessage message:
                {

                    //取消订阅消息
                    Console.WriteLine("Reving:" + e.MqttMessage.MessageType);
                    foreach (var topic in message.TopicFilters)
                    {
                        //取消订阅的主题
                        Console.WriteLine($"Unsubscribe Topic:{topic}");
                    }
                    break;
                }
            default:
                break;
        }
        Console.WriteLine("Server Reving:" + e.MqttMessage.MessageType);
        await e.InvokeNext();
    }
}
#endregion


#region Mqtt服务器接收发布消息
internal class MqttServerReceivedPlugin : PluginBase, IMqttReceivedPlugin
{
    public async Task OnMqttReceived(IMqttSession client, MqttReceivedEventArgs e)
    {
        var mqttMessage = e.MqttMessage;
        Console.WriteLine("Server Reved:" + mqttMessage);

        //订阅消息的主题
        var topicName = mqttMessage.TopicName;

        //订阅消息的Qos级别
        var qosLevel = mqttMessage.QosLevel;

        //订阅消息的Payload
        var payload = mqttMessage.Payload;
        await e.InvokeNext();
    }
}
#endregion
