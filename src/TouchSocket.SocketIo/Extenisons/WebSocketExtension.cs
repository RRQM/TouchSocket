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

internal static class WebSocketExtension
{
    public static async Task<string> ReadAsStringAsync(this WebSocket webSocket, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[4096];
        var sb = new StringBuilder();

        while (true)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                throw new WebSocketException("收到 WebSocket 关闭消息。");
            }

            sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

            if (result.EndOfMessage)
            {
                break;
            }
        }

        return sb.ToString();
    }

    public static async Task SendAsync(this WebSocket webSocket, string message, CancellationToken cancellationToken = default)
    {
        if (webSocket == null)
        {
            throw new ArgumentNullException(nameof(webSocket));
        }

        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
    }

    public static async Task SendBinaryAsync(this WebSocket webSocket, byte[] data, CancellationToken cancellationToken = default)
    {
        if (webSocket == null)
        {
            throw new ArgumentNullException(nameof(webSocket));
        }

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, cancellationToken);
    }
}
