//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Log;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 固定包头数据处理器
    /// </summary>
    public class FixedHeaderDataHandlingAdapter : DataHandlingAdapter
    {
        /// <summary>
        /// 获取或设置包头的最大值（默认为20Mb）
        /// </summary>
        public int MaxSizeHeader { get; set; } = 1024 * 1024 * 20;

        /// <summary>
        /// 获取或设置包头的最小值（默认为0）
        /// </summary>
        public int MinSizeHeader { get; set; }

        /// <summary>
        /// 临时包
        /// </summary>
        private ByteBlock tempByteBlock;

        /// <summary>
        /// 包剩余长度
        /// </summary>
        private int surPlusLength = 0;

        /// <summary>
        /// 协议临时包
        /// </summary>
        private byte[] agreementTempBytes;

        /// <summary>
        /// 当接收到数据时处理数据
        /// </summary>
        /// <param name="byteBlock">数据流</param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;

            if (agreementTempBytes != null)
            {
                SeamPackage(buffer, r);
            }
            else if (this.tempByteBlock == null)
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

        /// <summary>
        /// 缝合包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="r"></param>
        private void SeamPackage( byte[] buffer, int r)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(r + agreementTempBytes.Length);
            byteBlock.Write(agreementTempBytes);
            byteBlock.Write(buffer, 0, r);
            if (byteBlock.Length > 4)
            {
                r += agreementTempBytes.Length;
                agreementTempBytes = null;
                SplitPackage(byteBlock.Buffer, 0, r);
            }
            else
            {
                agreementTempBytes = byteBlock.ToArray();
            }
            byteBlock.Dispose();
        }

        /// <summary>
        /// 分解包
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="index"></param>
        /// <param name="r"></param>
        private void SplitPackage(byte[] dataBuffer, int index, int r)
        {
            while (index < r)
            {
                if (r - index <= 4)
                {
                    agreementTempBytes = new byte[r - index];
                    Array.Copy(dataBuffer, index, agreementTempBytes, 0, agreementTempBytes.Length);
                    return;
                }
                int length = BitConverter.ToInt32(dataBuffer, index);

                if (length < 4)
                {
                    Logger.Debug(LogType.Error, this, "接收数据长度错误，已放弃接收");
                    return;
                }
                else if (length < this.MinSizeHeader)
                {
                    Logger.Debug(LogType.Error, this, "接收数据长度小于设定值，已放弃接收");
                    return;
                }
                else if (length > this.MaxSizeHeader)
                {
                    Logger.Debug(LogType.Error, this, "接收数据长度大于设定值，已放弃接收");
                    return;
                }
                if (r - index >= length)
                {
                    ByteBlock byteBlock = this.BytePool.GetByteBlock(length - 4);
                    byteBlock.Write(dataBuffer, index + 4, length - 4);
                    PreviewHandle(byteBlock);
                    surPlusLength = 0;
                }
                else//半包
                {
                    this.tempByteBlock = this.BytePool.GetByteBlock(length - 4);
                    surPlusLength = length - (r - index);
                    this.tempByteBlock.Write(dataBuffer, index + 4, r - (index + 4));
                }
                index += length;
            }
        }

        private void PreviewHandle(ByteBlock byteBlock)
        {
            try
            {
                this.GoReceived(byteBlock);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 当发送数据前处理数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length)
        {
            int dataLen = length - offset + 4;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(dataLen);
            byte[] lenBytes = BitConverter.GetBytes(dataLen);
            byteBlock.Write(lenBytes);
            byteBlock.Write(buffer, offset, length);
            this.GoSend(byteBlock.Buffer, 0, (int)byteBlock.Position);
            byteBlock.Dispose();
        }
    }
}