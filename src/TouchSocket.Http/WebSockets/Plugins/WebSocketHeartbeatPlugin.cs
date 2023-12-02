//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// 初始化一个适用于WebSocket的心跳插件
    /// </summary>
    [PluginOption(Singleton = true)]
    public class WebSocketHeartbeatPlugin : HeartbeatPlugin, IWebSocketHandshakedPlugin
    {
        /// <inheritdoc/>
        public Task OnWebSocketHandshaked(IHttpClientBase client, HttpContextEventArgs e)
        {
            Task.Run(async () =>
            {
                var failedCount = 0;
                while (true)
                {
                    await Task.Delay(this.Tick);
                    if (!client.GetHandshaked())
                    {
                        return;
                    }

                    try
                    {
                        client.PingWS();
                        failedCount = 0;
                    }
                    catch
                    {
                        failedCount++;
                    }
                    if (failedCount > this.MaxFailCount)
                    {
                        client.CloseWithWS("自动心跳失败次数达到最大，已断开连接。");
                    }
                }
            });

            return e.InvokeNext();
        }
    }
}