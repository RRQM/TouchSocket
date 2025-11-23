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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ServiceConsoleApp;

internal class CloseException : Exception
{
    public CloseException(string msg) : base(msg)
    {
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
            await client.CloseAsync(ex.Message);
        }
        catch (Exception)
        {
        }
        finally
        {
        }
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

internal class Program
{
    private static async Task CreateCustomService()
    {
        #region 创建MyService服务器
        var service = new MyService();
        service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
        service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
        service.Closing = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在断开连接，只有当主动断开时才有效。
        service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接

        #region Tcp服务器使用Received异步委托接收数据并回应
        service.Received = async (client, e) =>
        {
            #region 日志注入容器后使用示例
            //从客户端收到信息
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            service.Logger.Info($"服务器已从{client.Id}接收到信息：{mes}");
            #endregion

            //按字符发送给客户端
            await client.SendAsync(mes);

            //按字节数组发送给客户端
            //await client.SendAsync(new byte[] { 0, 1, 2, 3, 4 });
        };
        #endregion


        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//可以同时监听多个地址
             .ConfigureContainer(a =>//容器的配置
             {
                 #region 日志容器配置控制台日志
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                 #endregion
             })
             .ConfigurePlugins(a =>
             {
                 //a.Add();//此处可以添加插件
             }));

        await service.StartAsync();//启动
        #endregion
    }

