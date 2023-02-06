using SerializationSelectorClassLibrary;
using System.ComponentModel;
using System.Threading.Channels;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace SerializationSelectorConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();//下列代码会用到源代码生成，所以启用企业版支持。
            }
            catch
            {

            }

            StartServer();

            var client = CreateClient();

            InvokeOption invokeOption = new InvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                SerializationType = (SerializationType)4,
                Timeout = 1000 * 10
            };

            var msg = client.Login(new LoginModel() { Account = "Account", Password = "Password" }, invokeOption);
            Console.WriteLine(msg);
            Console.ReadKey();
        }

        static TcpTouchRpcClient CreateClient()
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetSerializationSelector(new MemoryPackSerializationSelector())
                .SetVerifyToken("TouchRpc"));
            client.Connect();
            return client;
        }

        static void StartServer()
        {
            var service = new TcpTouchRpcService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .SetSerializationSelector(new MemoryPackSerializationSelector())
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigureRpcStore(a =>
                   {
                       a.RegisterServer<MyRpcServer>();
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

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
        [TouchRpc]
        public string Login(LoginModel loginModel)
        {
            return $"{loginModel.Account}-{loginModel.Password}";
        }
    }

    [GeneratorRpcProxy("SerializationSelectorConsoleApp.MyRpcServer")]
    public interface IMyRpcServer
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [Description("登录")]
        [GeneratorRpcMethod]
        string Login(LoginModel loginModel);
    }
}