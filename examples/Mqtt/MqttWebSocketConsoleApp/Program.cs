using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;

namespace MqttWebSocketConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
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

                    if (!client.Online)
                    {
                        Console.WriteLine("客户端已掉线");
                        return;
                    }

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

            #region WebSocket协议的Mqtt服务器获取IMqttService实例
            var mqttService = service.Resolver.Resolve<IMqttWebSocketService>();
            var mqttBroker = mqttService.MqttBroker;
            #endregion


            while (true)
            {
                Console.ReadKey();
            }
        }

        private static async Task<MqttWebSocketClient> CreateClient()
        {
            #region 创建WebSocket协议的Mqtt客户端
            var client = new MqttWebSocketClient();
            await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:1883/mqtt")
                .SetMqttConnectOptions(options =>
                {
                    options.ClientId = Guid.NewGuid().ToString();
                    options.UserName = "admin";
                    options.Password = "123456";
                    options.ProtocolName = "Mqtt";
                    options.Version = MqttProtocolVersion.V500;
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
                    //使用重连插件
                    a.UseReconnection<MqttWebSocketClient>(options =>
                    {
                        options.PollingInterval = TimeSpan.FromSeconds(10);
                        options.UseMqttCheckAction();
                    });
                }));//载入配置

            await client.ConnectAsync(10000000);//连接

            Console.WriteLine("客户端已连接");
            #endregion

            return client;
        }

        private static async Task<HttpService> CreateService()
        {
            #region 创建WebSocket协议的Mqtt服务器           
            var service = new HttpService();
            await service.SetupAsync(new TouchSocketConfig()//载入配置
                 .SetListenIPHosts("tcp://127.0.0.1:1883")//可以同时监听两个地址
                 .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                 {
                     //1).添加Mqtt服务容器，可以从IOC中通过IMqttWebSocketService接口获取服务实例
                     a.AddMqttWebSocketService();

                     a.AddConsoleLogger();
                 })
                 .ConfigurePlugins(a =>
                 {
                     //2).使用MqttWebSocket协议
                     a.UseMqttWebSocket("/mqtt");
                 }));

            await service.StartAsync();//启动

            Console.WriteLine("服务器已启动");
            #endregion

            return service;
        }
    }
}
