using SerializationSelectorClassLibrary;
using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.Generators;
using TouchSocket.Sockets;

namespace SerializationSelectorConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StartServer();

            var client = CreateClient();

            InvokeOption invokeOption = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = (SerializationType)4,
                Timeout = 1000 * 10
            };

            var msg = client.GetDmtpRpcActor().Login(new LoginModel() { Account = "Account", Password = "Password" }, invokeOption);
            Console.WriteLine("调用成功，结果：" + msg);
            Console.ReadKey();
        }

        private static TcpDmtpClient CreateClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc()
                        .SetSerializationSelector(new MemoryPackSerializationSelector());
                })
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                }));
            client.Connect();
            return client;
        }

        private static void StartServer()
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc()
                       .SetSerializationSelector(new MemoryPackSerializationSelector());
                   })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();
                       });
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"
                   });

            service.Setup(config);
            service.Start();

            service.Logger.Info($"{service.GetType().Name}已启动");
        }
    }

    public class MyRpcServer : RpcServer
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [Description("登录")]
        [DmtpRpc]
        public string Login(LoginModel loginModel)
        {
            return $"{loginModel.Account}-{loginModel.Password}";
        }
    }

    [GeneratorRpcProxy(Prefix = "SerializationSelectorConsoleApp.MyRpcServer")]
    public interface IMyRpcServer
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [Description("登录")]
        [DmtpRpc]
        string Login(LoginModel loginModel);
    }
}