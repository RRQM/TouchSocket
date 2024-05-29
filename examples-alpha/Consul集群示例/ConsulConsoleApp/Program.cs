using Consul;
using System;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ConsulConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new TcpService();
            service.Connecting = (client, e) =>
            {
                service.Logger.Info("Connecting");
                return Task.CompletedTask;
            };//有客户端正在连接

            service.Connected = (client, e) =>
            {
                service.Logger.Info("Connected");
                return Task.CompletedTask;
            };//有客户端连接

            service.Closed = (client, e) =>
            {
                service.Logger.Info("Disconnected");
                return Task.CompletedTask;
            };//有客户端断开连接

            service.Received = (client, e) =>
            {
                //从客户端收到信息

                var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");

                //client.Send(mes);//将收到的信息直接返回给发送方
                return Task.CompletedTask;
            };

            service.SetupAsync(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                );
            service.StartAsync();//启动

            RegisterConsul(7789);
            Console.ReadKey();
        }

        /// <summary>
        /// 注册Consul，使用该功能时，请先了解Consul，然后配置基本如下。
        /// </summary>
        public static void RegisterConsul(int port)
        {
            var consulClient = new ConsulClient(p => { p.Address = new Uri($"http://127.0.0.1:8500"); });//请求注册的 Consul 地址
                                                                                                         //这里的这个ip 就是本机的ip，这个端口8500 这个是默认注册服务端口
            var httpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                Interval = TimeSpan.FromSeconds(10),
                //HTTP = $"http://127.0.0.1:{port}/api/Health",//健康检查地址
                TCP = $"127.0.0.1:{port}",
                Timeout = TimeSpan.FromSeconds(5)
            };

            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { httpCheck },
                ID = Guid.NewGuid().ToString(),
                Name = "RRQM Tcp Service" + port,
                Address = "127.0.0.1",
                Port = port
            };

            consulClient.Agent.ServiceRegister(registration).Wait();//注册服务

            //consulClient.Agent.ServiceDeregister(registration.ID).Wait();//registration.ID是guid
            //当服务停止时需要取消服务注册，不然，下次启动服务时，会再注册一个服务。
            //但是，如果该服务长期不启动，那consul会自动删除这个服务，大约2，3分钟就会删了
        }
    }
}