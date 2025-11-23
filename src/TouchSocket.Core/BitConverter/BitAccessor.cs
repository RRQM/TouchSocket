// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TouchSocket.Core;

/// <summary>
/// 位访问器：用于对非托管类型 <typeparamref name="T"/> 的位进行读取与设置。
/// </summary>
/// <typeparam name="T">非托管类型。</typeparam>
public readonly ref struct BitAccessor<T> where T : unmanaged
{
    private readonly Span<byte> m_bytes;

    /// <summary>
    /// 初始化 <see cref="BitAccessor{T}"/> 实例，允许对 <typeparamref name="T"/> 类型的数据进行位访问。
    /// </summary>
    /// <param name="data">需要进行位访问的非托管类型数据引用。</param>
    public BitAccessor(ref T data)
    {
        this.m_bytes = MemoryMarshal.AsBytes(EasyMemoryMarshal.CreateSpan(ref data, 1));
    }

    /// <summary>
    /// 获取位总长度（单位：位）。
    /// </summary>
    public int Length => this.m_bytes.Length * 8;

    /// <summary>
    /// 读取指定索引处的位值。
    /// </summary>
    /// <param name="index">位索引（从 0 开始）。</param>
    /// <returns>若该位为 1，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引超出范围时抛出。</exception>
    public bool Get(int index)
    {
        var byteIndex = index / 8;
        var bitInByte = index % 8;
        if ((uint)byteIndex >= (uint)this.m_bytes.Length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(index), index, this.Length);
        }
        return (this.m_bytes[byteIndex] & (1 << bitInByte)) != 0;
    }

    /// <summary>
    /// 将指定索引处的位设置为给定布尔值。
    /// </summary>
    /// <param name="index">位索引（从 0 开始）。</param>
    /// <param name="value">为 <see langword="true"/> 时置 1；为 <see langword="false"/> 时置 0。</param>
    /// <exception cref="ArgumentOutOfRangeException">当索引超出范围时抛出。</exception>
    public void Set(int index, bool value)
    {
        var byteIndex = index / 8;
        var bitInByte = index % 8;
        if ((uint)byteIndex >= (uint)this.m_bytes.Length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(index), index, this.Length);
        }

        if (value)
        {
            this.m_bytes[byteIndex] = (byte)(this.m_bytes[byteIndex] | (1 << bitInByte));
        }
        else
        {
            this.m_bytes[byteIndex] = (byte)(this.m_bytes[byteIndex] & ~(1 << bitInByte));
        }
    }
}