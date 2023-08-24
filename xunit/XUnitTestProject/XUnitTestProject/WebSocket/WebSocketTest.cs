using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace XUnitTestProject.WebSocket
{
    public class WebSocketTest : UnitBase
    {
        [Fact]
        public void WebSocketConnectShouldBeOk()
        {
            for (var i = 0; i < 100; i++)
            {
                var handshaking = 0L;
                var handshaked = 0L;

                using (var client = new WebSocketClient())
                {
                    client.Setup(new TouchSocketConfig()
                    .ConfigurePlugins(a =>
                    {
                        a.Add(nameof(IWebSocketHandshakingPlugin.OnWebSocketHandshaking), () =>
                        {
                            Interlocked.Increment(ref handshaking);
                        });

                        a.Add(nameof(IWebSocketHandshakedPlugin.OnWebSocketHandshaked), () =>
                        {
                            Interlocked.Increment(ref handshaked);
                        });
                    })
                    .SetRemoteIPHost("ws://127.0.0.1:7801/ws"));

                    client.Connect();

                    Assert.True(handshaking == 1);
                    Assert.True(handshaked == 1);
                }
            }
        }
    }
}