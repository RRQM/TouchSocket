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

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpChannelConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await GetTcpDmtpService();
        var client = await GetTcpDmtpClient();

        var consoleAction = new ConsoleAction();

        consoleAction.Add("1", "测试完成写入", async () => { await RunComplete(client); });
        consoleAction.Add("2", "测试Hold", async () => { await RunHoldOn(client); });

        consoleAction.ShowAll();

        await consoleAction.RunCommandLineAsync();
    }

    private static async Task RunHoldOn(IDmtpActorObject client)
    {
        //HoldOn的使用，主要是解决同一个通道中，多个数据流传输的情况。

        //1.创建通道，同时支持通道路由和元数据传递
        using (var channel = await client.CreateChannelAsync())
        {
            ConsoleLogger.Default.Info($"通道创建成功，即将写入");
            var bytes = new byte[100];

            for (var i = 0; i < 100; i++)//循环100次
            {
                for (var j = 0; j < 10; j++)
                {
                    //2.持续写入数据
                    await channel.WriteAsync(bytes);
                }
                //3.在某个阶段完成数据传输时，可以调用HoldOn
                await channel.HoldOnAsync("等一下下");
            }
            //4.在写入完成后调用终止指令。例如：Complete、Cancel、HoldOn、Dispose等
            await channel.CompleteAsync("我完成了");

            ConsoleLogger.Default.Info("通道写入结束");
        }
    }

    private static async Task RunComplete(IDmtpActorObject client)
    {
        #region TcpDmtpClient创建Channel
        var count = 1024;

        //1.创建通道，同时支持通道路由和元数据传递

        using var cts = new CancellationTokenSource(1000 * 10);
        var metadata = new Metadata();

        using (var channel = await client.CreateChannelAsync(metadata, cts.Token))
        {
            ConsoleLogger.Default.Info($"通道创建成功");
            var bytes = new byte[1000];
            for (var i = 0; i < count; i++)
            {
                if (!channel.CanWrite)
                {
                    //通道不可写，即客户端可能已经断开连接
                    break;
                }
                using var writeCts = new CancellationTokenSource(1000 * 5);
                //2.持续写入数据
                await channel.WriteAsync(bytes, writeCts.Token);
            }

            //3.在写入完成后调用终止指令。例如：Complete、Cancel、HoldOn、Dispose等
            await channel.CompleteAsync("我完成了");
            ConsoleLogger.Default.Info("通道写入结束");
        }
        #endregion

    }

    private static async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var client = await new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7789")
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Channel";
               })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
        #region Dmtp心跳重连插件
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlugin>();

                    //使用重连
                    a.UseReconnection<TcpDmtpClient>(options =>
                    {
                        //重连间隔3秒
                        options.PollingInterval = TimeSpan.FromSeconds(3);

                        //使用Dmtp状态、心跳等联合检测作为连接状态检查依据。
                        options.UseDmtpCheckAction();
                    });
                })
        #endregion

               .BuildClientAsync<TcpDmtpClient>();

        client.Logger.Info("连接成功");
        return client;
    }

    private static async Task<TcpDmtpService> GetTcpDmtpService()
    {
        var service = new TcpDmtpService();

        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.Add<MyPlugin>();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Channel";//连接验证口令。
               });

        await service.SetupAsync(config);
        await service.StartAsync();
        service.Logger.Info("服务器成功启动");
        return service;
    }
}

#region TcpDmtpService使用插件订阅Channel
internal class MyPlugin : PluginBase, IDmtpCreatedChannelPlugin
{
    private readonly ILog m_logger;

    public MyPlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    public async Task OnDmtpCreatedChannel(IDmtpActorObject client, CreateChannelEventArgs e)
    {
        //1.订阅通道
        if (client.TrySubscribeChannel(e.ChannelId, out var channel))
        {
            //2.准备处理通道数据
            using (channel)
            {
                this.m_logger.Info("通道开始接收");

                long count = 0;

                //3.持续读取数据
                while (channel.CanRead)
                {
                    //4.设置读取超时
                    using var cts = new CancellationTokenSource(10 * 1000);

                    //5.读取到的数据
                    var memory = await channel.ReadAsync(cts.Token);

                    //6.可以判断通道状态
                    if (channel.Status == ChannelStatus.HoldOn)
                    {
                        Console.WriteLine($"HoldOn:{channel.LastOperationMes}");
                    }

                    //这里处理数据
                    count += memory.Length;
                    this.m_logger.Info($"通道已接收：{count}字节");
                }

                this.m_logger.Info($"通道接收结束，状态={channel.Status}，短语={channel.LastOperationMes}，共接收{count / (1048576.0):0.00}Mb字节");
            }
        }

        await e.InvokeNext();
    }
}
#endregion
