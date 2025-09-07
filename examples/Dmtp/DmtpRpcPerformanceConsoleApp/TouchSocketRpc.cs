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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcPerformanceConsoleApp
{
    public static class TouchSocketRpc
    {
        public static void StartServer()
        {
            var host = Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddServiceHostedService<ITcpDmtpService, TcpDmtpService>(config =>
            {
                config.SetListenIPHosts(7789)
                       .ConfigureContainer(a =>
                       {
                           a.AddConsoleLogger();

                           a.AddRpcStore(store =>
                           {
                               store.RegisterServer<TestController>();
                           });
                       })
                       .ConfigurePlugins(a =>
                       {
                           a.UseDmtpRpc();
                       })
                       .SetDmtpOption(options=>
                       {
                           options.VerifyToken = "Rpc";//设定连接口令，作用类似账号密码
                       });
            });
        })
        .Build();

            host.RunAsync();
        }

        public static async Task StartSumClient(int count)
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                  .ConfigurePlugins(a =>
                  {
                      a.UseDmtpRpc();
                  })
                  .SetRemoteIPHost("127.0.0.1:7789")
                  .SetDmtpOption(options=>
                  {
                      options.VerifyToken = "Rpc";
                  }));
            await client.ConnectAsync();

            var timeSpan = TimeMeasurer.Run(async () =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 0; i < count; i++)
                {
                    var rs = await actor.InvokeTAsync<Int32>("Sum", InvokeOption.WaitInvoke, i, i);
                    if (rs != i + i)
                    {
                        Console.WriteLine("调用结果不一致");
                    }
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }

        public static async Task StartGetBytesClient(int count)
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc();
                 })
                 .SetRemoteIPHost("127.0.0.1:7789")
                 .SetDmtpOption(options=>
                 {
                     options.VerifyToken = "Rpc";
                 }));
            await client.ConnectAsync();

            var timeSpan = TimeMeasurer.Run(async () =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 1; i < count; i++)
                {
                    var rs = await actor.InvokeTAsync<byte[]>("GetBytes", InvokeOption.WaitInvoke, i);//测试10k数据
                    if (rs.Length != i)
                    {
                        Console.WriteLine("调用结果不一致");
                    }
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }

        public static async Task StartBigStringClient(int count)
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc();
                 })
                 .SetRemoteIPHost("127.0.0.1:7789")
                 .SetDmtpOption(options=>
                 {
                     options.VerifyToken = "Rpc";
                 }));
            await client.ConnectAsync();


            var timeSpan = TimeMeasurer.Run(async () =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 0; i < count; i++)
                {
                    var rs = await actor.InvokeTAsync<string>("GetBigString", InvokeOption.WaitInvoke);
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }
    }
}
