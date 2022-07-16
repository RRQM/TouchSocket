using System.Threading;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets.Plugins
{
    /// <summary>
    /// WebSocketHeartbeatPlugin
    /// </summary>
    [SingletonPlugin]
    public class WebSocketHeartbeatPlugin : WebSocketPluginBase
    {
        private readonly int m_timeTick;

        /// <summary>
        /// 初始化一个适用于WebSocket的心跳插件
        /// </summary>
        /// <param name="interval"></param>
        [DependencyInject(1000 * 5)]
        public WebSocketHeartbeatPlugin(int interval)
        {
            this.m_timeTick = interval;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (client is HttpClientBase httpClientBase)
            {
                if (client.GetValue<Timer>(WebSocketExtensions.HeartbeatTimerProperty) is Timer timer)
                {
                    timer.Dispose();
                }
                client.SetValue(WebSocketExtensions.HeartbeatTimerProperty, new Timer((o) =>
                {
                    httpClientBase.Ping();
                }, null, 0, this.m_timeTick));
            }
            base.OnHandshaked(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            base.OnDisconnected(client, e);
            if (client.GetValue<Timer>(WebSocketExtensions.HeartbeatTimerProperty) is Timer timer)
            {
                timer.Dispose();
                client.SetValue(WebSocketExtensions.HeartbeatTimerProperty, null);
            }
        }
    }
}