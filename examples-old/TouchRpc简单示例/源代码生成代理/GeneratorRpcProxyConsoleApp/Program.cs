using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.Rpc.Generators;

namespace GeneratorRpcProxyConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch
            {


            }
            //创建服务器
            var service = new TcpTouchRpcService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddFileLogger();
                   })
                   .ConfigureRpcStore(a =>
                   {
                       a.RegisterServer<MyRpcServer>();
                   })
                   .SetVerifyToken("TouchRpc");//设定连接口令，作用类似账号密码

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");

            //创建客户端
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();

            Console.WriteLine(client.Login("123", "abc"));//此处的Login方法则是vs源代码自动生成的，可以f12查看。
            Console.ReadKey();
        }
    }

    public class MyRpcServer : RpcServer
    {
        [TouchRpc]
        public bool Login(string account, string password)
        {
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// GeneratorRpcProxy的标识，表明这个接口应该被生成其他源代码。
    /// ConsoleApp2.MyRpcServer参数是整个rpc调用的前缀，即：除方法名的所有，包括服务的类名。
    /// </summary>
    [GeneratorRpcProxy(Prefix = "GeneratorRpcProxyConsoleApp.MyRpcServer")]
    interface Test
    {
        [Description("这是登录方法")]//该作用是生成注释
        [GeneratorRpcMethod]//表明该方法应该被代理，也可以通过参数，直接设置调用键
        public bool Login(string account, string password);
    }
}