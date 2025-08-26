// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.ComponentModel;
using System.Text.Json.Serialization;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.DmtpRpc.Generators;
using TouchSocket.Sockets;

namespace DmtpRpcConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                var service = await GetService();
                var client = await GetClient();

                while (true)
                {
                    Console.WriteLine("请输入账号和密码，用空格隔开。");
                    var strs = Console.ReadLine()?.Split(" ");
                    if (strs == null || strs.Length != 2)
                    {
                        Console.WriteLine("无效输入");
                        continue;
                    }

                    var invokeOption = new DmtpInvokeOption(5000)
                    {
                        FeedbackType = FeedbackType.WaitInvoke,
                        SerializationType = SerializationType.FastBinary
                    };

                    var result = await client.GetDmtpRpcActor().LoginAsync(strs[0], strs[1], invokeOption);
                    Console.WriteLine($"结果：{result.IsSuccess}");
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.Default.Exception(ex);
                Console.ReadKey();
            }

        }
        private static async Task<TcpDmtpClient> GetClient()
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();
                 })
                 .SetRemoteIPHost("127.0.0.1:7789")
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc()
                     .ConfigureDefaultSerializationSelector(selector =>
                     {
                         //配置Fast序列化器
                         selector.FastSerializerContext = new AppFastSerializerContext();

                         //配置System.Text.Json序列化器
                         selector.UseSystemTextJson(options =>
                         {
                             options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                         });
                     });
                 })
                 .SetDmtpOption(new DmtpOption()
                 {
                     VerifyToken = "Rpc"
                 }));
            await client.ConnectAsync();
            client.Logger.Info($"客户端已连接");
            return client;
        }
        private static async Task<TcpDmtpService> GetService()
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<IMyRpcServer, MyRpcServer>();//注册服务
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc()
                       .ConfigureDefaultSerializationSelector(selector =>
                       {
                           //配置Fast序列化器
                           selector.FastSerializerContext = new AppFastSerializerContext();

                           //配置System.Text.Json序列化器
                           selector.UseSystemTextJson(options =>
                           {
                               options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                           });
                       });
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Rpc"
                   });

            await service.SetupAsync(config);
            await service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}已启动");
            return service;
        }
    }

    #region Fast序列化

    [FastSerializable(typeof(MyResult))]
    [FastSerializable(typeof(IMyRpcServer), TypeMode.All)]//直接按类型，搜索其属性，字段，方法参数，方法返回值的类型进行注册序列化
    internal partial class AppFastSerializerContext : FastSerializerContext
    {

    }
    #endregion

    #region System.Text.Json序列化
    [JsonSerializable(typeof(MyResult))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {
    }

    #endregion

    #region Rpc服务
    [GeneratorRpcProxy]
    public interface IMyRpcServer : IRpcServer
    {
        [DmtpRpc(MethodInvoke = true)]
        [Description("登录")]//服务描述，在生成代理时，会变成注释。
        MyResult Login(string account, string password);

        [DmtpRpc(MethodInvoke = true)]
        [Description("注册")]
        RpcResponse Register(RpcRequest request);
    }

    public partial class MyRpcServer : IMyRpcServer
    {
        public MyResult Login(string account, string password)
        {
            if (account == "123" && password == "abc")
            {
                return new MyResult() { Account = account, IsSuccess = true };
            }

            return new MyResult() { Account = account, IsSuccess = false };
        }

        public RpcResponse Register(RpcRequest request)
        {
            return new RpcResponse() { MyProperty = request.MyProperty };
        }
    }

    [GeneratorPackage]
    public partial class MyResult : PackageBase
    {
        public string? Account { get; set; }
        public bool IsSuccess { get; set; }
    }

    [GeneratorPackage]
    public partial class RpcRequest : PackageBase
    {
        public int MyProperty { get; set; }
    }

    [GeneratorPackage]
    public partial class RpcResponse : PackageBase
    {
        public int MyProperty { get; set; }
    }
    #endregion
}