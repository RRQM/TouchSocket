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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using WebApiProxy;

namespace WebApiClientApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await TestHttpClient();

            //此处预设一个30秒超时的请求设定。
            var invokeOption_30s = new InvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                Timeout = 30 * 1000
            };

            {
                var client = await CreateWebApiClient();

                var sum1 = client.InvokeT<int>("GET:/ApiServer/Sum?a={0}&b={1}", invokeOption_30s, 10, 20);
                Console.WriteLine($"Get调用成功，结果：{sum1}");

                var sum2 = client.InvokeT<int>("POST:/ApiServer/TestPost", invokeOption_30s, new MyClass() { A = 10, B = 20 });
                Console.WriteLine($"Post调用成功，结果：{sum2}");

                var sum3 = client.TestPost(new MyClass() { A = 10, B = 20 }, invokeOption_30s);
                Console.WriteLine($"代理调用成功，结果：{sum3}");
            }

            {
                var client =await CreateWebApiClientSlim();

                var sum1 = client.InvokeT<int>("GET:/ApiServer/Sum?a={0}&b={1}", invokeOption_30s, 10, 20);
                Console.WriteLine($"Get调用成功，结果：{sum1}");

                var sum2 = client.InvokeT<int>("POST:/ApiServer/TestPost", invokeOption_30s, new MyClass() { A = 10, B = 20 });
                Console.WriteLine($"Post调用成功，结果：{sum2}");

                var sum3 = client.TestPost(new MyClass() { A = 10, B = 20 }, invokeOption_30s);
                Console.WriteLine($"代理调用成功，结果：{sum3}");
            }

            Console.ReadKey();
        }

        private static async Task<HttpClient> TestHttpClient()
        {
            var client = new HttpClient();
            await client.ConnectAsync("127.0.0.1:7789");
            Console.WriteLine("连接成功");

            string responseString = await client.GetStringAsync("/ApiServer/Sum?a=10&b=20");
            return client;
        }

        private static async Task<WebApiClient> CreateWebApiClient()
        {
            var client = new WebApiClient();
            await client.SetupAsync(new TouchSocketConfig()
                 .SetRemoteIPHost("127.0.0.1:7789")
                 .ConfigurePlugins(a =>
                 {
                     a.Add<MyWebApiPlugin>();
                 }));
            await client.ConnectAsync();
            Console.WriteLine("连接成功");
            return client;
        }

        private static async Task<WebApiClientSlim> CreateWebApiClientSlim()
        {
            var client = new WebApiClientSlim(new System.Net.Http.HttpClient());
            await client.SetupAsync(new TouchSocketConfig()
                 .SetRemoteIPHost("http://127.0.0.1:7789")
                 .ConfigurePlugins(a =>
                 {
                     a.Add<MyWebApiPlugin>();
                 }));

            return client;
        }

        /// <summary>
        /// 此处可以做WebApi的请求之前和之后的拦截。
        /// </summary>
        private class MyWebApiPlugin : PluginBase, IWebApiRequestPlugin, IWebApiResponsePlugin
        {

            public async Task OnWebApiRequest(IWebApiClientBase client, WebApiEventArgs e)
            {
                if (e.IsHttpMessage)//发送的是System.Net.Http.HttpClient为通讯主体
                {
                    //添加一个header
                    e.RequestMessage.Headers.Add("Token", "123123");
                }
                else
                {
                    //添加一个header
                    e.Request.Headers.Add("Token", "123123");
                }

                await e.InvokeNext();
            }

            public async Task OnWebApiResponse(IWebApiClientBase client, WebApiEventArgs e)
            {
                await e.InvokeNext();
            }
        }
    }
}