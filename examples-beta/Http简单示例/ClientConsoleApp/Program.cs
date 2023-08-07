using System;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace ClientConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("http://localhost:7219")))
                .Connect();//先做连接

            {
                var request = new HttpRequest();
                request.InitHeaders()
                    .SetUrl("/WeatherForecast")
                    .SetHost(client.RemoteIPHost.Host)
                    .AsGet();

                var respose = client.Request(request, timeout: 1000 * 10);
                Console.WriteLine(respose.GetBody());
            }

            {
                var request = new HttpRequest();
                request.InitHeaders()
                    .SetUrl("/WeatherForecast")
                    .SetHost(client.RemoteIPHost.Host)
                    .AsGet();

                var respose = client.Request(request, timeout: 1000 * 10);

                var buffer = new byte[1024 * 64];
                while (true)
                {
                    int r = respose.Read(buffer, 0, buffer.Length);
                    if (r==0)
                    {
                        break;
                    }
                    Console.WriteLine(r);
                }
                Console.WriteLine(respose.GetBody());
            }
        }
    }
}