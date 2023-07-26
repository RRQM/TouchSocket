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
            {
                var client = CreateWebApiClient();

                var sum1 = client.InvokeT<int>("GET:/Server/Sum?a={0}&b={1}", null, 10, 20);
                Console.WriteLine($"Get调用成功，结果：{sum1}");

                var sum2 = client.InvokeT<int>("POST:/Server/TestPost", null, new MyClass() { A = 10, B = 20 });
                Console.WriteLine($"Post调用成功，结果：{sum2}");

                var sum3 = client.TestPost(new MyClass() { A = 10, B = 20 });
                Console.WriteLine($"代理调用成功，结果：{sum3}");
            }

            {
                var client = CreateWebApiClientSlim();

                var sum1 = client.InvokeT<int>("GET:/Server/Sum?a={0}&b={1}", null, 10, 20);
                Console.WriteLine($"Get调用成功，结果：{sum1}");

                var sum2 = client.InvokeT<int>("POST:/Server/TestPost", null, new MyClass() { A = 10, B = 20 });
                Console.WriteLine($"Post调用成功，结果：{sum2}");

                //var sum3 = client.TestPost(new MyClass() { A = 10, B = 20 });
                //Console.WriteLine($"代理调用成功，结果：{sum3}");
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
            var client = new WebApiClientSlim();
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
        class MyWebApiPlugin : PluginBase, IWebApiPlugin<IRpcClient>
        {
            async Task IWebApiPlugin<IRpcClient>.OnRequest(IRpcClient client, WebApiEventArgs e)
            {
                if (e.IsHttpMessage)//发送的是HttpClient为主题
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

            async Task IWebApiPlugin<IRpcClient>.OnResponse(IRpcClient client, WebApiEventArgs e)
            {
                await e.InvokeNext();
            }
        }
    }
}