using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeServiceConsoleApp
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var service = CreateService();

            Console.ReadKey();
        }

        private static NamedPipeService CreateService()
        {
            var service = new NamedPipeService();
            service.Connecting = (client, e) => { };//有客户端正在连接
            service.Connected = (client, e) => { };//有客户端成功连接
            service.Disconnected = (client, e) => { };//有客户端断开连接
            service.Setup(new TouchSocketConfig()//载入配置
                .SetPipeName("TouchSocketPipe")//设置命名管道名称
                .SetNamedPipeListenOptions(list =>
                {
                    //如果想实现多个命名管道的监听，即可这样设置，一直Add即可。
                    list.Add(new NamedPipeListenOption()
                    {
                        Adapter = () => new NormalDataHandlingAdapter(),
                        Name = "TouchSocketPipe2"//管道名称
                    });

                    list.Add(new NamedPipeListenOption()
                    {
                        Adapter = () => new NormalDataHandlingAdapter(),
                        Name = "TouchSocketPipe3"//管道名称
                    });
                })
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyNamedPipePlugin>();
                    //a.Add();//此处可以添加插件
                }))
                .Start();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }

    class MyNamedPipePlugin : PluginBase, INamedPipeConnectedPlugin, INamedPipeDisconnectedPlugin, INamedPipeReceivedPlugin
    {
        private readonly ILog m_logger;

        public MyNamedPipePlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnNamedPipeConnected(INamedPipeClientBase client, ConnectedEventArgs e)
        {
            m_logger.Info("Connected");
            await e.InvokeNext();
        }

        public async Task OnNamedPipeDisconnected(INamedPipeClientBase client, DisconnectEventArgs e)
        {
            m_logger.Info("Disconnected");
            await e.InvokeNext();
        }

        public async Task OnNamedPipeReceived(INamedPipeClientBase client, ReceivedDataEventArgs e)
        {
            m_logger.Info(e.ByteBlock.ToString());
            client.Send(e.ByteBlock);
            await e.InvokeNext();
        }
    }
}