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
                .SetRemoteIPHost(new IPHost("http://localhost:7219")));
            client.Connect();//先做连接

            {
                //创建一个请求
                var request = new HttpRequest();
                request.InitHeaders()
                    .SetUrl("/WeatherForecast")
                    .SetHost(client.RemoteIPHost.Host)
                    .AsGet();

                var respose = client.Request(request, false,1000 * 10);
                Console.WriteLine(respose.GetBody());//将接收的数据，一次性转为utf8编码的字符串
            }

            {
                //创建一个请求
                var request = new HttpRequest();
                request.InitHeaders()
                    .SetUrl("/WeatherForecast")
                    .SetHost(client.RemoteIPHost.Host)
                    .AsGet();

                var respose = client.Request(request, false, 1000 * 10);

                //一次性接收字节
                if (respose.TryGetContent(out var content))
                {

                }

                //如果数据太大，可以一直read
                var buffer = new byte[1024 * 64];
                while (true)
                {
                    var r = respose.Read(buffer, 0, buffer.Length);
                    if (r == 0)
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