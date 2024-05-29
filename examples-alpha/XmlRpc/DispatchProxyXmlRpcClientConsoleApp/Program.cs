using TouchSocket.Sockets;
using TouchSocket.XmlRpc;

namespace DispatchProxyXmlRpcClientConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var rpc = MyXmlRpcDispatchProxy.Create<IXmlRpcServer, MyXmlRpcDispatchProxy>();
            while (true)
            {
                Console.WriteLine("请输入两个数，中间用空格隔开，回车确认");
                var str = Console.ReadLine();
                var strs = str.Split(' ');
                var a = int.Parse(strs[0]);
                var b = int.Parse(strs[1]);

                var sum = rpc.Sum(a, b);
                Console.WriteLine(sum);
            }
        }
    }

    /// <summary>
    /// 新建一个类，继承XmlRpcDispatchProxy，亦或者RpcDispatchProxy基类。
    /// 然后实现抽象方法，主要是能获取到调用的IRpcClient派生接口。
    /// </summary>
    internal class MyXmlRpcDispatchProxy : XmlRpcDispatchProxy
    {
        private readonly IXmlRpcClient m_client;

        public MyXmlRpcDispatchProxy()
        {
            this.m_client = GetXmlRpcClient();
        }

        public override IXmlRpcClient GetClient()
        {
            return this.m_client;
        }

        private static XmlRpcClient GetXmlRpcClient()
        {
            var jsonRpcClient = new XmlRpcClient();
            jsonRpcClient.ConnectAsync("http://127.0.0.1:7789/xmlRpc");
            Console.WriteLine("连接成功");
            return jsonRpcClient;
        }
    }

    internal interface IXmlRpcServer
    {
        [XmlRpc(true)]
        int Sum(int a, int b);
    }
}