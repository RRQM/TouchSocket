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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket消息合并器。其作用是合并具有中继数据的消息
/// </summary>
public sealed class WebSocketMessageCombinator
{
    private SegmentedBytesWriter m_writer = default;//中继包缓存
    private bool m_combining;
    private WSDataType m_wSDataType;

    /// <summary>
    /// 清空所有缓存状态及数据。
    /// </summary>
    public void Clear()
    {
        this.PrivateClear();
    }

    /// <summary>
    /// 尝试将数据帧组合成WebSocket消息。
    /// </summary>
    /// <param name="dataFrame">待组合的数据帧。</param>
    /// <param name="webSocketMessage">组合成功的WebSocket消息。</param>
    /// <returns>如果成功组合则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public bool TryCombine(WSDataFrame dataFrame, out WebSocketMessage webSocketMessage)
    {
        var data = dataFrame.PayloadData;

        switch (dataFrame.Opcode)
        {
            case WSDataType.Cont:
                {
                    if (!this.m_combining)
                    {
                        ThrowHelper.ThrowInvalidOperationException("收到中继帧但没有待合并的消息");
                    }

                    if (this.m_writer is null)
                    {
                        ThrowHelper.ThrowInvalidOperationException("合并器处于无效状态");
                    }

                    this.m_writer.Write(data.Span);

                    if (dataFrame.FIN)
                    {
                        webSocketMessage = new WebSocketMessage(this.m_wSDataType, this.m_writer.Sequence, this.PrivateClear);
                        return true;
                    }
                    else
                    {
                        webSocketMessage = default;
                        return false;
                    }
                }
            case WSDataType.Close:
            case WSDataType.Ping:
            case WSDataType.Pong:
                {
                    webSocketMessage = default;
                    return false;
                }
            default:
                {
                    if (dataFrame.FIN)
                    {
                        if (this.m_combining)
                        {
                            ThrowHelper.ThrowInvalidOperationException("上个合并数据没有完成处理");
                        }
                        webSocketMessage = new WebSocketMessage(dataFrame.Opcode, new ReadOnlySequence<byte>(dataFrame.PayloadData), null);
                        return true;
                    }
                    else
                    {
                        if (this.m_combining)
                        {
                            ThrowHelper.ThrowInvalidOperationException("上个合并数据没有完成处理");
                        }

                        this.m_wSDataType = dataFrame.Opcode;
                        this.m_combining = true;

                        this.m_writer ??= new ();

                        this.m_writer.Write(data.Span);

                        webSocketMessage = default;
                        return false;
                    }
                }
        }
    }

    private void PrivateClear()
    {
        var byteBlock = this.m_writer;
        byteBlock?.Dispose();
        this.m_writer = default;
        this.m_combining = false;
        this.m_wSDataType = default;
    }
}