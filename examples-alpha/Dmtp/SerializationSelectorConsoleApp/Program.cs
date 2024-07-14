//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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
            client.SetupAsync(new TouchSocketConfig()
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
            client.ConnectAsync();
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

            service.SetupAsync(config);
            service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}已启动");
        }
    }

    public partial class MyRpcServer : RpcServer
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