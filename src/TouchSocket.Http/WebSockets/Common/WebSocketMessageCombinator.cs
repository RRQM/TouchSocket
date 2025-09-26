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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket消息结构体，实现了IDisposable接口，用于处理WebSocket消息的生命周期。
/// </summary>
public readonly struct WebSocketMessage : IDisposable
{
    // 定义一个动作，用于在消息释放时调用。
    private readonly Action m_disposeAction;

    /// <summary>
    /// 初始化WebSocketMessage结构体的新实例。
    /// </summary>
    /// <param name="opcode">消息的数据类型，使用WSDataType枚举表示。</param>
    /// <param name="payloadData">消息的负载数据，使用ByteBlock结构表示。</param>
    /// <param name="disposeAction">在消息释放时需要调用的动作。</param>
    public WebSocketMessage(WSDataType opcode, ReadOnlyMemory<byte> payloadData, Action disposeAction)
    {
        this.Opcode = opcode;
        this.PayloadData = payloadData;
        this.m_disposeAction = disposeAction;
    }

    /// <summary>
    /// 获取消息的数据类型。
    /// </summary>
    public WSDataType Opcode { get; }

    /// <summary>
    /// 获取消息的负载数据。
    /// </summary>
    public ReadOnlyMemory<byte> PayloadData { get; }

    /// <summary>
    /// 释放消息资源。
    /// </summary>
    public void Dispose()
    {
        this.m_disposeAction.Invoke();
    }
}

/// <summary>
/// WebSocket消息合并器。其作用是合并具有中继数据的消息
/// </summary>
public sealed class WebSocketMessageCombinator
{
    private ByteBlock m_byteBlock = default;//中继包缓存
    private bool m_combining;
    private WSDataType m_wSDataType;

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
                    //先写入数据
                    this.m_byteBlock.Write(data.Span);

                    //判断中继包
                    if (dataFrame.FIN)//判断是否为最终包
                    {
                        webSocketMessage = new WebSocketMessage(this.m_wSDataType, this.m_byteBlock.Memory, this.PrivateClear);
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
                    if (dataFrame.FIN)//判断是不是最后的包
                    {
                        //是，则直接输出

                        //如果上次并有中继数据缓存，则说明合并出现了无法处理的情况
                        if (this.m_combining)
                        {
                            ThrowHelper.ThrowInvalidOperationException("上个合并数据没有完成处理");
                        }
                        webSocketMessage = new WebSocketMessage(dataFrame.Opcode, dataFrame.PayloadData, this.PrivateClear);
                        return true;
                    }
                    else
                    {
                        //先保存数据类型
                        this.m_wSDataType = dataFrame.Opcode;
                        this.m_combining = true;

                        //否，则说明数据太大了，分中继包了。
                        //则，初始化缓存容器
                        this.m_byteBlock ??= new ByteBlock(1024 * 64);

                        this.m_byteBlock.Write(data.Span);

                        webSocketMessage = default;
                        return false;
                    }
                }
        }
    }

    private void PrivateClear()
    {
        var byteBlock = this.m_byteBlock;
        byteBlock?.Dispose();
        this.m_byteBlock = default;
        this.m_combining = false;
        this.m_wSDataType = default;
    }

    /// <summary>
    /// 清空所有缓存状态及数据。
    /// </summary>
    public void Clear()
    {
        this.PrivateClear();
    }
}