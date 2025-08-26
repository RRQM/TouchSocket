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
                //常规udp
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
