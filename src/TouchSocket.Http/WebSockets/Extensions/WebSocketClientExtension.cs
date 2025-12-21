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

using System.Buffers;
using System.Net.WebSockets;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket客户端扩展
/// </summary>
public static class WebSocketClientExtension
{
    /// <summary>
    /// 异步发送Ping帧
    /// </summary>
    /// <param name="webSocket">WebSocket实例</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回操作结果</returns>
    public static async Task<Result> PingAsync(this IWebSocket webSocket, CancellationToken cancellationToken = default)
    {
        try
        {
            await webSocket.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping }, cancellationToken: cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 异步发送Pong帧
    /// </summary>
    /// <param name="webSocket">WebSocket实例</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回操作结果</returns>
    public static async Task<Result> PongAsync(this IWebSocket webSocket, CancellationToken cancellationToken = default)
    {
        try
        {
            await webSocket.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong }, cancellationToken: cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 异步发送数据帧。
    /// </summary>
    /// <param name="webSocket">WebSocket客户端实例。</param>
    /// <param name="memory">要发送的二进制数据。</param>
    /// <param name="dataType">数据类型。</param>
    /// <param name="endOfMessage">是否为消息的结束帧。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task SendAsync(this IWebSocket webSocket, ReadOnlyMemory<byte> memory, WSDataType dataType, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        var frame = new WSDataFrame(memory) { FIN = endOfMessage, Opcode = dataType };
        await webSocket.SendAsync(frame, endOfMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步发送文本消息
    /// </summary>
    /// <param name="webSocket">WebSocket实例</param>
    /// <param name="text">要发送的文本内容</param>
    /// <param name="endOfMessage">是否为消息的结束帧</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>表示异步操作的任务</returns>
    public static async Task SendAsync(this IWebSocket webSocket, string text, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(text, nameof(text));
        var byteBlock = new ByteBlock(Encoding.UTF8.GetMaxByteCount(text.Length));
        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, text, Encoding.UTF8);
            var frame = new WSDataFrame(byteBlock.Memory) { FIN = endOfMessage, Opcode = WSDataType.Text };
            await webSocket.SendAsync(frame, endOfMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 异步发送二进制数据
    /// </summary>
    /// <param name="webSocket">WebSocket实例</param>
    /// <param name="memory">要发送的二进制数据</param>
    /// <param name="endOfMessage">是否为消息的结束帧</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>表示异步操作的任务</returns>
    public static async Task SendAsync(this IWebSocket webSocket, ReadOnlyMemory<byte> memory, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        var frame = new WSDataFrame(memory) { FIN = endOfMessage, Opcode = WSDataType.Binary };
        await webSocket.SendAsync(frame, endOfMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步发送字节序列数据，将序列的每个片段作为中继帧发送
    /// </summary>
    /// <param name="webSocket">WebSocket实例</param>
    /// <param name="sequence">要发送的字节序列</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>表示异步操作的任务</returns>
    public static async Task SendAsync(this IWebSocket webSocket, ReadOnlySequence<byte> sequence, WSDataType dataType, CancellationToken cancellationToken = default)
    {
        if (sequence.IsEmpty)
        {
            return;
        }

        if (sequence.IsSingleSegment)
        {
            var frame = new WSDataFrame(sequence.First) { FIN = true, Opcode = dataType };
            await webSocket.SendAsync(frame, true, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        var isFirst = true;
        var position = sequence.Start;

        while (sequence.TryGet(ref position, out var memory))
        {
            var isLast = position.Equals(sequence.End);
            var frame = new WSDataFrame(memory)
            {
                FIN = isLast,
                Opcode = isFirst ? dataType : WSDataType.Cont
            };

            await webSocket.SendAsync(frame, isLast, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            isFirst = false;
        }
    }
}