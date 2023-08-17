using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;

namespace RpcFactoryConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var clientFactory = CreateTcpClientFactory();

            var client = clientFactory.GetTransferClient();

            try
            {
                // client.GetDmtpRpcActor().Invoke();//这里可以让得到的通讯单体进行业务交流
            }
            finally
            {
                //重中之重，必须要释放单体
                clientFactory.ReleaseTransferClient(client);
            }
        }

        private static TcpDmtpClientFactory CreateTcpClientFactory()
        {
            var clientFactory = new TcpDmtpClientFactory()
            {
                MinCount = 5,//最小数量，在主连接器成功建立以后，会检测可用连接是否大于该值，否的话会自动建立。
                MaxCount = 10,//最大数量，当超过该数量的连接后，会等待指定时间，或者永久等待。
                OnGetTransferConfig = () => //配置辅助通信
                {
                    return new TouchSocketConfig()
                       .SetRemoteIPHost("tcp://127.0.0.1:7789")
                       .ConfigurePlugins(a =>
                       {
                           a.UseDmtpRpc();
                       });
                }
            };

            clientFactory.MainConfig
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetRemoteIPHost("tcp://127.0.0.1:7789");//配置主通信

            //检测主通信器连接，然后如果没有连接，会自动建立连接
            var result = clientFactory.CheckStatus();
            return clientFactory;
        }

        private static HttpDmtpClientFactory CreateHttpClientFactory()
        {
            var clientFactory = new HttpDmtpClientFactory()
            {
                MinCount = 5,//最小数量，在主连接器成功建立以后，会检测可用连接是否大于该值，否的话会自动建立。
                MaxCount = 10,//最大数量，当超过该数量的连接后，会等待指定时间，或者永久等待。
                OnGetTransferConfig = () => //配置辅助通信
                {
                    return new TouchSocketConfig()
                       .SetRemoteIPHost("tcp://127.0.0.1:7789");
                }
            };

            clientFactory.MainConfig
                    .SetRemoteIPHost("tcp://127.0.0.1:7789");//配置主通信

            //检测主通信器连接，然后如果没有连接，会自动建立连接
            var result = clientFactory.CheckStatus();
            return clientFactory;
        }
    }
}