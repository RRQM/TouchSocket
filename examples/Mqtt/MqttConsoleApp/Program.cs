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

            //取消订阅
            //var mqttUnsubAckMessage = await client.UnsubscribeAsync(new MqttUnsubscribeMessage(topic1,topic2));

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

        #region Mqtt服务器停止服务器
        await service.StopAsync();
        #endregion

        #region Mqtt服务器释放资源
        service.Dispose();
        #endregion
    }

    private static async Task<MqttTcpService> CreateService()
    {
        #region 创建Tcp协议的Mqtt服务器
        var service = new MqttTcpService();

        //配置
        var config = new TouchSocketConfig();

        config.SetListenIPHosts("tcp://127.0.0.1:7789");
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
        var client = new MqttTcpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("tcp://127.0.0.1:7789")
            //.SetNoDelay(true)
            .SetMqttConnectOptions(options =>
            {
                options.ClientId = "TestClient";
                options.UserName = "TestUser";
                options.Password = "TestPassword";
                options.ProtocolName = "MQTT";
                options.Version = MqttProtocolVersion.V311;
                options.KeepAlive = 60;
                options.CleanSession = true;
                options.UserProperties = new[]
                {
                    new MqttUserProperty("key1","value1"),
                    new MqttUserProperty("key2","value2")
                };
            })
            .ConfigurePlugins(a =>
            {
                a.Add<MyMqttReceivedPlugin>();//添加自定义插件
                ValueCounter counter = new()
                {
                    OnPeriod = (c) =>
                    {
                        Console.WriteLine($"Received:{c}");
                    },
                    Period = TimeSpan.FromSeconds(1)
                };

                a.AddMqttConnectingPlugin(async (mqttSession, e) =>
                {
                    Console.WriteLine($"Client Connecting:{e.ConnectMessage.ClientId}");
                    await e.InvokeNext();
                });

                a.AddMqttConnectedPlugin(async (mqttSession, e) =>
                {
                    Console.WriteLine($"Client Connected:{e.ConnectMessage.ClientId}");
                    await e.InvokeNext();
                });

                a.AddMqttReceivingPlugin(async (mqttSession, e) =>
                {
                    //var message = e.MqttMessage;

                    //counter.Increment();
                    await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                });

                a.AddMqttReceivedPlugin(async (mqttSession, e) =>
                {
                    var message = e.MqttMessage;
                    var s = message.Retain;
                    counter.Increment();
                    await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                });

                a.AddMqttClosingPlugin(async (mqttSession, e) =>
                {
                    Console.WriteLine($"Client Closing:{e.MqttMessage.MessageType}");
                    await e.InvokeNext();
                });

                a.AddMqttClosedPlugin(async (mqttSession, e) =>
                {
                    Console.WriteLine($"Client Closed:{e.Message}");
                    await e.InvokeNext();
                });

            }));//载入配置

        await client.ConnectAsync();//连接
        return client;
    }
}

#region Mqtt服务器通过插件接收所有消息
internal class MyMqttReceivingPlugin : PluginBase, IMqttReceivingPlugin
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
        Console.WriteLine("Reving:" + e.MqttMessage.MessageType);
        await e.InvokeNext();
    }
}
#endregion


#region Mqtt服务器接收发布消息
internal class MyMqttReceivedPlugin : PluginBase, IMqttReceivedPlugin
{
    public async Task OnMqttReceived(IMqttSession client, MqttReceivedEventArgs e)
    {
        var mqttMessage = e.MqttMessage;
        Console.WriteLine("Reved:" + mqttMessage);

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
