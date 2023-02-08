using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RpcFactoryConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        private static TcpTouchRpcClientFactory CreateClientFactory()
        {
            TcpTouchRpcClientFactory clientFactory = new TcpTouchRpcClientFactory()
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

            Result result = clientFactory.CheckStatus();
            return clientFactory;
        }
    }
}