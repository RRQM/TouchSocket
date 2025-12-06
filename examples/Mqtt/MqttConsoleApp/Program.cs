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
            a.UseReconnection<MqttTcpClient>(options =>
            {
                options.PollingInterval = TimeSpan.FromSeconds(5);//轮询检验间隔为5秒

                //启用断线重连兼心跳插件后，Mqtt客户端会自动发送心跳包以维持连接
                //使用的大概逻辑是：
                //1.每隔PollingInterval时间检验一次连接状态。
                //2.如果连接断开，则执行重连操作。
                //3.如果连接在线，则检查距离上次活动时间是否超过activeTimeSpan，如果超过，则发送ping包。
                //4.如果ping包在pingTimeout时间内没有收到响应，则认为连接断开，执行重连操作。
                options.UseMqttCheckAction(activeTimeSpan: TimeSpan.FromSeconds(10), pingTimeout: TimeSpan.FromSeconds(3));
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
