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

using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeServiceConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();

        Console.ReadKey();
    }

    private static void CreateTouchSocketConfig()
    {
        TouchSocketConfig config = new TouchSocketConfig();

        #region 示例简单NamedPipe服务器监听
        config.SetPipeName("touchsocketpipe");//设置命名管道名称
        #endregion

        #region 示例个性化NamedPipe服务器监听
        config.SetNamedPipeListenOption(list =>
        {
            //如果想实现多个命名管道的监听，即可这样设置，一直Add即可。
            list.Add(new NamedPipeListenOption()
            {
                //Adapter = () => new DataHandlingAdapter(),
                PipeName = "TouchSocketPipe2"//管道名称
            });

            list.Add(new NamedPipeListenOption()
            {
                Adapter = () => new FixedHeaderPackageAdapter(),
                PipeName = "TouchSocketPipe3"//管道名称
            });
        });
        #endregion

        #region NamedPipe服务器设置Id生成策略
        config.SetGetDefaultNewId((client) => Guid.NewGuid().ToString());
        #endregion
    }


    private static async Task CreateDefaultService2()
    {
        #region NamedPipe动态添加移除监听配置 {5,17}
        var service = new NamedPipeService();
        await service.StartAsync();//直接启动，此处没有配置任何东西

        //在Service运行时，可以调用，直接添加监听
        service.AddListen(new NamedPipeListenOption ()
        {
            PipeName="touchsocketpipe2",//管道名称
            Name="server2",//名称用于区分监听
            Adapter = () => new FixedHeaderPackageAdapter(),//可以单独对当前地址监听，配置适配器
        });

        //在Service运行时，可以调用，直接移除监听
        foreach (var item in service.Monitors)
        {
            service.RemoveListen(item);//在Service运行时，可以调用，直接移除现有监听
        }
        #endregion

        #region Tcp服务器重置Id
        await service.ResetIdAsync("oldId", "newId");
        #endregion

        #region Tcp服务器通过SessionClient重置Id
        if (service.TryGetClient("oldId", out var sessionClient))
        {
            await sessionClient.ResetIdAsync("newId");
        }
        #endregion
    }

    private static async Task CreateCustomService()
    {
        #region 创建MyNamedPipeService服务器
        var service = new MyNamedPipeService();
        service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
        service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
        service.Closing = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在断开连接，只有当主动断开时才有效。
        service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接

        #region NamedPipe服务器使用Received异步委托接收数据
        service.Received = async (client, e) =>
        {
            //从客户端收到信息
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"已从{client.Id}接收到信息：{mes}");

            //按字符发送给客户端
            await client.SendAsync(mes);

            //按字节数组发送给客户端
            //await client.SendAsync(new byte[] { 0, 1, 2, 3, 4 });
        };
        #endregion


        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetPipeName("touchsocketpipe")
             .ConfigureContainer(a =>//容器的配置
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })

        #region NamedPipe服务器配置插件
             .ConfigurePlugins(a =>
             {
                 a.Add<NamedPipeServiceReceivedPlugin>();
             })
        #endregion
             );

        await service.StartAsync();//启动
        #endregion

        #region NamedPipe服务器按目标Id直接回应
        await service.SendAsync("targetId", "消息内容");
        #endregion

        #region NamedPipe服务器按目标Id先查找再回应
        if (service.Clients.TryGetClient("targetId", out var tcpSessionClient))
        {
            await tcpSessionClient.SendAsync("消息内容");
        }
        #endregion

        #region NamedPipe清理当前所有连接
        await service.ClearAsync();
        #endregion

        #region NamedPipe清理指定连接
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

        #region NamedPipe服务器停止监听
        foreach (var item in service.Monitors)
        {
            service.RemoveListen(item);
        }
        #endregion

        #region NamedPipe停止服务器
        await service.StopAsync();
        #endregion

        #region NamedPipe释放服务器资源
        service.Dispose();
        #endregion
    }

    private static async Task<NamedPipeService> CreateService()
    {
        #region 创建NamedPipeService
        var service = new NamedPipeService();

        service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
        service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接

        var config = new TouchSocketConfig();
        config.SetPipeName("touchsocketpipe");//设置命名管道名称

        //配置容器
        config.ConfigureContainer(a =>
        {
            a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
        });

        //配置插件
        config.ConfigurePlugins(a =>
        {
            a.Add<MyNamedPipePlugin>();
            //a.Add();//此处可以添加插件
        });

        //载入配置
        await service.SetupAsync(config);

        //启动服务
        await service.StartAsync();//启动
        #endregion

        service.Logger.Info("服务器已启动");
        return service;
    }
}

