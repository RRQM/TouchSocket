//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
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
    /// 固定包头数据包处理适配器，支持Byte、UShort、Int三种类型作为包头。使用<see cref="RRQMBitConverter.DefaultEndianType"/>大小端设置。
    /// </summary>
    public class FixedHeaderPackageAdapter : DataHandlingAdapter
    {
        private byte[] agreementTempBytes;//协议临时包
        private FixedHeaderType fixedHeaderType = FixedHeaderType.Int;
        private int maxPackageSize = 1024 * 1024 * 10;
        private int minPackageSize = 0;
        private int surPlusLength = 0;//包剩余长度
        private ByteBlock tempByteBlock;//临时包

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => true;

        /// <summary>
        /// 设置包头类型，默认为int
        /// </summary>
        public FixedHeaderType FixedHeaderType
        {
            get => this.fixedHeaderType;
            set => this.fixedHeaderType = value;
        }

        /// <summary>
        /// 获取或设置包数据的最大值（默认为1024*1024*10）
        /// </summary>
        public int MaxPackageSize
        {
            get => this.maxPackageSize;
            set => this.maxPackageSize = value;
        }

        /// <summary>
        /// 获取或设置包数据的最小值（默认为0）
        /// </summary>
        public int MinPackageSize
        {
            get => this.minPackageSize;
            set => this.minPackageSize = value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dataResult"></param>
        /// <returns></returns>
        protected override bool OnReceivingError(DataResult dataResult)
        {
            this.Owner.Logger.Debug(RRQMCore.Log.LogType.Error, this, dataResult.Message, null);
            return true;
        }

        /// <summary>
        /// 当接收到数据时处理数据
        /// </summary>
        /// <param name="byteBlock">数据流</param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;

            if (this.agreementTempBytes != null)
            {
                this.SeamPackage(buffer, r);
            }
            else if (this.tempByteBlock == null)
            {
                this.SplitPackage(buffer, 0, r);
            }
            else
            {
                if (this.surPlusLength == r)
                {
                    this.tempByteBlock.Write(buffer, 0, this.surPlusLength);
                    this.PreviewHandle(this.tempByteBlock);
                    this.tempByteBlock = null;
                    this.surPlusLength = 0;
                }
                else if (this.surPlusLength < r)
                {
                    this.tempByteBlock.Write(buffer, 0, this.surPlusLength);
                    this.PreviewHandle(this.tempByteBlock);
                    this.tempByteBlock = null;
                    this.SplitPackage(buffer, this.surPlusLength, r);
                }
                else
                {
                    this.tempByteBlock.Write(buffer, 0, r);
                    this.surPlusLength -= r;
                }
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
            if (length < this.MinPackageSize)
            {
                throw new RRQMException("发送数据小于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            if (length > this.MaxPackageSize)
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

            if (length < this.minPackageSize)
            {
                throw new RRQMException("发送数据小于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            if (length > this.maxPackageSize)
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            this.agreementTempBytes = null;
            this.surPlusLength = default;
            this.tempByteBlock = null;
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
        /// 缝合包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="r"></param>
        private void SeamPackage(byte[] buffer, int r)
        {
            ByteBlock byteBlock = BytePool.GetByteBlock(r + this.agreementTempBytes.Length);
            byteBlock.Write(this.agreementTempBytes);
            byteBlock.Write(buffer, 0, r);
            r += this.agreementTempBytes.Length;
            this.agreementTempBytes = null;
            this.SplitPackage(byteBlock.Buffer, 0, r);
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
                    this.agreementTempBytes = new byte[r - index];
                    Array.Copy(dataBuffer, index, this.agreementTempBytes, 0, this.agreementTempBytes.Length);
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
                else if (length < this.minPackageSize)
                {
                    throw new RRQMException("接收数据长度小于设定值，已放弃接收");
                }
                else if (length > this.maxPackageSize)
                {
                    throw new RRQMException("接收数据长度大于设定值，已放弃接收");
                }

                int recedSurPlusLength = r - index - (byte)this.fixedHeaderType;
                if (recedSurPlusLength >= length)
                {
                    ByteBlock byteBlock = BytePool.GetByteBlock(length);
                    byteBlock.Write(dataBuffer, index + (byte)this.fixedHeaderType, length);
                    this.PreviewHandle(byteBlock);
                    this.surPlusLength = 0;
                }
                else//半包
                {
                    this.tempByteBlock = BytePool.GetByteBlock(length);
                    this.surPlusLength = length - recedSurPlusLength;
                    this.tempByteBlock.Write(dataBuffer, index + (byte)this.fixedHeaderType, recedSurPlusLength);
                }
                index += (length + (byte)this.fixedHeaderType);
            }
        }
    }
}