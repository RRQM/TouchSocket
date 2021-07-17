//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 固定长度数据处理器
    /// </summary>
    public class FixedSizeDataHandlingAdapter : DataHandlingAdapter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fixedSize">数据包的长度</param>
        public FixedSizeDataHandlingAdapter(int fixedSize)
        {
            this.FixedSize = fixedSize;
        }

        /// <summary>
        /// 获取已设置的数据包的长度
        /// </summary>
        public int FixedSize { get; private set; }

        /// <summary>
        /// 临时包
        /// </summary>
        private ByteBlock tempByteBlock;

        /// <summary>
        /// 包剩余长度
        /// </summary>
        private int surPlusLength = 0;

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            if (this.tempByteBlock == null)
            {
                SplitPackage(buffer, 0, r);
            }
            else
            {
                if (surPlusLength == r)
                {
                    this.tempByteBlock.Write(buffer, 0, surPlusLength);
                    PreviewHandle(this.tempByteBlock);
                    this.tempByteBlock = null;
                    surPlusLength = 0;
                }
                else if (surPlusLength < r)
                {
                    this.tempByteBlock.Write(buffer, 0, surPlusLength);
                    PreviewHandle(this.tempByteBlock);
                    this.tempByteBlock = null;
                    SplitPackage(buffer, surPlusLength, r);
                }
                else
                {
                    this.tempByteBlock.Write(buffer, 0, r);
                    surPlusLength -= r;
                }
            }
        }

        private void SplitPackage(byte[] dataBuffer, int index, int r)
        {
            while (index < r)
            {
                if (r - index >= this.FixedSize)
                {
                    ByteBlock byteBlock = this.BytePool.GetByteBlock(this.FixedSize);
                    byteBlock.Write(dataBuffer, index, this.FixedSize);
                    PreviewHandle(byteBlock);
                    surPlusLength = 0;
                }
                else//半包
                {
                    this.tempByteBlock = this.BytePool.GetByteBlock(this.FixedSize);
                    surPlusLength = this.FixedSize - (r - index);
                    this.tempByteBlock.Write(dataBuffer, index, r - index);
                }
                index += this.FixedSize;
            }
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
                throw new RRQMOverlengthException("发送的数据包长度大于FixedSize");
            }
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.FixedSize);

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
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}