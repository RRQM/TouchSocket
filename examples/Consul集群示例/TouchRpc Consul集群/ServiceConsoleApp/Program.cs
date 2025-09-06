//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Consul;
using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using TouchSocket.XmlRpc;

namespace ServiceConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("输入本地监听端口");
        var port = int.Parse(Console.ReadLine());

        //此处直接建立HttpDmtpService。
        //此组件包含Http所有功能，可以承载JsonRpc、XmlRpc、WebSocket、Dmtp等等。
        var service = await new TouchSocketConfig()
            .SetListenIPHosts(new IPHost[] { new IPHost(port) })
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<MyServer>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
                a.UseXmlRpc().SetXmlRpcUrl("/xmlrpc");
                a.UseWebApi();

                //添加WebSocket功能
                a.UseWebSocket(options =>
                {
                    options.SetUrl("/ws");//设置url直接可以连接。
                    options.SetAutoPong(true);//当收到ping报文时自动回应pong
                });

                a.Add<MyWebSocketPlug>();//添加WebSocket业务数据接收插件
                a.Add<MyWebSocketCommand>();//添加WebSocket快捷实现，常规WS客户端发送文本“Add 10 20”即可得到30。
            })
            .BuildServiceAsync<HttpDmtpService>();

        service.Logger.Info("Http服务器已启动");
        service.Logger.Info($"WS插件已加载，使用 ws://127.0.0.1:{port}/ws 连接");
        service.Logger.Info("WS命令行插件已加载，使用WS发送文本“Add 10 20”获取答案");

        service.Logger.Info($"jsonrpc插件已加载，使用 Http://127.0.0.1:{port}/jsonrpc +JsonRpc规范调用");
        service.Logger.Info($"xmlrpc插件已加载，使用 Http://127.0.0.1:{port}/xmlrpc +XmlRpc规范调用");
        service.Logger.Info("WebApi插件已加载");
        service.Logger.Info("RPC注册完成。");

        RegisterConsul(port);
        service.Logger.Info("Consul已成功注册");

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

internal partial class MyServer : SingletonRpcServer
{
    [WebApi(Method = HttpMethodType.Get)]
    [XmlRpc]
    [JsonRpc]
    [DmtpRpc]
    public string SayHello(string name)
    {
        return $"{name},RRQM says hello to you.";
    }

    /// <summary>
    /// 健康检测
    /// </summary>
    /// <returns></returns>
    [Router("/api/health")]
    [WebApi(Method = HttpMethodType.Get)]
    public string Health()
    {
        return "ok";
    }
}

/// <summary>
/// WS命令行执行
/// </summary>
internal class MyWebSocketCommand : WebSocketCommandLinePlugin
{
    public MyWebSocketCommand(ILog logger) : base(logger)
    {
    }

    public int AddCommand(int a, int b)
    {
        return a + b;
    }
}

/// <summary>
/// WS收到数据等业务。
/// </summary>
internal class MyWebSocketPlug : PluginBase, IWebSocketConnectedPlugin, IWebSocketReceivedPlugin
{
    public async Task OnWebSocketConnected(IWebSocket client, HttpContextEventArgs e)
    {
        if (client.Client is IHttpSessionClient socketClient)
        {
            socketClient.Logger.Info($"WS客户端连接，ID={socketClient.Id}，IPHost={socketClient.IP}:{socketClient.Port}");
        }

        await e.InvokeNext();
    }

    public async Task OnWebSocketReceived(IWebSocket client, WSDataFrameEventArgs e)
    {
        if (client.Client is IHttpSessionClient socketClient)
        {
            if (e.DataFrame.Opcode == WSDataType.Text)
            {
                socketClient.Logger.Info($"WS Msg={e.DataFrame.ToText()}");
            }
        }

        await e.InvokeNext();
    }
}