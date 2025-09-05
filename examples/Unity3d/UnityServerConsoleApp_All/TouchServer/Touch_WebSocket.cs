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

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace UnityServerConsoleApp_All.TouchServer;

/// <summary>
/// Web Socket
/// </summary>
public class Touch_WebSocket : BaseTouchServer
{
    private readonly HttpDmtpService dmtpService = new HttpDmtpService();
    public async Task StartService(int port)
    {
        var config = new TouchSocketConfig()//配置
            .SetListenIPHosts(port)

            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();//注册一个日志组
            })
            .ConfigurePlugins(a =>
            {
                //添加WebSocket功能
                a.UseWebSocket(options =>
                {
                    options.SetUrl("/ws");//设置url直接可以连接。
                    options.SetAutoPong(true);//当收到ping报文时自动回应pong
                });

                a.Add<Touch_WebSocket_Log_Plguin>();

            });

        await this.dmtpService.SetupAsync(config);
        await this.dmtpService.StartAsync();


        this.dmtpService.Logger.Info($"TCP_WebSocket已启动，监听端口：{port}");
    }
}
/// <summary>
/// 状态日志打印插件
/// </summary>
internal class Touch_WebSocket_Log_Plguin : PluginBase, IWebSocketHandshakedPlugin, IWebSocketReceivedPlugin, IWebSocketClosedPlugin
{

    public async Task OnWebSocketClosed(IWebSocket webSocket, ClosedEventArgs e)
    {
        webSocket.Client.Logger.Info($"TCP_WebSocket:客户端{webSocket.Client.IP}已断开");
        await e.InvokeNext();
    }

    public async Task OnWebSocketHandshaked(IWebSocket webSocket, HttpContextEventArgs e)
    {
        webSocket.Client.Logger.Info($"TCP_WebSocket:客户端{webSocket.Client.IP}已连接");
        await e.InvokeNext();
    }

    public async Task OnWebSocketReceived(IWebSocket webSocket, WSDataFrameEventArgs e)
    {
        var m_logger = webSocket.Client.Logger;
        var client = webSocket;
        switch (e.DataFrame.Opcode)
        {
            case WSDataType.Cont:
                m_logger.Info($"TCP_WebSocket:收到中间数据，长度为：{e.DataFrame.PayloadData.Length}");

                return;

            case WSDataType.Text:
                m_logger.Info("TCP_WebSocket:" + e.DataFrame.ToText());

                if (!client.Client.IsClient)
                {
                    await client.SendAsync("TCP_WebSocket:我已收到");
                }
                return;

            case WSDataType.Binary:
                if (e.DataFrame.FIN)
                {
                    m_logger.Info($"TCP_WebSocket:收到二进制数据，长度为：{e.DataFrame.PayloadData.Length}");
                }
                else
                {
                    m_logger.Info($"TCP_WebSocket:收到未结束的二进制数据，长度为：{e.DataFrame.PayloadData.Length}");
                }
                return;

            case WSDataType.Close:
                {
                    m_logger.Info("TCP_WebSocket:远程请求断开");
                    await client.CloseAsync("TCP_WebSocket:断开");
                }
                return;

            case WSDataType.Ping:
                break;

            case WSDataType.Pong:
                break;

            default:
                break;
        }

        await e.InvokeNext();

    }
}
