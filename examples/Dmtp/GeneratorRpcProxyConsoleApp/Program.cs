using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.Generators;
using TouchSocket.Sockets;

namespace GeneratorRpcProxyConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //创建服务器
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"//设定连接口令，作用类似账号密码
                   });

            service.Setup(config);
            service.Start();

            service.Logger.Info($"{service.GetType().Name}已启动");

            //创建客户端
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                }));
            client.Connect();

            //此处的Login方法则是vs源代码自动生成的，可以f12查看。
            Console.WriteLine(client.GetDmtpRpcActor().Login("123", "abc"));
            Console.ReadKey();
        }
    }

    public class MyRpcServer : RpcServer
    {
        [DmtpRpc]
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
    internal interface Test
    {
        [Description("这是登录方法")]//该作用是生成注释
        [DmtpRpc]//表明该方法应该被代理，也可以通过参数，直接设置调用键
        bool Login(string account, string password);
    }
}