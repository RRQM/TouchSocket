using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using WebApiProxy;

namespace WebApiClientApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //此处预设一个30秒超时的请求设定。
            var invokeOption_30s = new InvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                Timeout = 30 * 1000
            };

            {
                var client = CreateWebApiClient();

                var sum1 = client.InvokeT<int>("GET:/Server/Sum?a={0}&b={1}", invokeOption_30s, 10, 20);
                Console.WriteLine($"Get调用成功，结果：{sum1}");

                var sum2 = client.InvokeT<int>("POST:/Server/TestPost", invokeOption_30s, new MyClass() { A = 10, B = 20 });
                Console.WriteLine($"Post调用成功，结果：{sum2}");

                var sum3 = client.TestPost(new MyClass() { A = 10, B = 20 }, invokeOption_30s);
                Console.WriteLine($"代理调用成功，结果：{sum3}");
            }

            {
                var client = CreateWebApiClientSlim();

                var sum1 = client.InvokeT<int>("GET:/Server/Sum?a={0}&b={1}", invokeOption_30s, 10, 20);
                Console.WriteLine($"Get调用成功，结果：{sum1}");

                var sum2 = client.InvokeT<int>("POST:/Server/TestPost", invokeOption_30s, new MyClass() { A = 10, B = 20 });
                Console.WriteLine($"Post调用成功，结果：{sum2}");

                var sum3 = client.TestPost(new MyClass() { A = 10, B = 20 }, invokeOption_30s);
                Console.WriteLine($"代理调用成功，结果：{sum3}");
            }


            Console.ReadKey();
        }

        private static WebApiClient CreateWebApiClient()
        {
            var client = new WebApiClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.Add<MyWebApiPlugin>();
                }));
            client.Connect();
            Console.WriteLine("连接成功");
            return client;
        }

        private static WebApiClientSlim CreateWebApiClientSlim()
        {
            var client = new WebApiClientSlim(new System.Net.Http.HttpClient());
            client.Setup(new TouchSocketConfig()
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
        class MyWebApiPlugin : PluginBase, IWebApiPlugin<IWebApiClientBase>
        {
            public async Task OnRequest(IWebApiClientBase client, WebApiEventArgs e)
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

            public async Task OnResponse(IWebApiClientBase client, WebApiEventArgs e)
            {
                await e.InvokeNext();
            }
        }
    }
}