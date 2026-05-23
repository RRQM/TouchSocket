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

namespace TouchSocket.Http;

/// <summary>
/// HTTP/2 帧，包含帧头和负载数据。负载内存来自内存池，使用后须调用 <see cref="Dispose"/>。
/// </summary>
internal readonly struct Http2Frame : IDisposable
{
    private static readonly IMemoryOwner<byte> s_emptyOwner = EmptyMemoryOwner.Instance;

    private readonly IMemoryOwner<byte> m_payloadOwner;

    /// <summary>
    /// 初始化不含负载的 <see cref="Http2Frame"/>
    /// </summary>
    internal Http2Frame(Http2FrameHeader header)
    {
        this.Header = header;
        this.m_payloadOwner = s_emptyOwner;
        this.Payload = ReadOnlyMemory<byte>.Empty;
    }

    /// <summary>
    /// 初始化含负载的 <see cref="Http2Frame"/>
    /// </summary>
    internal Http2Frame(Http2FrameHeader header, IMemoryOwner<byte> owner, int length)
    {
        this.Header = header;
        this.m_payloadOwner = owner;
        this.Payload = owner.Memory.Slice(0, length);
    }

    /// <summary>帧头</summary>
    public readonly Http2FrameHeader Header;

    /// <summary>负载数据（来自内存池，Dispose 后不可继续访问）</summary>
    public readonly ReadOnlyMemory<byte> Payload;

    /// <inheritdoc/>
    public void Dispose() => this.m_payloadOwner.Dispose();

    private sealed class EmptyMemoryOwner : IMemoryOwner<byte>
    {
        public static readonly EmptyMemoryOwner Instance = new EmptyMemoryOwner();
        public Memory<byte> Memory => Memory<byte>.Empty;
        public void Dispose() { }
    }
}
