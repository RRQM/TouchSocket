//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 固定长度数据包处理适配器。
    /// </summary>
    public class FixedSizePackageAdapter : DealDataHandlingAdapter
    {
        /// <summary>
        /// 包剩余长度
        /// </summary>
        private int m_surPlusLength = 0;

        /// <summary>
        /// 临时包
        /// </summary>
        private ByteBlock m_tempByteBlock;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fixedSize">数据包的长度</param>
        public FixedSizePackageAdapter(int fixedSize)
        {
            this.FixedSize = fixedSize;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => true;

        /// <summary>
        /// 获取已设置的数据包的长度
        /// </summary>
        public int FixedSize { get; private set; }

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (this.CacheTimeoutEnable && DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.Reset();
            }
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            if (this.m_tempByteBlock == null)
            {
                this.SplitPackage(buffer, 0, r);
            }
            else
            {
                if (this.m_surPlusLength == r)
                {
                    this.m_tempByteBlock.Write(buffer, 0, this.m_surPlusLength);
                    this.PreviewHandle(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    this.m_surPlusLength = 0;
                }
                else if (this.m_surPlusLength < r)
                {
                    this.m_tempByteBlock.Write(buffer, 0, this.m_surPlusLength);
                    this.PreviewHandle(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    this.SplitPackage(buffer, this.m_surPlusLength, r);
                }
                else
                {
                    this.m_tempByteBlock.Write(buffer, 0, r);
                    this.m_surPlusLength -= r;
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                }
            }
        }

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            int dataLen = length - offset;
            if (dataLen > this.FixedSize)
            {
                throw new OverlengthException("发送的数据包长度大于FixedSize");
            }
            ByteBlock byteBlock = BytePool.GetByteBlock(this.FixedSize);

            byteBlock.Write(buffer, offset, length);
            for (int i = (int)byteBlock.Position; i < this.FixedSize; i++)
            {
                byteBlock.Buffer[i] = 0;
            }
            byteBlock.SetLength(this.FixedSize);
            try
            {
                if (isAsync)
                {
                    byte[] data = byteBlock.ToArray();
                    this.GoSend(data, 0, data.Length, true);//使用ByteBlock时不能异步发送
                }
                else
                {
                    this.GoSend(byteBlock.Buffer, 0, byteBlock.Len, false);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes, bool isAsync)
        {
            int length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }

            if (length > this.FixedSize)
            {
                throw new OverlengthException("发送的数据包长度大于FixedSize");
            }
            ByteBlock byteBlock = BytePool.GetByteBlock(this.FixedSize);

            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Array, item.Offset, item.Count);
            }

            Array.Clear(byteBlock.Buffer, byteBlock.Pos, this.FixedSize);
            byteBlock.SetLength(this.FixedSize);
            try
            {
                if (isAsync)
                {
                    byte[] data = byteBlock.ToArray();
                    this.GoSend(data, 0, data.Length, true);//使用ByteBlock时不能异步发送
                }
                else
                {
                    this.GoSend(byteBlock.Buffer, 0, byteBlock.Len, false);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(IRequestInfo requestInfo, bool isAsync)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            this.m_tempByteBlock.SafeDispose();
            this.m_tempByteBlock = null;
            this.m_surPlusLength = 0;
        }

        private void PreviewHandle(ByteBlock byteBlock)
        {
            try
            {
                this.GoReceived(byteBlock, null);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void SplitPackage(byte[] dataBuffer, int index, int r)
        {
            while (index < r)
            {
                if (r - index >= this.FixedSize)
                {
                    ByteBlock byteBlock = BytePool.GetByteBlock(this.FixedSize);
                    byteBlock.Write(dataBuffer, index, this.FixedSize);
                    this.PreviewHandle(byteBlock);
                    this.m_surPlusLength = 0;
                }
                else//半包
                {
                    this.m_tempByteBlock = BytePool.GetByteBlock(this.FixedSize);
                    this.m_surPlusLength = this.FixedSize - (r - index);
                    this.m_tempByteBlock.Write(dataBuffer, index, r - index);
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                }
                index += this.FixedSize;
            }
        }
    }
}