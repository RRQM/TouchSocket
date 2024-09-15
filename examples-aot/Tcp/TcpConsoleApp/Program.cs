using System.Collections;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = CreateService();
            var client = CreateClient();
            Console.WriteLine("输入任意内容，回车发送");
            while (true)
            {
                client.Send(Console.ReadLine());
            }
        }

        private static TcpService CreateService()
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置
                .SetRegistrator(new MyContainer())
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                    a.RegisterSingleton(service);//将服务器以单例注入。便于插件或其他地方获取。
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<ClosePlugin>();
                    a.Add<TcpServiceReceivedPlugin>();
                    a.Add<MyServicePluginClass>();
                    //a.Add();//此处可以添加插件
                }));
            service.Start();//启动
            return service;
        }

        private static TcpClient CreateClient()
        {
            var tcpClient = new TcpClient();
            tcpClient.Received = (client, e) =>
            {
                //从服务器收到信息
                var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);

                tcpClient.Logger.Info($"客户端接收到信息：{mes}");
                return EasyTask.CompletedTask;
            };

            //载入配置
            tcpClient.Setup(new TouchSocketConfig()
                .SetRegistrator(new MyContainer())
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .ConfigurePlugins(a =>
                {
                    a.UseTcpReconnection()
                    .UsePolling(TimeSpan.FromSeconds(1));
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));
            tcpClient.Connect();
            tcpClient.Logger.Info("客户端成功连接");
            return tcpClient;
        }
    }

    /// <summary>
    /// IOC容器
    /// </summary>
    [AddSingletonInject(typeof(IPluginManager), typeof(PluginManager))]
    [AddSingletonInject(typeof(ILog), typeof(LoggerGroup))]
    [GeneratorContainer]
    public partial class MyContainer : ManualContainer
    {
    }

    [AutoInjectForSingleton]
    internal partial class MyServicePluginClass : PluginBase
    {
        [GeneratorPlugin(typeof(IServerStartedPlugin))]
        public Task OnServerStarted(IServiceBase sender, ServiceStateEventArgs e)
        {
            if (sender is ITcpService service)
            {
                foreach (var item in service.Monitors)
                {
                    ConsoleLogger.Default.Info($"iphost={item.Option.IpHost}");
                }
            }
            if (e.ServerState == ServerState.Running)
            {
                ConsoleLogger.Default.Info($"服务器成功启动");
            }
            else
            {
                ConsoleLogger.Default.Info($"服务器启动失败，状态：{e.ServerState}，异常：{e.Exception}");
            }
            return e.InvokeNext();
        }

        [GeneratorPlugin(typeof(IServerStopedPlugin))]
        public Task OnServerStoped(IServiceBase sender, ServiceStateEventArgs e)
        {
            Console.WriteLine("服务已停止");
            return e.InvokeNext();
        }
    }

    [AutoInjectForSingleton]
    partial class TcpServiceReceivedPlugin : PluginBase
    {
        [GeneratorPlugin(typeof(ITcpReceivedPlugin))]
        public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
        {
            //从客户端收到信息
            var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
            if (mes == "close")
            {
                throw new CloseException(mes);
            }
            client.Logger.Info($"已从{client.GetIPPort()}接收到信息：{mes}");

            if (client is not ITcpSessionClient sessionClient)
            {
                return;
            }

            await sessionClient.SendAsync(mes);//将收到的信息直接返回给发送方

            //await sessionClient.SendAsync("id", mes);//将收到的信息返回给特定ID的客户端

            //注意，此处是使用的当前客户端的接收线程做发送，实际使用中不可以这样做。不然一个客户端阻塞，将导致本客户端无法接收数据。
            var ids = sessionClient.Service.GetIds();
            foreach (var clientId in ids)//将收到的信息返回给在线的所有客户端。
            {
                if (clientId != sessionClient.Id)//不给自己发
                {
                    await sessionClient.SendAsync(clientId, mes);
                }
            }

            //await Task.Delay(1000);
        }
    }

    /// <summary>
    /// 应一个网友要求，该插件主要实现，在接收数据时如果触发<see cref="CloseException"/>异常，则断开连接。
    /// </summary>
    [AutoInjectForSingleton]
    partial class ClosePlugin : PluginBase
    {
        private readonly ILog m_logger;

        public ClosePlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        [GeneratorPlugin(typeof(ITcpReceivedPlugin))]
        public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
        {
            try
            {
                await e.InvokeNext();
            }
            catch (CloseException ex)
            {
                m_logger.Info("拦截到CloseException");
                await client.CloseAsync(ex.Message);
            }
            catch (Exception exx)
            {

            }
            finally
            {

            }
        }
    }

    class CloseException : Exception
    {
        public CloseException(string msg) : base(msg) { }
    }
}