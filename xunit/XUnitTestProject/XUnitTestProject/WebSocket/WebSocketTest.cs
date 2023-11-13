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
            int waitTime = 100;
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

                    Assert.True(client.GetHandshaked());
                    Assert.True(handshaking == 1);
                    Thread.Sleep(waitTime);
                    Assert.True(handshaked == 1);
                }
            }
        }

        [Fact]
        public async Task WebSocketConnectAsyncShouldBeOk()
        {
            int waitTime = 100;
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

                    await client.ConnectAsync();

                    Assert.True(client.GetHandshaked());
                    Assert.True(handshaking == 1);
                    await Task.Delay(waitTime);
                    Assert.True(handshaked == 1);
                }
            }
        }

        [Fact]
        public async Task WebSocketReadShouldBeOk()
        {
            var receivedCount = 0;
            using (var client = new WebSocketClient())
            {
                client.Setup(new TouchSocketConfig()
                .ConfigurePlugins(a =>
                {
                    a.Add(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), () =>
                    {
                        Interlocked.Increment(ref receivedCount);
                    });
                })
                .SetRemoteIPHost("ws://127.0.0.1:7801/wsread"));

                client.Connect();

                using (var websocket = client.GetWebSocket())
                {
                    Assert.True(websocket.IsHandshaked == true);

                    for (var i = 0; i < 100; i++)
                    {
                        var str = "RRQM" + i;
                        websocket.Send(str);
                        using (var tokenSource = new CancellationTokenSource(5000))
                        {
                            using (var receiveResult = await websocket.ReadAsync(tokenSource.Token))
                            {
                                var strNew = receiveResult.DataFrame.ToText();
                                Assert.Equal(str, strNew);
                            }
                        }
                    }
                }
            }
        }
    }
}