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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ServiceConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var consoleAction = new ConsoleAction();
        consoleAction.Add("1", "以Received委托接收", RunClientForReceived);
        consoleAction.Add("2", "以Ipv6的Received委托接收", RunClientForReceivedWithIpv6);
        consoleAction.Add("3", "以ReadAsync异步阻塞接收", RunClientForReadAsync);

        var service = await CreateService();

        consoleAction.ShowAll();
        await consoleAction.RunCommandLineAsync();

    }

    private static async Task<TcpService> CreateService()
    {
        var service = new TcpService();
        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790, new IPHost(System.Net.IPAddress.IPv6Any, 7791))//同时监听两个地址
             .ConfigureContainer(a =>//容器的配置顺序应该在最前面
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })
             .ConfigurePlugins(a =>
             {
                 a.UseCheckClear()
                 .SetCheckClearType(CheckClearType.All)
                 .SetTick(TimeSpan.FromSeconds(60))
                 .SetOnClose(async (c, t) =>
                 {
                     await c.ShutdownAsync(System.Net.Sockets.SocketShutdown.Both);
                     await c.CloseAsync("超时无数据");
                 });

                 a.Add<ClosePlugin>();
                 a.Add<TcpServiceReceivedPlugin>();
                 a.Add<MyServicePluginClass>();
             }));
        await service.StartAsync();//启动
        return service;
    }

    /// <summary>
    /// 以Received异步委托接收数据
    /// </summary>
    private static async Task RunClientForReceived()
    {
        var client = new TcpClient();
        client.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到服务器
        client.Closed = (client, e) => { return EasyTask.CompletedTask; };//从服务器断开连接，当连接不成功时不会触发。
        client.Received = (client, e) =>
        {
            //从服务器收到信息
            var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"客户端接收到信息：{mes}");
            return EasyTask.CompletedTask;
        };

        await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .ConfigurePlugins(a =>
                {
                    a.UseTcpReconnection()
                    .UsePolling(TimeSpan.FromSeconds(1));
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));//载入配置
        await client.ConnectAsync();//连接
        client.Logger.Info("客户端成功连接");

        Console.WriteLine("输入任意内容，回车发送");
        while (true)
        {
            await client.SendAsync(Console.ReadLine());
        }
    }

    private static async Task RunClientForReceivedWithIpv6()
    {
        var client = new TcpClient();
        client.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到服务器
        client.Closed = (client, e) => { return EasyTask.CompletedTask; };//从服务器断开连接，当连接不成功时不会触发。
        client.Received = (client, e) =>
        {
            //从服务器收到信息
            var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"客户端接收到信息：{mes}");
            return EasyTask.CompletedTask;
        };

        await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("[::1]:7791"))
                .ConfigurePlugins(a =>
                {
                    a.UseTcpReconnection()
                    .UsePolling(TimeSpan.FromSeconds(1));
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));//载入配置
        await client.ConnectAsync();//连接
        client.Logger.Info("客户端成功连接");

        Console.WriteLine("输入任意内容，回车发送");
        while (true)
        {
            await client.SendAsync(Console.ReadLine());
        }
    }

    private static async Task RunClientForReadAsync()
    {
        var client = new TcpClient();
        await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .ConfigurePlugins(a =>
                {
                    a.UseTcpReconnection()
                    .UsePolling(TimeSpan.FromSeconds(1));
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));//载入配置
        await client.ConnectAsync();//连接
        client.Logger.Info("客户端成功连接");

        Console.WriteLine("输入任意内容，回车发送");
        //receiver可以复用，不需要每次接收都新建
        using (var receiver = client.CreateReceiver())
        {
            while (true)
            {
                client.Send(Console.ReadLine());

                //receiverResult必须释放
                using (var receiverResult = await receiver.ReadAsync(CancellationToken.None))
                {
                    if (receiverResult.IsCompleted)
                    {
                        //断开连接了
                    }

                    //从服务器收到信息。
                    var mes = receiverResult.ByteBlock.Span.ToString(Encoding.UTF8);
                    client.Logger.Info($"客户端接收到信息：{mes}");

                    //如果是适配器信息，则可以直接获取receiverResult.RequestInfo;
                }
            }
        }
    }
}

