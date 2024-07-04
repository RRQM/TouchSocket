using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    public static class WebSocketExtension
    {
        #region string
        /// <summary>
        /// 异步读取完整字符串。
        /// <para>
        /// 注意：该访问调用时如果收到非字符串数据则会抛出异常。同时，该方法不可在<see cref="IWebSocket"/>的接收数据事件（插件）中使用。
        /// 相关用法请按照<see cref="IWebSocket.ReadAsync(CancellationToken)"/>进行。
        /// </para>
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="byteBlock"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task ReadStringAsync(this IWebSocket webSocket, ByteBlock byteBlock, CancellationToken token = default)
        {
            if (!webSocket.AllowAsyncRead)
            {
                ThrowHelper.ThrowInvalidOperationException($"必须启用{nameof(webSocket.AllowAsyncRead)}");
            }
            while (true)
            {
                using (var receiveResult = await webSocket.ReadAsync(token))
                {
                    if (receiveResult.IsCompleted)
                    {
                        ThrowHelper.ThrowClientNotConnectedException();
                    }

                    var dataFrame = receiveResult.DataFrame;
                    var data = receiveResult.DataFrame.PayloadData;

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
        /// <param name="webSocket"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<string> ReadStringAsync(this IWebSocket webSocket, CancellationToken token = default)
        {
            using (var byteBlock = new ByteBlock(1024 * 64))
            {
                await ReadStringAsync(webSocket, byteBlock, token);

                return byteBlock.Span.ToString(Encoding.UTF8);
            }
        }
        #endregion


        #region Binary
        /// <summary>
        /// 异步读取完整二进制数据。
        /// <para>
        /// 注意：该访问调用时如果收到非二进制数据则会抛出异常。同时，该方法不可在<see cref="IWebSocket"/>的接收数据事件（插件）中使用。
        /// 相关用法请按照<see cref="IWebSocket.ReadAsync(CancellationToken)"/>进行。
        /// </para>
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="byteBlock"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task ReadBinaryAsync(this IWebSocket webSocket, ByteBlock byteBlock, CancellationToken token = default)
        {
            if (!webSocket.AllowAsyncRead)
            {
                ThrowHelper.ThrowInvalidOperationException($"必须启用{nameof(webSocket.AllowAsyncRead)}");
            }
            while (true)
            {
                using (var receiveResult = await webSocket.ReadAsync(token))
                {
                    if (receiveResult.IsCompleted)
                    {
                        ThrowHelper.ThrowClientNotConnectedException();
                    }

                    var dataFrame = receiveResult.DataFrame;
                    var data = receiveResult.DataFrame.PayloadData;

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
        /// <param name="webSocket"></param>
        /// <param name="stream"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task ReadBinaryAsync(this IWebSocket webSocket, Stream stream, CancellationToken token = default)
        {
            if (!webSocket.AllowAsyncRead)
            {
                ThrowHelper.ThrowInvalidOperationException($"必须启用{nameof(webSocket.AllowAsyncRead)}");
            }
            while (true)
            {
                using (var receiveResult = await webSocket.ReadAsync(token))
                {
                    if (receiveResult.IsCompleted)
                    {
                        ThrowHelper.ThrowClientNotConnectedException();
                    }

                    var dataFrame = receiveResult.DataFrame;
                    var data = receiveResult.DataFrame.PayloadData;

                    switch (dataFrame.Opcode)
                    {
                        case WSDataType.Cont:
                            {
                                //收到的是中继包
                                if (dataFrame.FIN)//判断是否为最终包
                                {
                                    var segment = data.AsSegment();
                                    await stream.WriteAsync(segment.Array, segment.Offset, segment.Count);
                                    return;
                                }
                                else
                                {
                                    var segment = data.AsSegment();
                                    await stream.WriteAsync(segment.Array, segment.Offset, segment.Count);
                                }
                            }
                            break;
                        case WSDataType.Binary:
                            {
                                if (dataFrame.FIN)//判断是不是最后的包
                                {
                                    var segment = data.AsSegment();
                                    await stream.WriteAsync(segment.Array, segment.Offset, segment.Count);
                                    return;
                                }
                                else
                                {
                                    var segment = data.AsSegment();
                                    await stream.WriteAsync(segment.Array, segment.Offset, segment.Count);
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
        #endregion
    }
}
