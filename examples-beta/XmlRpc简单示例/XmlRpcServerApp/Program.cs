using System;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.XmlRpc;
using TouchSocket.Sockets;
using TouchSocket.XmlRpc;

namespace XmlRpcServerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RpcStore rpcStore = new RpcStore(new Container());

            //添加解析器，解析器根据传输协议，序列化方式的不同，调用RPC服务
            rpcStore.AddRpcParser("xmlRpcParser ", CreateXmlRpcRpcParser());

            //注册当前程序集的所有服务
            rpcStore.RegisterAllServer();

            //分享代理，代理文件可通过RRQMTool远程获取。
            rpcStore.ShareProxy(new IPHost(8848));

            //或者直接本地导出代理文件。
            //RpcProxyInfo proxyInfo = rpcService.GetProxyInfo(RpcType.RRQMRPC, "RPC");
            //string codeString = CodeGenerator.ConvertToCode("RRQMProxy", proxyInfo.Codes);

            Console.WriteLine("服务器已启动");
            Console.ReadKey();
        }

        private static IRpcParser CreateXmlRpcRpcParser()
        {
            HttpService service = new HttpService();

            service.Setup(new TouchSocketConfig().UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7706) }))
                .Start();

            service.AddPlugin<MyPlugin>();

            return service.AddPlugin<XmlRpcParserPlugin>()
                 .SetXmlRpcUrl("/xmlRpc");
        }
    }

    public class MyPlugin : HttpPluginBase
    {
        protected override void OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {
            string s = e.Context.Request.GetBody();
            base.OnPost(client, e);
        }
    }

    public class Server : RpcServer
    {
        [XmlRpc]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        [XmlRpc]
        public int TestClass(MyClass myClass)
        {
            return myClass.A + myClass.B;
        }
    }

    public class MyClass
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}