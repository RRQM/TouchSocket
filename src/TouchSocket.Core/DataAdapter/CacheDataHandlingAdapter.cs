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

namespace TouchSocket.Core;

/// <summary>
/// CacheDataHandlingAdapter
/// </summary>
public abstract class CacheDataHandlingAdapter : SingleStreamDataHandlingAdapter
{
    /// <summary>
    /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="IDisposable.Dispose"/>后，再置空；
    /// </summary>
    private ByteBlock m_cacheByteBlock;

    /// <summary>
    /// 将数据缓存起来
    /// </summary>
    protected void Cache(ReadOnlySpan<byte> span)
    {
        this.m_cacheByteBlock ??= new ByteBlock(span.Length);
        this.m_cacheByteBlock.Write(span);
        if (this.UpdateCacheTimeWhenRev)
        {
            this.LastCacheTime = DateTimeOffset.UtcNow;
        }
    }

    /// <inheritdoc/>
    protected override void Reset()
    {
        this.m_cacheByteBlock.SafeDispose();
        this.m_cacheByteBlock = null;
        base.Reset();
    }

    protected bool TryCombineCache(ReadOnlySpan<byte> span, out IByteBlock cacheByteBlock)
    {
        if (this.m_cacheByteBlock == null)
        {
            cacheByteBlock = default;
            return false;
        }

        if (this.CacheTimeoutEnable && DateTimeOffset.UtcNow - this.LastCacheTime > this.CacheTimeout)
        {
            this.m_cacheByteBlock.SafeDispose();
            this.m_cacheByteBlock = null;
            cacheByteBlock = default;
            return false;
        }

        this.m_cacheByteBlock.Write(span);
        cacheByteBlock = this.m_cacheByteBlock;
        ReaderExtension.SeekToStart(ref cacheByteBlock);
        this.m_cacheByteBlock = default;
        return true;
    }
}