using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UnityServerConsoleApp_All.TouchServer;

/// <summary>
/// TCP 网络服务
/// </summary>
public class Touch_TCP : BaseTouchServer
{
    private readonly TcpService tcpService = new TcpService();
    public async Task StartService(int port)
    {
        await this.tcpService.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts(new IPHost(port))
             .SetTcpDataHandlingAdapter(() => new FixedHeaderPackageAdapter())
             .ConfigurePlugins(a =>
             {
                 a.Add<Touch_TCP_Log_Plguin>();//此处可以添加插件
             })
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();//添加一个日志注入
             }));
        await this.tcpService.StartAsync();//启动
        this.tcpService.Logger.Info($"Tcp服务器已启动，端口{port}");
    }
}

/// <summary>
/// 状态日志打印插件
/// </summary>
internal class Touch_TCP_Log_Plguin : PluginBase, ITcpConnectedPlugin, ITcpClosedPlugin, ITcpReceivedPlugin
{
    public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
    {
        client.Logger.Info($"TCP:客户端{client.GetIPPort()}已断开");
        await e.InvokeNext();
    }

    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        client.Logger.Info($"TCP:客户端{client.GetIPPort()}已连接");
        await e.InvokeNext();
    }


    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        client.Logger.Info($"TCP:接收到信息：{e.Memory.Span.ToString(Encoding.UTF8)}");

        if (client is ITcpSessionClient sessionClient)
        {
            await sessionClient.SendAsync($"TCP:服务器已收到你发送的消息：{e.Memory.Span.ToUtf8String()}");
        }
        await e.InvokeNext();
    }
}
