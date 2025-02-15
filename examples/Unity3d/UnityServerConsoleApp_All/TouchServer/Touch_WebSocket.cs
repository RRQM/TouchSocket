using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace UnityServerConsoleApp_All.TouchServer
{
    /// <summary>
    /// Web Socket
    /// </summary>
    public class Touch_WebSocket : BaseTouchServer
    {
        HttpDmtpService dmtpService = new HttpDmtpService();
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
                    a.UseWebSocket()
                     .SetWSUrl("/ws");

                    a.Add<Touch_WebSocket_Log_Plguin>();

                });

            await dmtpService.SetupAsync(config);
            await dmtpService.StartAsync();


            dmtpService.Logger.Info($"TCP_WebSocket已启动，监听端口：{port}");
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
            var m_logger=webSocket.Client.Logger;
            var client =webSocket;
            switch (e.DataFrame.Opcode)
            {
                case WSDataType.Cont:
                    m_logger.Info($"TCP_WebSocket:收到中间数据，长度为：{e.DataFrame.PayloadLength}");

                    return;

                case WSDataType.Text:
                    m_logger.Info("TCP_WebSocket:"+e.DataFrame.ToText());

                    if (!client.Client.IsClient)
                    {
                       await client.SendAsync("TCP_WebSocket:我已收到");
                    }
                    return;

                case WSDataType.Binary:
                    if (e.DataFrame.FIN)
                    {
                        m_logger.Info($"TCP_WebSocket:收到二进制数据，长度为：{e.DataFrame.PayloadLength}");
                    }
                    else
                    {
                        m_logger.Info($"TCP_WebSocket:收到未结束的二进制数据，长度为：{e.DataFrame.PayloadLength}");
                    }
                    return;

                case WSDataType.Close:
                    {
                        m_logger.Info("TCP_WebSocket:远程请求断开");
                        client.Close("TCP_WebSocket:断开");
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
}