#region NamedPipe服务器异步阻塞接收
internal class TcpServiceReceiveAsyncPlugin : PluginBase, INamedPipeConnectedPlugin
{
    public async Task OnNamedPipeConnected(INamedPipeSession client, ConnectedEventArgs e)
    {
        if (client is not INamedPipeSessionClient sessionClient)
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

#region NamedPipe服务器使用插件接收
class NamedPipeServiceReceivedPlugin : PluginBase, INamedPipeReceivedPlugin
{
    public async Task OnNamedPipeReceived(INamedPipeSession client, ReceivedDataEventArgs e)
    {
        //从客户端收到信息
        var mes = e.Memory.Span.ToString(Encoding.UTF8);
        client.Logger.Info($"已从客户端接收到信息：{mes}");

        if (client is INamedPipeSessionClient sessionClient)
        {
            //将收到的信息直接返回给发送方
            await sessionClient.SendAsync(mes);
        }

        await e.InvokeNext();
    }
}
#endregion

internal class MyNamedPipePlugin : PluginBase, INamedPipeConnectedPlugin, INamedPipeClosedPlugin, INamedPipeReceivedPlugin
{
    private readonly ILog m_logger;

    public MyNamedPipePlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    public async Task OnNamedPipeClosed(INamedPipeSession client, ClosedEventArgs e)
    {
        this.m_logger.Info("Closed");
        await e.InvokeNext();
    }

    public async Task OnNamedPipeConnected(INamedPipeSession client, ConnectedEventArgs e)
    {
        this.m_logger.Info("Connected");
        await e.InvokeNext();
    }


    public async Task OnNamedPipeReceived(INamedPipeSession client, ReceivedDataEventArgs e)
    {
        this.m_logger.Info(e.Memory.Span.ToUtf8String());

        if (client is INamedPipeSessionClient sessionClient)
        {
            await sessionClient.SendAsync(e.Memory);
        }

        await e.InvokeNext();
    }
}

#region 自定义NamedPipe服务器
class MyNamedPipeService : NamedPipeService<MyNamedPipeSessionClient>
{
    protected override void LoadConfig(TouchSocketConfig config)
    {
        //此处加载配置，用户可以从配置中获取配置项。
        base.LoadConfig(config);
    }

    protected override MyNamedPipeSessionClient NewClient()
    {
        return new MyNamedPipeSessionClient();
    }

    protected override async Task OnNamedPipeConnecting(MyNamedPipeSessionClient sessionClient, ConnectingEventArgs e)
    {
        //此处逻辑会多线程处理。

        //e.Id:对新连接的客户端进行ID初始化，默认情况下是按照设定的规则随机分配的。
        //但是按照需求，您可以自定义设置，例如设置为其IP地址。但是需要注意的是id必须在生命周期内唯一。

        //e.IsPermitOperation:指示是否允许该客户端链接。
        await base.OnNamedPipeConnecting(sessionClient, e);
    }
}
#endregion

#region 自定义NamedPipe服务器通讯会话
class MyNamedPipeSessionClient : NamedPipeSessionClient
{
    protected override async Task OnNamedPipeReceived(ReceivedDataEventArgs e)
    {
        //此处逻辑单线程处理。

        //此处处理数据，功能相当于Received委托。
        var mes = e.Memory.Span.ToString(Encoding.UTF8);
        Console.WriteLine($"已接收到信息：{mes}");

        await base.OnNamedPipeReceived(e);
    }
}
#endregion