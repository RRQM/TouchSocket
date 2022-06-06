using RRQMCore.ByteManager;
using RRQMSocket;
using System.Collections.Generic;

namespace AdapterConsoleApp
{
    /// <summary>
    /// 从原始适配器实现<see href=""="https://www.yuque.com/eo2w71/rrqm/77bcd9c825e08ab9bd0ba5908bc3e515#sGiqF"/>
    /// </summary>
    internal class RawDataHandlingAdapter : DataHandlingAdapter
    {
        /// <summary>
        /// 包剩余长度
        /// </summary>
        private byte m_surPlusLength;

        /// <summary>
        /// 临时包，此包仅当前实例储存
        /// </summary>
        private ByteBlock m_tempByteBlock;

        /// <summary>
        /// 是否支持拼接发送，为false的话可以不实现<see cref="PreviewSend(IList{TransferByte}, bool)"/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        ///  设计原则:接收时，尽量不抛出异常。
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            if (this.m_tempByteBlock == null)//如果没有临时包，则直接分包。
            {
                SplitPackage(buffer, 0, r);
            }
            else
            {
                if (m_surPlusLength == r)//接收长度正好等于剩余长度，组合完数据以后直接处理数据。
                {
                    this.m_tempByteBlock.Write(buffer, 0, m_surPlusLength);
                    PreviewHandle(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    m_surPlusLength = 0;
                }
                else if (m_surPlusLength < r)//接收长度大于剩余长度，先组合包，然后处理包，然后将剩下的分包。
                {
                    this.m_tempByteBlock.Write(buffer, 0, m_surPlusLength);
                    PreviewHandle(this.m_tempByteBlock);
                    this.m_tempByteBlock = null;
                    SplitPackage(buffer, m_surPlusLength, r);
                }
                else//接收长度小于剩余长度，无法处理包，所以必须先组合包，然后等下次接收。
                {
                    this.m_tempByteBlock.Write(buffer, 0, r);
                    m_surPlusLength -= (byte)r;
                }
            }
        }

        /// <summary>
        /// 设计原则:发送时的异常，应当直接抛出，让发送者直接捕获
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            int dataLen = length - offset;//先获取需要发送的实际数据长度
            if (dataLen > byte.MaxValue)//超长判断
            {
                throw new RRQMOverlengthException("发送数据太长。");
            }
            ByteBlock byteBlock = BytePool.GetByteBlock(64 * 1024);//从内存池申请内存块，因为此处数据绝不超过255，所以避免内存池碎片化，每次申请64K
                                                                   //ByteBlock byteBlock = BytePool.GetByteBlock(dataLen+1);//实际写法。
            try
            {
                byteBlock.Write((byte)dataLen);//先写长度
                byteBlock.Write(buffer, offset, length);//再写数据
                if (isAsync)//判断异步
                {
                    byte[] data = byteBlock.ToArray();//使用异步时不能将byteBlock.Buffer进行发送，应当ToArray成新的Byte[]。
                    this.GoSend(data, 0, data.Length, isAsync);//调用GoSend，实际发送
                }
                else
                {
                    this.GoSend(byteBlock.Buffer, 0, byteBlock.Len, isAsync);
                }
            }
            finally
            {
                byteBlock.Dispose();//释放内存块
            }
        }

        protected override void PreviewSend(IList<TransferByte> transferBytes, bool isAsync)
        {
            //暂时不实现。
        }

        protected override void Reset()
        {
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        private void PreviewHandle(ByteBlock byteBlock)
        {
            try
            {
                this.GoReceived(byteBlock, null);
            }
            finally
            {
                byteBlock.Dispose();//在框架里面将内存块释放
            }
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
                byte length = dataBuffer[index];
                int recedSurPlusLength = r - index - 1;
                if (recedSurPlusLength >= length)
                {
                    ByteBlock byteBlock = BytePool.GetByteBlock(length);
                    byteBlock.Write(dataBuffer, index + 1, length);
                    PreviewHandle(byteBlock);
                    m_surPlusLength = 0;
                }
                else//半包
                {
                    this.m_tempByteBlock = BytePool.GetByteBlock(length);
                    m_surPlusLength = (byte)(length - recedSurPlusLength);
                    this.m_tempByteBlock.Write(dataBuffer, index + 1, recedSurPlusLength);
                }
                index += length + 1;
            }
        }
    }
}