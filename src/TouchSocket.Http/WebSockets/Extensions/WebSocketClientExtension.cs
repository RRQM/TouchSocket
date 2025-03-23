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

using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocketClientExtension
/// </summary>
public static class WebSocketClientExtension
{
    /// <summary>
    /// 异步发送数据帧。
    /// </summary>
    /// <param name="webSocket">WebSocket客户端实例。</param>
    /// <param name="memory">要发送的二进制数据。</param>
    /// <param name="dataType">数据类型。</param>
    /// <param name="endOfMessage">是否为消息的结束帧。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task SendAsync(this IWebSocket webSocket, ReadOnlyMemory<byte> memory, WSDataType dataType, bool endOfMessage = true)
    {
        using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = dataType })
        {
            frame.AppendBinary(memory.Span);
            await webSocket.SendAsync(frame, endOfMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }
}
