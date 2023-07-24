using System;
using TouchSocket.Rpc;
using TouchSocket.WebApi;
using WebApiProxy;

namespace WebApiClientApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = CreateWebApiClient();

            int sum1 = client.InvokeT<int>("GET:/Server/Sum?a={0}&b={1}", null, 10, 20);
            Console.WriteLine($"Get调用成功，结果：{sum1}");

            int sum2 = client.InvokeT<int>("POST:/Server/TestPost", null, new MyClass() { A = 10, B = 20 });
            Console.WriteLine($"Post调用成功，结果：{sum2}");

            int sum3 = client.TestPost(new MyClass() { A = 10, B = 20 });
            Console.WriteLine($"代理调用成功，结果：{sum3}");
            Console.ReadKey();
        }

        private static WebApiClient CreateWebApiClient()
        {
            WebApiClient client = new WebApiClient();
            client.Setup("127.0.0.1:7789");
            client.Connect();
            Console.WriteLine("连接成功");
            return client;
        }
    }
}