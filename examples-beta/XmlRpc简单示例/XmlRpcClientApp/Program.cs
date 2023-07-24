using RRQMProxy;
using System;
using TouchSocket.Rpc;
using TouchSocket.XmlRpc;

namespace XmlRpcClientApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = GetXmlRpcClient();

            //直接调用
            var result1 = client.InvokeT<int>("Sum", InvokeOption.WaitInvoke, 10, 20);
            Console.WriteLine($"直接调用，返回结果:{result1}");

            var server = new Server(client);
            var result2 = server.Sum(10, 20);
            Console.WriteLine($"代理调用，返回结果:{result2}");

            Console.ReadKey();
        }

        private static XmlRpcClient GetXmlRpcClient()
        {
            var jsonRpcClient = new XmlRpcClient();
            jsonRpcClient.Setup("http://127.0.0.1:7706/xmlRpc");
            jsonRpcClient.Connect();
            Console.WriteLine("连接成功");
            return jsonRpcClient;
        }
    }
}