    private static async Task CreateDefaultService()
    {
        #region 创建TcpService
        var service = new TcpService();
        service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
        service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
        service.Closing = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在断开连接，只有当主动断开时才有效。
        service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接

        #region Tcp服务器使用Received异步委托接收数据
        service.Received = async (client, e) =>
        {
            //从客户端收到信息
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"已从{client.Id}接收到信息：{mes}");

            //简单消除Task，当使用插件接收时，需要使用 await e.InvokeNext();来继续执行后续插件。
            await EasyTask.CompletedTask;
        };
        #endregion


        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//可以同时监听多个地址
             .ConfigureContainer(a =>//容器的配置
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })
             .ConfigurePlugins(a =>
             {
                 //a.Add();//此处可以添加插件
             }));

        await service.StartAsync();//启动
        #endregion

        #region Tcp服务器按目标Id直接回应
        await service.SendAsync("targetId", "消息内容");
        #endregion

        #region Tcp服务器按目标Id先查找再回应
        if (service.Clients.TryGetClient("targetId", out var tcpSessionClient))
        {
            await tcpSessionClient.SendAsync("消息内容");
        }
        #endregion

        #region Tcp清理当前所有连接
        await service.ClearAsync();
        #endregion

        #region Tcp清理指定连接
        foreach (var item in service.Clients)
        {
            if (item.Id == "targetId")
            {
                await item.CloseAsync();

                //在断开连接后，释放对象。
                item.Dispose();
            }
        }
        #endregion

        #region Tcp服务器停止监听
        foreach (var item in service.Monitors)
        {
            service.RemoveListen(item);
        }
        #endregion

        #region Tcp停止服务器
        await service.StopAsync();
        #endregion

        #region Tcp释放服务器资源
        service.Dispose();
        #endregion
    }

    private static async Task CreateDefaultService2()
    {
        #region 动态添加移除监听配置 {5,17}
        var service = new TcpService();
        await service.StartAsync();//直接启动，此处没有配置任何东西

        //在Service运行时，可以调用，直接添加监听
        service.AddListen(new TcpListenOption()
        {
            IpHost = 7791,
            Name = "server3",//名称用于区分监听
            ServiceSslOption = null,//可以针对当前监听，单独启用ssl加密
            Adapter = () => new FixedHeaderPackageAdapter(),//可以单独对当前地址监听，配置适配器
                                                            //还有其他可配置项，都是单独对当前地址有效。
        });

        //在Service运行时，可以调用，直接移除监听
        foreach (var item in service.Monitors)
        {
            service.RemoveListen(item);//在Service运行时，可以调用，直接移除现有监听
        }
        #endregion

        #region 服务器重置Id
        await service.ResetIdAsync("oldId", "newId");
        #endregion

        #region 服务器通过SessionClient重置Id
        if (service.TryGetClient("oldId", out var sessionClient))
        {
            await sessionClient.ResetIdAsync("newId");
        }
        #endregion
    }

    private static async Task CreateDefaultTcpClient()
    {
        #region 创建Tcp客户端
        var tcpClient = new TcpClient();
        tcpClient.Connecting = (client, e) => { return EasyTask.CompletedTask; };//即将连接到服务器，此时已经创建socket，但是还未建立tcp
        tcpClient.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到服务器
        tcpClient.Closing = (client, e) => { return EasyTask.CompletedTask; };//即将从服务器断开连接。此处仅主动断开才有效。
        tcpClient.Closed = (client, e) => { return EasyTask.CompletedTask; };//从服务器断开连接，当连接不成功时不会触发。
        #region Tcp客户端使用Received异步委托接收数据
        tcpClient.Received = (client, e) =>
        {
            //从服务器收到信息。但是一般byteBlock和requestInfo会根据适配器呈现不同的值。
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            tcpClient.Logger.Info($"客户端接收到信息：{mes}");
            return EasyTask.CompletedTask;
        };
        #endregion


        //载入配置
        #region Tcp客户端配置插件 {5}
        await tcpClient.SetupAsync(new TouchSocketConfig()
              .SetRemoteIPHost("tcp://127.0.0.1:7789")
              .ConfigurePlugins(a =>
              {
                  a.Add<TcpClientReceivedPlugin>();
              })
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();//添加一个日志注入
              }));
        #endregion

        await tcpClient.ConnectAsync();//调用连接，当连接不成功时，会抛出异常。
        #endregion

        #region Tcp客户端发送数据
        //发送字符串数据
        await tcpClient.SendAsync("hello");

        //发送字节数组数据
        await tcpClient.SendAsync(new byte[] { 0, 1, 2, 3, 4 });
        #endregion

    }

    private static async Task CreateReconnection2TcpClient()
    {
        #region Tcp客户端启用轮询断线重连
        var tcpClient = new TcpClient();

        //载入配置
        await tcpClient.SetupAsync(new TouchSocketConfig()
              .SetRemoteIPHost("127.0.0.1:7789")
              .ConfigurePlugins(a =>
              {
                  a.UseReconnection<TcpClient>(options =>
                  {
                      options.PollingInterval = TimeSpan.FromSeconds(1);
                  });
              }));

        await tcpClient.ConnectAsync();//调用连接
        #endregion
    }

    private static async Task CreateReconnectionTcpClient()
    {
        var client = new TcpClient();

        //载入配置
        await client.SetupAsync(new TouchSocketConfig()
              .SetRemoteIPHost("127.0.0.1:7789")
        #region Tcp客户端启用断线重连
              .ConfigurePlugins(a =>
              {
                  a.UseReconnection<TcpClient>(options =>
                  {
                      options.PollingInterval = TimeSpan.FromSeconds(1);
                  });
              }));
        #endregion
        await client.ConnectAsync();//调用连接

        #region Reconnection重连插件暂停重连
        client.SetPauseReconnection(true);//暂停重连
        await Task.Delay(5000);
        client.SetPauseReconnection(false);//恢复重连
        #endregion
    }

    private static async Task<TcpService> CreateService()
    {
        var service = new TcpService();
        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790, new IPHost(System.Net.IPAddress.IPv6Any, 7791))//同时监听多个地址
             .ConfigureContainer(a =>//容器的配置顺序应该在最前面
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })
             .ConfigurePlugins(a =>
             {
                 #region Tcp健康活性检验插件配置
                 a.UseTcpSessionCheckClear(options =>
                 {
                     options.CheckClearType = CheckClearType.All;
                     options.Tick = TimeSpan.FromSeconds(60);
                     options.OnClose = async (c, t) =>
                     {
                         await c.CloseAsync("超时无数据");
                     };
                 });
                 #endregion
                 a.Add<ClosePlugin>();
                 a.Add<TcpServiceReceivedPlugin>();
                 a.Add<MyServicePluginClass>();
             }));
        await service.StartAsync();//启动
        return service;
    }

    private static void CreateTcpClientConfig()
    {

        var config = new TouchSocketConfig();
        #region 设置远程服务器地址
        config.SetRemoteIPHost("tcp://127.0.0.1:7789");
        #endregion

        #region 设置Tcp客户端Ssl加密
        config.SetClientSslOption(options =>
        {
            options.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            options.CheckCertificateRevocation = false;
            options.ClientCertificates = new X509Certificate2Collection() { new X509Certificate2("client.pfx", "pwd") };
            options.SslProtocols = System.Security.Authentication.SslProtocols.None;
            options.TargetHost = "127.0.0.1";
        });
        #endregion

        #region 设置Tcp底层心跳
        //此配置项仅在windows下有效。
        //非必要请勿开启。
        config.SetKeepAliveValue(options =>
        {
            options.AckInterval = 2000;
            options.Interval = 20 * 1000;
        });
        #endregion

        #region 设置客户端固定端口号
        //不设置时，系统会自动分配端口
        config.SetBindIPHost("127.0.0.1:8848");
        #endregion

        #region 设置客户端端口复用
        config.SetReuseAddress(true);
        #endregion
    }

    private static void GetTouchSocketConfig()
    {
        var config = new TouchSocketConfig();

        #region 示例简单Tcp服务器监听
        //设置监听IP地址和端口
        config.SetListenIPHosts(new IPHost("127.0.0.1:7789"));

        //或者直接设置端口
        config.SetListenIPHosts(7789, 7790);
        #endregion

        #region 示例个性化Tcp服务器监听
        config.SetListenOptions(options =>
        {
            options.Add(new TcpListenOption()
            {
                IpHost = new IPHost(7789),
                Adapter = () => new FixedHeaderPackageAdapter()
                //其他配置
            });
        });
        #endregion

        #region 服务器设置Id生成策略
        config.SetGetDefaultNewId((client) => Guid.NewGuid().ToString());
        #endregion

        #region 服务器设置半连接数量
        config.SetBacklog(100);
        #endregion

        #region 服务器最大连接数
        config.SetMaxCount(10000);
        #endregion

        #region 配置NoDelay算法
        config.SetNoDelay(true);
        #endregion

        #region 服务器端口复用
        config.SetReuseAddress(true);
        #endregion

        #region 接收缓存池的设定场景
        //此配置已在4.0中无实际意义，TouchSocket会根据实际情况动态分配缓存。
        //config.SetMaxBufferSize(1024);
        //config.SetMinBufferSize(1024);
        #endregion

        #region 服务器名称设置
        config.SetServerName("Touch Server");
        #endregion

        #region SSL加密设置
        config.SetServiceSslOption(options =>
        {
            options.Certificate = new X509Certificate2("key.pfx", "pwd");
        });
        #endregion
    }

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
    private static async Task RunClientForReadAsync()
    {
        #region Tcp客户端异步阻塞接收
        var client = new TcpClient();
        await client.ConnectAsync("tcp://127.0.0.1:7789");//连接

        client.Logger.Info("客户端成功连接");

        Console.WriteLine("输入任意内容，回车发送");
        //receiver可以复用，不需要每次接收都新建
        using (var receiver = client.CreateReceiver())
        {
            while (true)
            {
                //发送信息
                await client.SendAsync(Console.ReadLine());

                //设置接收超时
                using (var cts = new CancellationTokenSource(1000 * 60))
                {
                    //receiverResult必须释放
                    using (var receiverResult = await receiver.ReadAsync(cts.Token))
                    {
                        if (receiverResult.IsCompleted)
                        {
                            //断开连接了
                        }

                        //从服务器收到信息。
                        var mes = receiverResult.Memory.Span.ToString(Encoding.UTF8);
                        client.Logger.Info($"客户端接收到信息：{mes}");

                        //如果是适配器信息，则可以直接获取receiverResult.RequestInfo;
                    }
                }

            }
        }
        #endregion

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
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"客户端接收到信息：{mes}");
            return EasyTask.CompletedTask;
        };

        await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection<TcpClient>(options =>
                    {
                        options.PollingInterval = TimeSpan.FromSeconds(1);
                    });
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
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"客户端接收到信息：{mes}");
            return EasyTask.CompletedTask;
        };

        await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("[::1]:7791"))
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection<TcpClient>(options =>
                    {
                        options.PollingInterval = TimeSpan.FromSeconds(1);
                    });
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
}

