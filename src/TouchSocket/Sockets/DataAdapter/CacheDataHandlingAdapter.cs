using System;
using System.Linq;
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// CacheDataHandlingAdapter
    /// </summary>
    public abstract class CacheDataHandlingAdapter : DealDataHandlingAdapter
    {
        /// <summary>
        /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="ByteBlock.Dispose"/>后，再置空；
        /// </summary>
        protected ByteBlock m_cacheByteBlock;

        /// <summary>
        /// 将数据缓存起来
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected void Cache(byte[] buffer, int offset, int length)
        {
            this.m_cacheByteBlock ??= new ByteBlock(length);
            this.m_cacheByteBlock.Write(buffer, offset, length);
            if (this.UpdateCacheTimeWhenRev)
            {
                this.LastCacheTime = DateTime.Now;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            this.m_cacheByteBlock.SafeDispose();
            this.m_cacheByteBlock = null;
        }

        /// <summary>
        /// 获取当前缓存，
        /// 如果缓存超时，或者不存在，均会返回false。
        /// 如果获取成功，则会清空内部缓存。
        /// </summary>
        /// <returns></returns>
        protected bool TryGetCache(out byte[] buffer)
        {
            if (this.m_cacheByteBlock == null)
            {
                buffer = null;
                return false;
            }
            if (DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.m_cacheByteBlock.SafeDispose();
                this.m_cacheByteBlock = null;
                buffer = null;
                return false;
            }
            buffer = this.m_cacheByteBlock.ToArray();
            this.m_cacheByteBlock.SafeDispose();
            this.m_cacheByteBlock = null;
            return true;
        }

        /// <summary>
        /// 获取缓存，注意：获取的ByteBlock需要手动释放。
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        protected bool TryGetCache(out ByteBlock byteBlock)
        {
            if (this.m_cacheByteBlock == null)
            {
                byteBlock = null;
                return false;
            }
            if (DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.m_cacheByteBlock.SafeDispose();
                this.m_cacheByteBlock = null;
                byteBlock = null;
                return false;
            }
            byteBlock = this.m_cacheByteBlock;
            return true;
        }
    }
}