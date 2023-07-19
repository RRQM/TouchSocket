using System;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace PluginConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpService service = new TcpService();
            service.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })
                .UsePlugin()
                .ConfigureContainer(a=>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a => 
                {
                    a.Add<MyPlugin>();
                }))
                .Start();
            Console.ReadKey();
        }
    }

    internal class MyCommandLinePlugin : TcpCommandLinePlugin
    {
        public MyCommandLinePlugin(ILog logger) : base(logger)
        {
        }

        public int AddCommand(int a, int b)
        {
            return a + b;
        }
    }

    public class MyPlugin : TcpPluginBase<SocketClient>
    {
        public MyPlugin()
        {
            this.Order = 0;//此值表示插件的执行顺序，当多个插件并存时，该值越大，越在前执行。
        }
        protected override void OnConnecting(SocketClient client, OperationEventArgs e)
        {
            client.SetDataHandlingAdapter(new NormalDataHandlingAdapter());
            base.OnConnecting(client, e);
        }

        protected override void OnReceivedData(SocketClient client, ReceivedDataEventArgs e)
        {
            //这里处理数据接收
            //根据适配器类型，e.ByteBlock与e.RequestInfo会呈现不同的值，具体看文档=》适配器部分。
            ByteBlock byteBlock = e.ByteBlock;
            IRequestInfo requestInfo = e.RequestInfo;

            //e.Handled = true;//表示该数据已经被本插件处理，无需再投递到其他插件。
            base.OnReceivedData(client, e);
        }
    }
}