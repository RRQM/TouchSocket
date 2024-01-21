using RpcImplementationClassLibrary;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.Generators;
using TouchSocket.Sockets;

namespace RecommendRpcConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddFileLogger();
                       a.AddRpcStore(store =>
                       {
                           ////此处使用限定名称，因为源代码生成时，也会生成TouchSocket.Rpc.Generators.IUserServer的接口
                           //store.RegisterServer<RpcClassLibrary.ServerInterface.IUserServer, UserServer>();

                           //此处使用的是源生成注册，具体可看文档》Rpc》注册服务
                           store.RegisterAllFromRpcImplementationClassLibrary();
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"
                   });//设定连接口令，作用类似账号密码

            service.Setup(config);
            service.Start();

            service.Logger.Info($"{service.GetType().Name}已启动");

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

            //Login即为在RpcClassLibrary中自动生成的项目
            var response = client.GetDmtpRpcActor().Login(new RpcClassLibrary.Models.LoginRequest() { Account = "Account", Password = "Account" });
            Console.WriteLine(response.Result);
            Console.ReadKey();
        }
    }
}