using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpAspnetcoreConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //WebSocketDmtpClient连接
            var websocketDmtpClient = new WebSocketDmtpClient();
            websocketDmtpClient.Setup(new TouchSocketConfig()
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                })
                .SetRemoteIPHost("ws://localhost:5174/WebSocketDmtp"));
            await websocketDmtpClient.ConnectAsync();
            Console.WriteLine("WebSocketDmtpClient连接成功");

            //HttpDmtpClient连接
            var httpDmtpClient = new HttpDmtpClient();
            httpDmtpClient.Setup(new TouchSocketConfig()
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                })
                .SetRemoteIPHost("http://127.0.0.1:5174"));
            httpDmtpClient.Connect();
            Console.WriteLine("HttpDmtpClient连接成功");

            Console.ReadKey();
        }
    }
}
