using RpcProxy;
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

            var result2 = client.Sum(10, 20);//此Sum方法是服务端生成的代理。
            Console.WriteLine($"代理调用，返回结果:{result2}");

            Console.ReadKey();
        }

        private static XmlRpcClient GetXmlRpcClient()
        {
            var jsonRpcClient = new XmlRpcClient();
            jsonRpcClient.Setup("http://127.0.0.1:7789/xmlRpc");
            jsonRpcClient.Connect();
            Console.WriteLine("连接成功");
            return jsonRpcClient;
        }
    }
}