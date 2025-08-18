using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UnityServerConsoleApp_All.TouchServer;


/// <summary>
/// UDP 网络服务
/// </summary>
public class Touch_UDP : BaseTouchServer
{
    private readonly UdpSession udpService = new UdpSession();
    public async Task StartService(int port)
    {
        await this.udpService.SetupAsync(new TouchSocketConfig()
               .SetBindIPHost(new IPHost(port))
               .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter())//常规udp
                                                                                   //.SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())//Udp包模式，支持超过64k数据。
                .ConfigurePlugins(a =>
                {
                    a.Add<Touch_UDP_Log_Plguin>();//此处可以添加插件
                })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();//添加一个日志注入
               }));
        await this.udpService.StartAsync();

        this.udpService.Logger.Info($"UdpService已启动，端口：{port}");
    }
    /// <summary>
    /// 状态日志打印插件
    /// </summary>
    private class Touch_UDP_Log_Plguin : PluginBase, IUdpReceivedPlugin
    {
        public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
        {
            client.Logger.Info($"UDP:收到：{e.Memory.Span.ToString(Encoding.UTF8)}");
            if (client is UdpSession session)
            {
                await session.SendAsync(e.EndPoint, "UDP:" + e.EndPoint.ToString() + "收到了你的消息：" + e.Memory.Span.ToString(Encoding.UTF8));
            }
            await e.InvokeNext();
        }
    }
}