internal class MyTcpClient : TcpClientBase
{
    protected override Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        //此处逻辑单线程处理。

        //此处处理数据，功能相当于Received委托。
        var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
        Console.WriteLine($"已接收到信息：{mes}");

        return EasyTask.CompletedTask;
    }
}

internal class MyPluginClass : PluginBase
{
    protected override void Loaded(IPluginManager pluginManager)
    {
        pluginManager.Add<ITcpSession, ReceivedDataEventArgs>(typeof(ITcpReceivedPlugin), this.OnTcpReceived);
        base.Loaded(pluginManager);
    }

    private async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        await e.InvokeNext();
    }
}

internal class MyServicePluginClass : PluginBase, IServerStartedPlugin, IServerStoppedPlugin
{
    public Task OnServerStarted(IServiceBase sender, ServiceStateEventArgs e)
    {
        if (sender is ITcpService service)
        {
            foreach (var item in service.Monitors)
            {
                ConsoleLogger.Default.Info($"iphost={item.Option.IpHost}");
            }
        }
        if (e.ServerState == ServerState.Running)
        {
            ConsoleLogger.Default.Info($"服务器成功启动");
        }
        else
        {
            ConsoleLogger.Default.Info($"服务器启动失败，状态：{e.ServerState}，异常：{e.Exception}");
        }
        return e.InvokeNext();
    }

    public Task OnServerStopped(IServiceBase sender, ServiceStateEventArgs e)
    {
        Console.WriteLine("服务已停止");
        return e.InvokeNext();
    }
}

internal class TcpServiceReceiveAsyncPlugin : PluginBase, ITcpConnectedPlugin
{
    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        if (client is not ITcpSessionClient sessionClient)
        {
            await e.InvokeNext();
            return;
        }
        //receiver可以复用，不需要每次接收都新建
        using (var receiver = sessionClient.CreateReceiver())
        {
            while (true)
            {
                //receiverResult必须释放
                using (var receiverResult = await receiver.ReadAsync(CancellationToken.None))
                {
                    if (receiverResult.IsCompleted)
                    {
                        //断开连接了
                    }

                    //从服务器收到信息。
                    var mes = receiverResult.ByteBlock.Span.ToString(Encoding.UTF8);
                    client.Logger.Info($"客户端接收到信息：{mes}");

                    //如果是适配器信息，则可以直接获取receiverResult.RequestInfo;
                }
            }
        }
    }
}

internal class TcpServiceReceivedPlugin : PluginBase, ITcpReceivedPlugin
{
    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        //从客户端收到信息
        var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
        if (mes == "close")
        {
            throw new CloseException(mes);
        }
        client.Logger.Info($"已从{client.GetIPPort()}接收到信息：{mes}");

        if (client is ITcpSessionClient sessionClient)
        {
            await sessionClient.SendAsync(mes);//将收到的信息直接返回给发送方

            //sessionClient.Send("id",mes);//将收到的信息返回给特定ID的客户端

            //注意，此处是使用的当前客户端的接收线程做发送，实际使用中不可以这样做。不然一个客户端阻塞，将导致本客户端无法接收数据。
            //var ids = client.Service.GetIds();
            //foreach (var clientId in ids)//将收到的信息返回给在线的所有客户端。
            //{
            //    if (clientId != client.Id)//不给自己发
            //    {
            //        await client.Service.SendAsync(clientId, mes);
            //    }
            //}
        }



        await e.InvokeNext();
    }
}

/// <summary>
/// 应一个网友要求，该插件主要实现，在接收数据时如果触发<see cref="CloseException"/>异常，则断开连接。
/// </summary>
internal class ClosePlugin : PluginBase, ITcpReceivedPlugin
{
    private readonly ILog m_logger;

    public ClosePlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        try
        {
            await e.InvokeNext();
        }
        catch (CloseException ex)
        {
            this.m_logger.Info("拦截到CloseException");
            client.Close(ex.Message);
        }
        catch (Exception exx)
        {
        }
        finally
        {
        }
    }
}

internal class CloseException : Exception
{
    public CloseException(string msg) : base(msg)
    {
    }
}