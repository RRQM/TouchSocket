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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket扩展类
/// </summary>
public static class WebSocketExtension
{
    #region WebSocket
    public static async Task<Result> SafeCloseClientAsync(this WebSocket webSocket, string msg, CancellationToken cancellationToken)
    {
        if (webSocket is null)
        {
            return Result.Success;
        }

        if (webSocket.State != WebSocketState.Open)
        {
            return Result.Success;
        }

        try
        {
            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, msg, cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }
    #endregion

    #region string

    /// <summary>
    /// 异步读取完整字符串。
    /// <para>
    /// 注意：该访问调用时如果收到非字符串数据则会抛出异常。同时，该方法不可在<see cref="IWebSocket"/>的接收数据事件（插件）中使用。
    /// 相关用法请按照<see cref="IWebSocket.ReadAsync(CancellationToken)"/>进行。
    /// </para>
    /// </summary>
    /// <param name="webSocket">要读取数据的WebSocket实例</param>
    /// <param name="byteBlock">用于存储接收到的数据的字节块</param>
    /// <param name="cancellationToken">用于取消操作的取消令牌</param>
    /// <returns>返回一个任务，该任务在完成后将包含读取到的字符串</returns>
    public static async Task ReadStringAsync(this IWebSocket webSocket, ByteBlock byteBlock, CancellationToken cancellationToken = default)
    {
        if (!webSocket.AllowAsyncRead)
        {
            ThrowHelper.ThrowInvalidOperationException($"必须启用{nameof(webSocket.AllowAsyncRead)}");
        }
        while (true)
        {
            using (var receiveResult = await webSocket.ReadAsync(cancellationToken))
            {
                if (receiveResult.IsCompleted)
                {
                    ThrowHelper.ThrowClientNotConnectedException();
                }

                var dataFrame = receiveResult.DataFrame;
                var data = dataFrame.PayloadData;

                switch (dataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        {
                            //收到的是中继包
                            if (dataFrame.FIN)//判断是否为最终包
                            {
                                byteBlock.Write(data.Span);
                                return;
                            }
                            else
                            {
                                byteBlock.Write(data.Span);
                            }
                        }
                        break;

                    case WSDataType.Text:
                        {
                            if (dataFrame.FIN)//判断是不是最后的包
                            {
                                byteBlock.Write(data.Span);
                                return;
                            }
                            else
                            {
                                byteBlock.Write(data.Span);
                            }
                        }
                        break;

                    default:
                        ThrowHelper.ThrowInvalidEnumArgumentException(dataFrame.Opcode);
                        return;
                }
            }
        }
    }

    /// <summary>
    /// 异步读取完整字符串。
    /// <para>
    /// 注意：该访问调用时如果收到非字符串数据则会抛出异常。同时，该方法不可在<see cref="IWebSocket"/>的接收数据事件（插件）中使用。
    /// 相关用法请按照<see cref="IWebSocket.ReadAsync(CancellationToken)"/>进行。
    /// </para>
    /// </summary>
    /// <param name="webSocket">要读取数据的WebSocket实例</param>
    /// <param name="cancellationToken">用于取消异步读取操作的取消令牌</param>
    /// <returns>返回异步读取到的字符串</returns>
    public static async Task<string> ReadStringAsync(this IWebSocket webSocket, CancellationToken cancellationToken = default)
    {
        using (var byteBlock = new ByteBlock(1024 * 64))
        {
            await ReadStringAsync(webSocket, byteBlock, cancellationToken);

            return byteBlock.Span.ToString(Encoding.UTF8);
        }
    }

    #endregion string

    #region Binary

    /// <summary>
    /// 异步读取完整二进制数据。
    /// <para>
    /// 注意：该访问调用时如果收到非二进制数据则会抛出异常。同时，该方法不可在<see cref="IWebSocket"/>的接收数据事件（插件）中使用。
    /// 相关用法请按照<see cref="IWebSocket.ReadAsync(CancellationToken)"/>进行。
    /// </para>
    /// </summary>
    /// <param name="webSocket">要读取数据的WebSocket实例。</param>
    /// <param name="byteBlock">用于存储读取的二进制数据的容器。</param>
    /// <param name="cancellationToken">用于取消异步读取操作的取消令牌。</param>
    /// <returns>返回一个Task对象，表示异步读取操作。</returns>
    public static async Task ReadBinaryAsync(this IWebSocket webSocket, ByteBlock byteBlock, CancellationToken cancellationToken = default)
    {
        if (!webSocket.AllowAsyncRead)
        {
            ThrowHelper.ThrowInvalidOperationException($"必须启用{nameof(webSocket.AllowAsyncRead)}");
        }
        while (true)
        {
            using (var receiveResult = await webSocket.ReadAsync(cancellationToken))
            {
                if (receiveResult.IsCompleted)
                {
                    ThrowHelper.ThrowClientNotConnectedException();
                }

                var dataFrame = receiveResult.DataFrame;
                var data = dataFrame.PayloadData;

                switch (dataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        {
                            //收到的是中继包
                            if (dataFrame.FIN)//判断是否为最终包
                            {
                                byteBlock.Write(data.Span);
                                return;
                            }
                            else
                            {
                                byteBlock.Write(data.Span);
                            }
                        }
                        break;

                    case WSDataType.Binary:
                        {
                            if (dataFrame.FIN)//判断是不是最后的包
                            {
                                byteBlock.Write(data.Span);
                                return;
                            }
                            else
                            {
                                byteBlock.Write(data.Span);
                            }
                        }
                        break;

                    default:
                        ThrowHelper.ThrowInvalidEnumArgumentException(dataFrame.Opcode);
                        return;
                }
            }
        }
    }

    /// <summary>
    /// 异步读取完整二进制数据。
    /// <para>
    /// 注意：该访问调用时如果收到非二进制数据则会抛出异常。同时，该方法不可在<see cref="IWebSocket"/>的接收数据事件（插件）中使用。
    /// 相关用法请按照<see cref="IWebSocket.ReadAsync(CancellationToken)"/>进行。
    /// </para>
    /// </summary>
    /// <param name="webSocket">要读取数据的WebSocket实例。</param>
    /// <param name="stream">用于存储读取的二进制数据的流。</param>
    /// <param name="cancellationToken">用于取消异步读取操作的取消令牌。默认值为<see cref="CancellationToken.None"/>。</param>
    /// <returns>返回一个<see cref="Task"/>对象，表示异步读取操作。</returns>
    public static async Task ReadBinaryAsync(this IWebSocket webSocket, Stream stream, CancellationToken cancellationToken = default)
    {
        if (!webSocket.AllowAsyncRead)
        {
            ThrowHelper.ThrowInvalidOperationException($"必须启用{nameof(webSocket.AllowAsyncRead)}");
        }
        while (true)
        {
            using (var receiveResult = await webSocket.ReadAsync(cancellationToken))
            {
                if (receiveResult.IsCompleted)
                {
                    ThrowHelper.ThrowClientNotConnectedException();
                }

                var dataFrame = receiveResult.DataFrame;
                var data = dataFrame.PayloadData;

                switch (dataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        {
                            //收到的是中继包
                            if (dataFrame.FIN)//判断是否为最终包
                            {
                                await stream.WriteAsync(data, CancellationToken.None);
                                return;
                            }
                            else
                            {
                                await stream.WriteAsync(data, CancellationToken.None);
                            }
                        }
                        break;

                    case WSDataType.Binary:
                        {
                            if (dataFrame.FIN)//判断是不是最后的包
                            {
                                await stream.WriteAsync(data, CancellationToken.None);
                                return;
                            }
                            else
                            {
                                await stream.WriteAsync(data, CancellationToken.None);
                            }
                        }
                        break;

                    default:
                        ThrowHelper.ThrowInvalidEnumArgumentException(dataFrame.Opcode);
                        return;
                }
            }
        }
    }

    #endregion Binary

    #region WebSocketMessageCombinator

    /// <summary>
    /// WebSocketMessageCombinatorProperty
    /// </summary>
    public static readonly DependencyProperty<WebSocketMessageCombinator> WebSocketMessageCombinatorProperty =
             new DependencyProperty<WebSocketMessageCombinator>("WebSocketMessageCombinator", (obj) =>
             {
                 var combinator = new WebSocketMessageCombinator();
                 obj.SetValue(WebSocketMessageCombinatorProperty, combinator);
                 return combinator;
             }, false);

    /// <summary>
    /// 获取消息合并器。
    /// </summary>
    /// <param name="webSocket">WebSocket实例</param>
    /// <returns>消息合并器实例</returns>
    public static WebSocketMessageCombinator GetMessageCombinator(this IWebSocket webSocket)
    {
        // 从WebSocket客户端的属性中获取消息合并器实例
        return webSocket.Client.GetValue(WebSocketMessageCombinatorProperty);
    }

    #endregion WebSocketMessageCombinator
}