#region 从继承创建Tcp客户端
internal class MyTcpClient : TcpClientBase
{
    protected override Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        //此处逻辑单线程处理。

        //此处处理数据，功能相当于Received委托。
        var mes = e.Memory.Span.ToString(Encoding.UTF8);
        Console.WriteLine($"已接收到信息：{mes}");

        return EasyTask.CompletedTask;
    }
}

#endregion
#region Tcp服务器异步阻塞接收
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
                    var mes = receiverResult.Memory.Span.ToString(Encoding.UTF8);
                    client.Logger.Info($"客户端接收到信息：{mes}");

                    //如果是适配器信息，则可以直接获取receiverResult.RequestInfo;
                }
            }
        }
    }
}
#endregion


#region Tcp服务器使用插件接收
internal class TcpServiceReceivedPlugin : PluginBase, ITcpReceivedPlugin
{
    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        //从客户端收到信息
        var mes = e.Memory.Span.ToString(Encoding.UTF8);
        if (mes == "close")
        {
            throw new CloseException(mes);
        }
        client.Logger.Info($"已从{client.GetIPPort()}接收到信息：{mes}");

        if (client is ITcpSessionClient sessionClient)
        {
            //将收到的信息直接返回给发送方
            await sessionClient.SendAsync(mes);
        }

