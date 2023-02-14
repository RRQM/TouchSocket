using RpcClassLibrary.ServerInterface;
using RpcImplementationClassLibrary;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Rpc.Generators;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RecommendRpcConsoleApp
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
                       //此处使用限定名称，因为源代码生成时，也会生成TouchSocket.Rpc.Generators.IUserServer的接口
                       a.RegisterServer<RpcClassLibrary.ServerInterface.IUserServer, UserServer>();
                   })
                   .SetVerifyToken("TouchRpc");//设定连接口令，作用类似账号密码

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");

            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();

            //Loging即为在RpcClassLibrary中自动生成的项目
            var response = client.Login(new RpcClassLibrary.Models.LoginRequest() { Account= "Account",Password= "Account" });
            Console.WriteLine(response.Result);
            Console.ReadKey();
        }
    }
}