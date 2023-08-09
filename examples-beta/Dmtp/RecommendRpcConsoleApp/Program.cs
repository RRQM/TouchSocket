﻿using RpcImplementationClassLibrary;
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
            try
            {
                Enterprise.ForTest();
            }
            catch
            {
            }
            CodeGenerator.AddIgnoreProxyAssembly(typeof(Program).Assembly);
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddFileLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc()
                           .ConfigureRpcStore(store =>
                           {
                               //此处使用限定名称，因为源代码生成时，也会生成TouchSocket.Rpc.Generators.IUserServer的接口
                               store.RegisterServer<RpcClassLibrary.ServerInterface.IUserServer, UserServer>();
                           });
                   })
                   .SetVerifyToken("Rpc");//设定连接口令，作用类似账号密码

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");

            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetVerifyToken("Rpc"));
            client.Connect();

            //Login即为在RpcClassLibrary中自动生成的项目
            var response = client.GetDmtpRpcActor().Login(new RpcClassLibrary.Models.LoginRequest() { Account = "Account", Password = "Account" });
            Console.WriteLine(response.Result);
            Console.ReadKey();
        }
    }
}