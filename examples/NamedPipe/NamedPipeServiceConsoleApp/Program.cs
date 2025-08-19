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
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeServiceConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        var service = CreateService();

        Console.ReadKey();
    }

    private static NamedPipeService CreateService()
    {
        var service = new NamedPipeService();
        service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
        service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
        service.Closed = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接
        service.SetupAsync(new TouchSocketConfig()//载入配置
            .SetPipeName("touchsocketpipe")//设置命名管道名称
            .SetNamedPipeListenOptions(list =>
            {
                //如果想实现多个命名管道的监听，即可这样设置，一直Add即可。
                list.Add(new NamedPipeListenOption()
                {
                    //Adapter = () => new DataHandlingAdapter(),
                    Name = "TouchSocketPipe2"//管道名称
                });

                list.Add(new NamedPipeListenOption()
                {
                    Adapter = () => new FixedHeaderPackageAdapter(),
                    Name = "TouchSocketPipe3"//管道名称
                });
            })
            .ConfigureContainer(a =>//容器的配置顺序应该在最前面
            {
                a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
            })
            .ConfigurePlugins(a =>
            {
                a.Add<MyNamedPipePlugin>();
                //a.Add();//此处可以添加插件
            }));
        service.StartAsync();//启动
        service.Logger.Info("服务器已启动");
        return service;
    }
}

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
            await sessionClient.SendAsync(e.Memory.Memory);
        }

        await e.InvokeNext();
    }
}