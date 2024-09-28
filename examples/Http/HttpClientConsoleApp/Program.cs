//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
            ConsoleAction consoleAction = new ConsoleAction();

            consoleAction.Add("1", "Get字符串", GetString);
            consoleAction.Add("2", "Get数组", GetBytesArray);
            consoleAction.Add("3", "Get文件", GetFile);
            consoleAction.Add("4", "自定义请求", Request1);
            consoleAction.Add("5", "自定义请求，并持续读取", Request2);
            consoleAction.Add("6", "自定义请求，并持续写入", BigWrite);

            consoleAction.ShowAll();

            await consoleAction.RunCommandLineAsync();
        }

        private static async Task BigWrite()
        {
            var client = await GetHttpClient();

            //创建一个请求
            var request = new HttpRequest(client);
            request.InitHeaders()
                .SetContentLength(100 * 1000)
                .SetUrl("/bigwrite")
                .SetHost(client.RemoteIPHost.Host)
                .AsPost();

            var task = client.RequestAsync(request, 1000 * 10);

            for (int i = 0; i < 100; i++)
            {
                await request.WriteAsync(new byte[1000]);
            }

            using (var responseResult = await task)
            {
                var response = responseResult.Response;
            }
            Console.WriteLine("完成");
        }

        private static async Task Request2()
        {
            var client = await GetHttpClient();
            //创建一个请求
            var request = new HttpRequest(client);
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

        private static async Task Request1()
        {
            var client = await GetHttpClient();
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

        private static async Task GetString()
        {
            var client = await GetHttpClient();
            //直接发起一个Get请求，然后返回Body字符串。
            var body = await client.GetStringAsync("/WeatherForecast");
        }

        private static async Task GetFile()
        {
            var client = await GetHttpClient();
            //直接发起一个Get请求文件，然后写入到流中。
            using (var stream = File.Create("1.txt"))
            {
                await client.GetFileAsync("/WeatherForecast", stream);
            }
        }

        private static async Task GetBytesArray()
        {
            var client = await GetHttpClient();

            //直接发起一个Get请求，然后返回Body数组。
            var bodyBytes = await client.GetByteArrayAsync("/WeatherForecast");
        }

        private static async Task<HttpClient> GetHttpClient()
        {
            var client = new HttpClient();

            var config = new TouchSocketConfig();
            config.SetRemoteIPHost("http://localhost:7789");

            //配置config
            await client.SetupAsync(config);
            await client.ConnectAsync();//先做连接

            return client;
        }
    }
}