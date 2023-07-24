using System;
using System.Threading.Tasks;
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
            HttpService service = new HttpService();

            service.Setup(new TouchSocketConfig()
                .ConfigurePlugins(a => 
                {
                    a.UseXmlRpc()
                    .SetXmlRpcUrl("/xmlRpc")
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<XmlServer>();
                    });
                    a.Add<MyPlugin>();
                })
                .SetListenIPHosts(new IPHost[] { new IPHost(7706) }))
                .Start();

            Console.WriteLine("服务器已启动");
            Console.ReadKey();
        }
    }

    public class MyPlugin : PluginBase,IHttpPostPlugin<IHttpSocketClient>
    {
        public async Task OnHttpPost(IHttpSocketClient client, HttpContextEventArgs e)
        {
            string s = e.Context.Request.GetBody();
            await e.InvokeNext();
        }
    }

    public class XmlServer : RpcServer
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