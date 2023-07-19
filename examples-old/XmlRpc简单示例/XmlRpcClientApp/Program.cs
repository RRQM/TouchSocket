using RRQMProxy;
using System;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.XmlRpc;

namespace XmlRpcClientApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = GetXmlRpcClient();

            //直接调用
            int result1 = client.Invoke<int>("Sum", InvokeOption.WaitInvoke, 10, 20);
            Console.WriteLine($"直接调用，返回结果:{result1}");

            Server server = new Server(client);
            int result2 = server.Sum(10, 20);
            Console.WriteLine($"代理调用，返回结果:{result2}");

            Console.ReadKey();
        }

        private static XmlRpcClient GetXmlRpcClient()
        {
            XmlRpcClient jsonRpcClient = new XmlRpcClient();
            jsonRpcClient.Setup("http://127.0.0.1:7706/xmlRpc");
            jsonRpcClient.Connect();
            Console.WriteLine("连接成功");
            return jsonRpcClient;
        }
    }
}