        await e.InvokeNext();
    }
}
#endregion

#region Tcp客户端使用插件接收
internal class TcpClientReceivedPlugin : PluginBase, ITcpReceivedPlugin
{
    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        //从客户端收到信息
        var mes = e.Memory.Span.ToString(Encoding.UTF8);

        client.Logger.Info($"已从{client.GetIPPort()}接收到信息：{mes}");

        await e.InvokeNext();
    }
}
#endregion
#region Tcp服务器连接时以IPPort作为Id
internal class InitIdPluginWithIpPort : PluginBase, ITcpConnectingPlugin
{
    public async Task OnTcpConnecting(ITcpSession client, ConnectingEventArgs e)
    {
        if (!client.IsClient)
        {
            //判断在服务器端

            e.Id = $"{client.IP}:{client.Port}";
        }

        await e.InvokeNext();
    }
}
#endregion


#region 自定义Tcp服务器通讯会话
public sealed class MySessionClient : TcpSessionClient
{
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        //此处逻辑单线程处理。

        //此处处理数据，功能相当于Received委托。
        var mes = e.Memory.Span.ToString(Encoding.UTF8);
        Console.WriteLine($"已接收到信息：{mes}");

        await base.OnTcpReceived(e);
    }
}
#endregion

#region 自定义Tcp服务器
public class MyService : TcpService<MySessionClient>
{
    protected override void LoadConfig(TouchSocketConfig config)
    {
        //此处加载配置，用户可以从配置中获取配置项。
        base.LoadConfig(config);
    }

    protected override MySessionClient NewClient()
    {
        return new MySessionClient();
    }

    protected override async Task OnTcpConnecting(MySessionClient socketClient, ConnectingEventArgs e)
    {
        //此处逻辑会多线程处理。

        //e.Id:对新连接的客户端进行ID初始化，默认情况下是按照设定的规则随机分配的。
        //但是按照需求，您可以自定义设置，例如设置为其IP地址。但是需要注意的是id必须在生命周期内唯一。

        //e.IsPermitOperation:指示是否允许该客户端链接。

        await base.OnTcpConnecting(socketClient, e);
    }
}

#endregion
