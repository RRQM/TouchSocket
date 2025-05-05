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
    static async Task Main(string[] args)
    {
        var service = await CreateService();

        Console.ReadKey();

        var client = await CreateClient();

        try
        {
            var topic = "topic1";

            var mqttSubscribeMessage = new MqttSubscribeMessage(new SubscribeRequest(topic, QosLevel.AtLeastOnce));

            var mqttSubAckMessage = await client.SubscribeAsync(mqttSubscribeMessage);

            //var mqttUnsubAckMessage = await client.UnsubscribeAsync(new MqttUnsubscribeMessage(topic));

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

            MqttPublishMessage message = new(topic, false, QosLevel.AtLeastOnce, Encoding.UTF8.GetBytes(
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
    }

    static async Task<MqttTcpService> CreateService()
    {
        var service = new MqttTcpService();
        await service.SetupAsync(new TouchSocketConfig()//载入配置
                                                        //.SetNoDelay(true)
             .SetListenIPHosts("tcp://127.0.0.1:7789")//可以同时监听两个地址
             .ConfigureContainer(a =>//容器的配置顺序应该在最前面
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })
             .ConfigurePlugins(a =>
             {
                 a.AddIdChangedPlugin(async (client, e) =>
                 {
                     Console.WriteLine($"IdChanged:{e.OldId}->{e.NewId}");
                     await e.InvokeNext();
                 });
                 //a.AddMqttReceivingPlugin(async (client, e) => 
                 //{
                 //    Console.WriteLine("Reving:"+ e.MqttMessage.MessageType);
                 //    await e.InvokeNext();
                 //});

                 //a.AddMqttReceivedPlugin(async (client, e) =>
                 //{
                 //    Console.WriteLine("Reved:"+ e.Message);
                 //    await e.InvokeNext();
                 //});

                 a.AddMqttConnectingPlugin(async (client, e) =>
                 {
                     Console.WriteLine($"Server Connecting:{e.ConnectMessage.ClientId}");
                     await e.InvokeNext();
                 });

                 a.AddMqttConnectedPlugin(async (client, e) =>
                 {
                     Console.WriteLine($"Server Connected:{e.ConnectMessage.ClientId}");
                     await e.InvokeNext();
                 });

                 a.AddMqttClosingPlugin(async (client, e) =>
                 {
                     Console.WriteLine($"Server Closing:{e.MqttMessage.MessageType}");
                     await e.InvokeNext();
                 });

                 a.AddMqttClosedPlugin(async (client, e) =>
                 {
                     Console.WriteLine($"Server Closed:{e.Message}");
                     await e.InvokeNext();
                 });
             }));

        await service.StartAsync();//启动

        return service;
    }

    static async Task<MqttTcpClient> CreateClient()
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
                    var message = e.Message;
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
