using Consul;
using System;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Http.WebSockets.Plugins;
using TouchSocket.Rpc;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Rpc.XmlRpc;
using TouchSocket.Sockets;

namespace ServiceConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("输入本地监听端口");
            int port = int.Parse(Console.ReadLine());

            //此处直接建立HttpTouchRpcService。
            //此组件包含Http所有功能，可以承载JsonRpc、XmlRpc、WebSocket、TouchRpc等等。
            var service = new TouchSocketConfig()
                .UsePlugin()
                .SetBufferLength(1024 * 64)
                .SetThreadCount(50)
                .SetReceiveType(ReceiveType.Auto)
                .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger<ConsoleLogger>();
                })
                .BuildWithHttpTouchRpcService();

            service.Logger.Message("Http服务器已启动");

            service.AddPlugin<WebSocketServerPlugin>().//添加WebSocket功能
                    SetWSUrl("/ws");
            service.Logger.Message($"WS插件已加载，使用 ws://127.0.0.1:{port}/ws 连接");

            service.AddPlugin<MyWebSocketPlug>();//添加WebSocket业务数据接收插件
            service.AddPlugin<MyWebSocketCommand>();//添加WebSocket快捷实现，常规WS客户端发送文本“Add 10 20”即可得到30。
            service.Logger.Message("WS命令行插件已加载，使用WS发送文本“Add 10 20”获取答案");

            IRpcParser jsonRpcParser = service.AddPlugin<JsonRpcParserPlugin>()
                 .SetJsonRpcUrl("/jsonrpc");
            service.Logger.Message($"jsonrpc插件已加载，使用 Http://127.0.0.1:{port}/jsonrpc +JsonRpc规范调用");

            IRpcParser xmlRpcParser = service.AddPlugin<XmlRpcParserPlugin>()
                .SetXmlRpcUrl("/xmlrpc");
            service.Logger.Message($"jsonrpc插件已加载，使用 Http://127.0.0.1:{port}/xmlrpc +XmlRpc规范调用");

            IRpcParser webApiParser = service.AddPlugin<WebApiParserPlugin>();
            service.Logger.Message("WebApi插件已加载");

            RpcStore rpcStore = new RpcStore(new TouchSocket.Core.Dependency.Container());
            rpcStore.AddRpcParser("httpTouchRpcService", service);
            rpcStore.AddRpcParser("jsonRpcParser", jsonRpcParser);
            rpcStore.AddRpcParser("xmlRpcParser", xmlRpcParser);
            rpcStore.AddRpcParser("webApiParser", webApiParser);
            rpcStore.RegisterServer<MyServer>();
            service.Logger.Message("RPC注册完成。");

            //rpcStore.ShareProxy(new IPHost(8848));
            //rpcStore.ProxyUrl = "/proxy";
            //service.Logger.Message("RPC代理文件已分享，使用 Http://127.0.0.1:8848/proxy?proxy=all 获取");

            RegisterConsul(port);
            service.Logger.Message("Consul已成功注册");

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.WriteLine("按ESC键退出。");
            }
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
                Interval = TimeSpan.FromSeconds(10),//间隔固定的时间访问一次，https://127.0.0.1:7789/api/Health
                HTTP = $"http://127.0.0.1:{port}/api/Health",//健康检查地址
                Timeout = TimeSpan.FromSeconds(5)
            };

            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { httpCheck },
                ID = Guid.NewGuid().ToString(),
                Name = "RRQMService" + port,
                Address = "127.0.0.1",
                Port = port
            };

            consulClient.Agent.ServiceRegister(registration).Wait();//注册服务

            //consulClient.Agent.ServiceDeregister(registration.ID).Wait();//registration.ID是guid
            //当服务停止时需要取消服务注册，不然，下次启动服务时，会再注册一个服务。
            //但是，如果该服务长期不启动，那consul会自动删除这个服务，大约2，3分钟就会删了
        }
    }

    internal class MyServer : RpcServer
    {
        [WebApi(HttpMethodType.GET)]
        [XmlRpc]
        [JsonRpc]
        [TouchRpc]
        public string SayHello(string name)
        {
            return $"{name},RRQM says hello to you.";
        }

        /// <summary>
        /// 健康检测
        /// </summary>
        /// <returns></returns>
        [Router("/api/health")]
        [WebApi(HttpMethodType.GET)]
        public string Health()
        {
            return "ok";
        }
    }

    /// <summary>
    /// WS命令行执行
    /// </summary>
    internal class MyWebSocketCommand : WSCommandLinePlugin
    {
        public int AddCommand(int a, int b)
        {
            return a + b;
        }
    }

    /// <summary>
    /// WS收到数据等业务。
    /// </summary>
    internal class MyWebSocketPlug : WebSocketPluginBase
    {
        protected override void OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {
            SocketClient socketClient = (SocketClient)client;

            client.Logger.Message($"WS客户端连接，ID={socketClient.ID}，IPHost={client.IP}:{client.Port}");
            base.OnHandshaked(client, e);
        }

        protected override void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Text)
            {
                client.Logger.Message($"WS Msg={e.DataFrame.ToText()}");
            }

            base.OnHandleWSDataFrame(client, e);
        }
    }
}