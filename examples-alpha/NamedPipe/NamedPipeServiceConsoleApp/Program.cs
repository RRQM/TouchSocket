using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeServiceConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = CreateService();

            Console.ReadKey();
        }

        private static NamedPipeService CreateService()
        {
            var service = new NamedPipeService();
            service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
            service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
            service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接
            service.SetupAsync(new TouchSocketConfig()//载入配置
                .SetPipeName("touchsocketpipe")//设置命名管道名称
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
                }));
            service.StartAsync();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }

    internal class MyNamedPipePlugin : PluginBase, INamedPipeConnectedPlugin, INamedPipeClosedPlugin, INamedPipeReceivedPlugin
    {
        private readonly ILog m_logger;

        public MyNamedPipePlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnNamedPipeClosed(INamedPipeSession client, ClosedEventArgs e)
        {
            this.m_logger.Info("Closed");
            await e.InvokeNext();
        }

        public async Task OnNamedPipeConnected(INamedPipeSession client, ConnectedEventArgs e)
        {
            this.m_logger.Info("Connected");
            await e.InvokeNext();
        }


        public async Task OnNamedPipeReceived(INamedPipeSession client, ReceivedDataEventArgs e)
        {
            this.m_logger.Info(e.ByteBlock.ToString());

            if (client is INamedPipeSessionClient sessionClient)
            {
                await sessionClient.SendAsync(e.ByteBlock.Memory);
            }

            await e.InvokeNext();
        }
    }
}