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

namespace TouchSocket.SocketIo;

internal class WebSocketTransport : ISocketIoTransport
{
    private static readonly byte[] s_recvBuffer = new byte[1024 * 16];
    private readonly Func<ISocketIoMessage, Task> m_receivedSocketIoMessage;
    private readonly SocketIoCore m_socketIo;
    private readonly WebSocket m_webSocket;
    private ISocketIoMessage m_socketIOMessage;

    public WebSocketTransport(SocketIoCore socketIo, WebSocket webSocket, Func<ISocketIoMessage, Task> receivedSocketIoMessage)
    {
        this.m_socketIo = socketIo;
        this.m_webSocket = webSocket;
        this.m_receivedSocketIoMessage = receivedSocketIoMessage;
    }

    public async Task BeginPolling()
    {
        var buffer = new byte[1024 * 16];

        while (true)
        {
            WebSocketReceiveResult receiveResult;
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                do
                {
                    var arraySegment = new ArraySegment<byte>(buffer);
                    receiveResult = await this.m_webSocket.ReceiveAsync(arraySegment, CancellationToken.None);
                    if (receiveResult.Count > 0)
                    {
                        ms.Write(buffer, 0, receiveResult.Count);
                    }
                }
                while (!receiveResult.EndOfMessage);
            }
            catch
            {
                ms?.Dispose();
                return;
            }

            var data = ms.ToArray();
            ms.Dispose();

            switch (receiveResult.MessageType)
            {
                case WebSocketMessageType.Text:
                    await this.ReceivedText(Encoding.UTF8.GetString(data));
                    break;

                case WebSocketMessageType.Binary:
                    {
                        if (this.m_socketIOMessage != null)
                        {
                            this.m_socketIOMessage.Bytes.Add(data);
                            if (this.m_socketIOMessage.Bytes.Count == this.m_socketIOMessage.BytesIndexs.Length)
                            {
                                await this.m_receivedSocketIoMessage.Invoke(this.m_socketIOMessage);
                                this.m_socketIOMessage = null;
                            }
                        }
                    }
                    break;

                case WebSocketMessageType.Close:
                default:
                    await this.m_webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    return;
            }
        }
    }

    public async Task PingAsync()
    {
        await this.m_webSocket.SendAsync("3");
    }

    public async Task SendAsync(List<DataItem> dataItems)
    {
        if (dataItems.Count > 0 && dataItems[0].IsText)
        {
            await this.m_webSocket.SendAsync(dataItems[0].Text);
        }

        for (var i = 1; i < dataItems.Count; i++)
        {
            await this.m_webSocket.SendBinaryAsync(dataItems[i].Bytes);
        }
    }

    private async Task ReceivedEngineIoMessage(EngineIoMessage engineIOMessage)
    {
        switch (engineIOMessage.MessageType)
        {
            case EngineIoMessageType.Open:
                break;

            case EngineIoMessageType.Close:
                break;

            case EngineIoMessageType.Ping:
                {
                    await this.PingAsync();
                }
                break;

            case EngineIoMessageType.Pong:
                break;

            case EngineIoMessageType.Message:
                {
                    var socketIOMessage = this.m_socketIo.Decode(engineIOMessage);
                    if (socketIOMessage == null)
                    {
                        break;
                    }

                    if (socketIOMessage.BytesIndexs.Length > 0)
                    {
                        socketIOMessage.Bytes = new List<byte[]>();
                        this.m_socketIOMessage = socketIOMessage;
                    }
                    else
                    {
                        await this.m_receivedSocketIoMessage.Invoke(socketIOMessage);
                    }
                }
                break;

            case EngineIoMessageType.Upgrade:
                break;

            case EngineIoMessageType.Noop:
                break;

            default:
                break;
        }
    }

    private async Task ReceivedText(string text)
    {
        var engineIOMessage = this.m_socketIo.EngineIo.Decode(text);
        await this.ReceivedEngineIoMessage(engineIOMessage);
    }
}
