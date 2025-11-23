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

//using System;
//using System.Buffers;
//using System.Data.SqlTypes;
//using System.Threading.Tasks;

//namespace TouchSocket.Core;

///// <summary>
///// CacheDataHandlingAdapter
///// </summary>
//public abstract class CacheDataHandlingAdapter : SingleStreamDataHandlingAdapter
//{
//    /// <summary>
//    /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="IDisposable.Dispose"/>后，再置空；
//    /// </summary>
//    private SegmentedBytesWriter m_cacheByteBlock;

//    /// <summary>
//    /// 将数据缓存起来
//    /// </summary>
//    protected void Cache<TReader>(ref TReader reader)
//        where TReader :  IBytesReader
//    {
//        var sequence = reader.Sequence;
//        this.m_cacheByteBlock ??= new SegmentedBytesWriter((int)sequence.Length);
//        foreach (var item in sequence)
//        {
//            this.m_cacheByteBlock.Write(item.Span);
//        }

//        if (this.UpdateCacheTimeWhenRev)
//        {
//            this.LastCacheTime = DateTimeOffset.UtcNow;
//        }
//    }

//    /// <inheritdoc/>
//    protected override void Reset()
//    {
//        this.m_cacheByteBlock.SafeDispose();
//        this.m_cacheByteBlock = null;
//        base.Reset();
//    }

//    private long m_cacheSize;

//    protected sealed override Task PreviewReceivedAsync<TReader>(TReader reader)
//    {

//    }

//    protected abstract Task PreviewReceivedAsyncAfterCacheVerification<TReader>(ref TReader reader)
//        where TReader : IBytesReader;
//}