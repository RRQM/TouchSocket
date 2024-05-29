using System;
using System.IO;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace ClientConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var client = new HttpClient();
            await client.ConnectAsync("http://localhost:7219");//先做连接

            {
                //直接发起一个Get请求，然后返回Body字符串。
                var body = await client.GetStringAsync("/WeatherForecast");
            }

            {
                //直接发起一个Get请求，然后返回Body数组。
                var bodyBytes = await client.GetByteArrayAsync("/WeatherForecast");
            }

            {
                //直接发起一个Get请求文件，然后写入到流中。
                using (var stream=File.Create("1.txt"))
                {
                    await client.GetFileAsync("/WeatherForecast",stream);
                }
               
            }

            {
                //创建一个请求
                var request = new HttpRequest();
                request.InitHeaders()
                    .SetUrl("/WeatherForecast")
                    .SetHost(client.RemoteIPHost.Host)
                    .AsGet();


                using (var responseResult = await client.RequestAsync(request, 1000 * 10))
                {
                    var response = responseResult.Response;
                    Console.WriteLine(await response.GetBodyAsync());//将接收的数据，一次性转为utf8编码的字符串
                }
            }

            {
                //创建一个请求
                var request = new HttpRequest();
                request.InitHeaders()
                    .SetUrl("/WeatherForecast")
                    .SetHost(client.RemoteIPHost.Host)
                    .AsGet();


                using (var responseResult = await client.RequestAsync(request, 1000 * 10))
                {
                    var response = responseResult.Response;

                    while (true)
                    {
                        using (var blockResult = await response.ReadAsync())
                        {
                            if (blockResult.IsCompleted)
                            {
                                //数据读完成
                                break;
                            }

                            //每次读到的数据
                            var memory = blockResult.Memory;
                            Console.WriteLine(memory.Length);
                        }
                    }
                }
            }
        }
    }
}