using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;
using RRQMSocket;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TokenService<MySocketClient> tokenService = new TokenService<MySocketClient>();

            
            var config = new ServerConfig();
            config.SetValue(ServerConfig.BindIPHostProperty, new IPHost(7789))
                .SetValue(TcpServerConfig.MaxCountProperty, 10000)
                .SetValue(TokenServerConfig.VerifyTokenProperty, "123TT");

            var config1 = new TcpServerConfig();
            config1.BindIPHost = new IPHost(7789);
            config1.MaxCount = 10000;
            config1.ClearInterval = 60;
           
            tokenService.Setup(config);
            tokenService.Start();
        }
    }
    class MySocketClient : SocketClient
    {
        protected override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {

        }
    }
}
