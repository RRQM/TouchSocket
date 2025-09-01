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
using TouchSocket.Sockets;

namespace ReuseAddressServerConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        var service = new TcpService();
        service.SetupAsync(new TouchSocketConfig()//载入配置
            .SetReuseAddress(true)
            .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
            .ConfigureContainer(a =>//容器的配置顺序应该在最前面
            {
                a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
            })
            .ConfigurePlugins(a =>
            {
                a.Add<MyClassPlugin>();
                //a.Add();//此处可以添加插件
            }));

        service.StartAsync();//启动

        service.Logger.Info("服务器已启动");

        Console.ReadKey();
    }
}

internal class MyClassPlugin : PluginBase, ITcpConnectedPlugin, ITcpClosedPlugin, ITcpReceivedPlugin
{
    private readonly ILog m_logger;

    public MyClassPlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        this.m_logger.Info($"已连接，信息：{client.GetIPPort()}");
        await e.InvokeNext();
    }

    public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
    {
        this.m_logger.Info($"已断开，信息：{client.GetIPPort()}");
        await e.InvokeNext();
    }

    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        this.m_logger.Info($"收到数据，信息：{e.Memory.Span.ToUtf8String()}");
        await e.InvokeNext();
    }
}