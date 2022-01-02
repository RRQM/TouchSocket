//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using System;
using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 固定包头数据处理器
    /// </summary>
    public class FixedHeaderDataHandlingAdapter : DataHandlingAdapter
    {
        private int maxSizeHeader = 1024 * 1024 * 10;

        /// <summary>
        /// 获取或设置包头的最大值（默认为10Mb）
        /// </summary>
        public int MaxSizeHeader
        {
            get { return maxSizeHeader; }
            set { maxSizeHeader = value; }
        }

        private int minSizeHeader = 0;

        /// <summary>
        /// 获取或设置包头的最小值（默认为0）
        /// </summary>
        public int MinSizeHeader
        {
            get { return minSizeHeader; }
            set { minSizeHeader = value; }
        }

        private FixedHeaderType fixedHeaderType = FixedHeaderType.Int;

        /// <summary>
        /// 设置包头类型，默认为int
        /// </summary>
        public FixedHeaderType FixedHeaderType
        {
            get { return fixedHeaderType; }
            set { fixedHeaderType = value; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => true;

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
            int r = byteBlock.Len;

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
        private void SeamPackage(byte[] buffer, int r)
        {
            ByteBlock byteBlock = BytePool.GetByteBlock(r + agreementTempBytes.Length);
            byteBlock.Write(agreementTempBytes);
            byteBlock.Write(buffer, 0, r);
            r += agreementTempBytes.Length;
            agreementTempBytes = null;
            SplitPackage(byteBlock.Buffer, 0, r);
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
                if (r - index <= (byte)this.fixedHeaderType)
                {
                    agreementTempBytes = new byte[r - index];
                    Array.Copy(dataBuffer, index, agreementTempBytes, 0, agreementTempBytes.Length);
                    return;
                }
                int length = 0;

                switch (this.fixedHeaderType)
                {
                    case FixedHeaderType.Byte:
                        length = dataBuffer[index];
                        break;

                    case FixedHeaderType.Ushort:
                        length = RRQMBitConverter.Default.ToUInt16(dataBuffer, index);
                        break;

                    case FixedHeaderType.Int:
                        length = RRQMBitConverter.Default.ToInt32(dataBuffer, index);
                        break;
                }

                if (length < 0)
                {
                    throw new RRQMException("接收数据长度错误，已放弃接收");
                }
                else if (length < this.minSizeHeader)
                {
                    throw new RRQMException("接收数据长度小于设定值，已放弃接收");
                }
                else if (length > this.maxSizeHeader)
                {
                    throw new RRQMException("接收数据长度大于设定值，已放弃接收");
                }

                int recedSurPlusLength = r - index - (byte)this.fixedHeaderType;
                if (recedSurPlusLength >= length)
                {
                    ByteBlock byteBlock = BytePool.GetByteBlock(length);
                    byteBlock.Write(dataBuffer, index + (byte)this.fixedHeaderType, length);
                    PreviewHandle(byteBlock);
                    surPlusLength = 0;
                }
                else//半包
                {
                    this.tempByteBlock = BytePool.GetByteBlock(length);
                    surPlusLength = length - recedSurPlusLength;
                    this.tempByteBlock.Write(dataBuffer, index + (byte)this.fixedHeaderType, recedSurPlusLength);
                }
                index += (length + (byte)this.fixedHeaderType);
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
        /// 当发送数据前处理数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (length < this.MinSizeHeader)
            {
                throw new RRQMException("发送数据小于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            if (length > this.MaxSizeHeader)
            {
                throw new RRQMException("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            ByteBlock byteBlock = null;
            byte[] lenBytes = null;

            switch (this.fixedHeaderType)
            {
                case FixedHeaderType.Byte:
                    {
                        byte dataLen = (byte)(length - offset);
                        byteBlock = BytePool.GetByteBlock(dataLen + 1);
                        lenBytes = new byte[] { dataLen };
                        break;
                    }
                case FixedHeaderType.Ushort:
                    {
                        ushort dataLen = (ushort)(length - offset);
                        byteBlock = BytePool.GetByteBlock(dataLen + 2);
                        lenBytes = RRQMBitConverter.Default.GetBytes(dataLen);
                        break;
                    }
                case FixedHeaderType.Int:
                    {
                        int dataLen = length - offset;
                        byteBlock = BytePool.GetByteBlock(dataLen + 4);
                        lenBytes = RRQMBitConverter.Default.GetBytes(dataLen);
                        break;
                    }
            }

            try
            {
                byteBlock.Write(lenBytes);
                byteBlock.Write(buffer, offset, length);
                if (isAsync)
                {
                    byte[] data = byteBlock.ToArray();
                    this.GoSend(data, 0, data.Length, isAsync);//使用ByteBlock时不能异步发送
                }
                else
                {
                    this.GoSend(byteBlock.Buffer, 0, byteBlock.Len, isAsync);
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
        protected override void PreviewSend(IList<TransferByte> transferBytes, bool isAsync)
        {
            int length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Length;
            }

            if (length < this.minSizeHeader)
            {
                throw new RRQMException("发送数据小于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            if (length > this.maxSizeHeader)
            {
                throw new RRQMException("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            ByteBlock byteBlock = null;
            byte[] lenBytes = null;

            switch (this.fixedHeaderType)
            {
                case FixedHeaderType.Byte:
                    {
                        byte dataLen = (byte)length;
                        byteBlock = BytePool.GetByteBlock(dataLen + 1);
                        lenBytes = new byte[] { dataLen };
                        break;
                    }
                case FixedHeaderType.Ushort:
                    {
                        ushort dataLen = (ushort)length;
                        byteBlock = BytePool.GetByteBlock(dataLen + 2);
                        lenBytes = RRQMBitConverter.Default.GetBytes(dataLen);
                        break;
                    }
                case FixedHeaderType.Int:
                    {
                        byteBlock = BytePool.GetByteBlock(length + 4);
                        lenBytes = RRQMBitConverter.Default.GetBytes(length);
                        break;
                    }
            }

            try
            {
                byteBlock.Write(lenBytes);

                foreach (var item in transferBytes)
                {
                    byteBlock.Write(item.Buffer, item.Offset, item.Length);
                }

                if (isAsync)
                {
                    byte[] data = byteBlock.ToArray();
                    this.GoSend(data, 0, data.Length, isAsync);//使用ByteBlock时不能异步发送
                }
                else
                {
                    this.GoSend(byteBlock.Buffer, 0, byteBlock.Len, isAsync);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}