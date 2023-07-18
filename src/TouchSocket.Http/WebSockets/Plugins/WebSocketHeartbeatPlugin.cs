//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// 初始化一个适用于WebSocket的心跳插件
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = true)]
    public class WebSocketHeartbeatPlugin<TClient> : PluginBase, IWebsocketHandshakedPlugin<TClient> where TClient : IHttpClientBase
    {
        /// <summary>
        /// 心跳频率
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(5);


        /// <summary>
        /// 设置心跳间隔，默认5秒。
        /// </summary>
        /// <param name="timeSpan"></param>
        public WebSocketHeartbeatPlugin<TClient> SetTick(TimeSpan timeSpan)
        {
            this.Tick = timeSpan;
            return this;
        }

        /// <inheritdoc/>
        public Task OnWebsocketHandshaked(TClient client, HttpContextEventArgs e)
        {
            Task.Run(async() => 
            {
                while (true)
                {
                    await Task.Delay(this.Tick);
                    if (!client.GetHandshaked())
                    {
                        return;
                    }
                    if (client is IHttpClient httpClient)
                    {
                        httpClient.PingWS();
                    }
                    else if (client is IHttpSocketClient httpSocketClient)
                    {
                        httpSocketClient.PingWS();
                    }
                }
            });

            return e.InvokeNext();
        }
    }
}