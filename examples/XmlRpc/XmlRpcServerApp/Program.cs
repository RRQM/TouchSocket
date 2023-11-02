using System;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.XmlRpc;

namespace XmlRpcServerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new HttpService();

            service.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseXmlRpc()
                    .SetXmlRpcUrl("/xmlRpc")
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<XmlServer>();

#if DEBUG
                        File.WriteAllText("../../../RpcProxy.cs", store.GetProxyCodes("RpcProxy", new Type[] { typeof(XmlRpcAttribute) }));
                        ConsoleLogger.Default.Info("成功生成代理");
#endif
                    });
                })
                .SetListenIPHosts(7789));
            service.Start();

            service.Logger.Info("服务器已启动");
            Console.ReadKey();
        }
    }


    public class XmlServer : RpcServer
    {
        [XmlRpc(true)]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        [XmlRpc(true)]
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