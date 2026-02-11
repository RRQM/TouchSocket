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

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个共享的、支持引用计数的 Payload。
/// </summary>
internal sealed class SharedPayload : IDisposable
{
    private readonly ReadOnlySequence<byte> m_payload;
    private readonly byte[] m_payloadBuffer;
    private int m_referenceCount;

    /// <summary>
    /// 初始化 <see cref="SharedPayload"/> 类的新实例。
    /// </summary>
    /// <param name="payload">有效负载数据。</param>
    /// <param name="initialRefCount">初始引用计数。</param>
    public SharedPayload(ReadOnlySequence<byte> payload, int initialRefCount)
    {
        var length = (int)payload.Length;
        if (length > 0)
        {
            this.m_payloadBuffer = ArrayPool<byte>.Shared.Rent(length);
            payload.CopyTo(this.m_payloadBuffer);
            this.m_payload = new ReadOnlySequence<byte>(this.m_payloadBuffer, 0, length);
        }
        else
        {
            this.m_payload = ReadOnlySequence<byte>.Empty;
        }
        this.m_referenceCount = initialRefCount;
    }

    /// <summary>
    /// 获取有效负载。
    /// </summary>
    public ReadOnlySequence<byte> Payload => this.m_payload;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Decrement(ref this.m_referenceCount)==0)
        {
            if (this.m_payloadBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(this.m_payloadBuffer);
            }
        }
    }
}