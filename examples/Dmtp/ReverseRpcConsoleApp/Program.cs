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

using System;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ReverseRpcConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //创建逻辑服务器
            var tcpDmtpService = CreateTcpDmtpService(7789);

            //创建逻辑客户端
            var client = CreateTcpDmtpClient();

            foreach (var item in tcpDmtpService.Clients)
            {
                client.Logger.Info(item.GetDmtpRpcActor().InvokeT<string>("SayHello", InvokeOption.WaitInvoke, "张三"));
                client.Logger.Info("调用完成");
            }

            Console.ReadKey();
        }

        private static TcpDmtpService CreateTcpDmtpService(int port)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"
                   });

            service.SetupAsync(config);
            service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
            return service;
        }

        private static TcpDmtpClient CreateTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                    a.AddRpcStore(store =>
                     {
                         store.RegisterServer<ReverseCallbackServer>();
                     });
                })
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc();
                 })
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                }));
            client.ConnectAsync();

            return client;
        }
    }

    public partial class ReverseCallbackServer : RpcServer
    {
        [DmtpRpc(MethodInvoke = true)]
        public string SayHello(string name)
        {
            return $"{name},hi";
        }
    }
}