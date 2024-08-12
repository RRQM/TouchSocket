//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Net.WebSockets;
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

                    Assert.True(client.IsHandshaked);
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

                    Assert.True(client.IsHandshaked);
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
                client.AllowAsyncRead = true;

                Assert.True(client.IsHandshaked == true);

                for (var i = 0; i < 100; i++)
                {
                    var str = "RRQM" + i;
                    client.Send(str);
                    using (var tokenSource = new CancellationTokenSource(5000))
                    {
                        using (var receiveResult = await client.ReadAsync(tokenSource.Token))
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