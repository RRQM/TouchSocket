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
/// WebSocket消息结构体，实现了IDisposable接口，用于处理WebSocket消息的生命周期。
/// </summary>
public struct WebSocketMessage : IDisposable
{
    private readonly Action m_disposeAction;
    private readonly ReadOnlySequence<byte> m_sequence;
    private bool m_contiguousMemory = false;
    private ContiguousMemoryBuffer m_contiguousMemoryBuffer;

    /// <summary>
    /// 初始化WebSocketMessage结构体的新实例。
    /// </summary>
    /// <param name="opcode">消息的数据类型，使用WSDataType枚举表示。</param>
    /// <param name="sequence">消息的负载数据，使用ByteBlock结构表示。</param>
    /// <param name="disposeAction">在消息释放时需要调用的动作。</param>
    public WebSocketMessage(WSDataType opcode, ReadOnlySequence<byte> sequence, Action disposeAction)
    {
        this.Opcode = opcode;
        this.m_sequence = sequence;
        this.m_disposeAction = disposeAction;
    }

    /// <summary>
    /// 获取消息的数据类型。
    /// </summary>
    public WSDataType Opcode { get; }

    /// <summary>
    /// 获取消息的负载数据。
    /// </summary>
    public ReadOnlyMemory<byte> PayloadData
    {
        get
        {
            if (this.m_contiguousMemory)
            {
                return this.m_contiguousMemoryBuffer.Memory;
            }

            this.m_contiguousMemoryBuffer = new ContiguousMemoryBuffer(this.m_sequence);
            this.m_contiguousMemory = true;
            return this.m_contiguousMemoryBuffer.Memory;
        }
    }

    public readonly ReadOnlySequence<byte> PayloadSequence => this.m_sequence;

    /// <summary>
    /// 释放消息资源。
    /// </summary>
    public readonly void Dispose()
    {
        this.m_contiguousMemoryBuffer.Dispose();
        this.m_disposeAction?.Invoke();
    }
}
