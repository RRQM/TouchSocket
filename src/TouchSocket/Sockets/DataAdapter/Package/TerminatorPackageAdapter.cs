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
using System.Text;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.Log;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 终止字符数据包处理适配器，支持以任意字符、字节数组结尾的数据包。
    /// </summary>
    public class TerminatorPackageAdapter : DealDataHandlingAdapter
    {
        private int m_minSize = 0;

        private bool m_reserveTerminatorCode;

        private ByteBlock m_tempByteBlock;

        private readonly byte[] m_terminatorCode;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="terminator"></param>
        public TerminatorPackageAdapter(string terminator) : this(0, Encoding.UTF8.GetBytes(terminator))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="terminator"></param>
        /// <param name="encoding"></param>
        public TerminatorPackageAdapter(string terminator, Encoding encoding)
            : this(0, encoding.GetBytes(terminator))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="minSize"></param>
        /// <param name="terminatorCode"></param>
        public TerminatorPackageAdapter(int minSize, byte[] terminatorCode)
        {
            this.m_minSize = minSize;
            this.m_terminatorCode = terminatorCode;
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
        /// 即使找到了终止因子，也不会结束，默认0
        /// </summary>
        public int MinSize
        {
            get => this.m_minSize;
            set => this.m_minSize = value;
        }

        /// <summary>
        /// 保留终止因子
        /// </summary>
        public bool ReserveTerminatorCode
        {
            get => this.m_reserveTerminatorCode;
            set => this.m_reserveTerminatorCode = value;
        }

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
            if (this.m_tempByteBlock != null)
            {
                this.m_tempByteBlock.Write(buffer, 0, r);
                buffer = this.m_tempByteBlock.Buffer;
                r = (int)this.m_tempByteBlock.Position;
            }

            List<int> indexes = buffer.IndexOfInclude(0, r, this.m_terminatorCode);
            if (indexes.Count == 0)
            {
                if (r > this.MaxPackageSize)
                {
                    this.Reset();
                    this.Client?.Logger.Error("在已接收数据大于设定值的情况下未找到终止因子，已放弃接收");
                }
                else if (this.m_tempByteBlock == null)
                {
                    this.m_tempByteBlock = BytePool.GetByteBlock(r * 2);
                    this.m_tempByteBlock.Write(buffer, 0, r);
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                }
            }
            else
            {
                int startIndex = 0;
                foreach (int lastIndex in indexes)
                {
                    int length;
                    if (this.m_reserveTerminatorCode)
                    {
                        length = lastIndex - startIndex + 1;
                    }
                    else
                    {
                        length = lastIndex - startIndex - this.m_terminatorCode.Length + 1;
                    }

                    ByteBlock packageByteBlock = BytePool.GetByteBlock(length);
                    packageByteBlock.Write(buffer, startIndex, length);

                    string mes = Encoding.UTF8.GetString(packageByteBlock.Buffer, 0, (int)packageByteBlock.Position);

                    this.PreviewHandle(packageByteBlock);
                    startIndex = lastIndex + 1;
                }
                this.Reset();
                if (startIndex < r)
                {
                    this.m_tempByteBlock = BytePool.GetByteBlock((r - startIndex) * 2);
                    this.m_tempByteBlock.Write(buffer, startIndex, r - startIndex);
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
            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送的数据长度大于适配器设定的最大值，接收方可能会抛弃。");
            }
            int dataLen = length - offset + this.m_terminatorCode.Length;
            ByteBlock byteBlock = BytePool.GetByteBlock(dataLen);
            byteBlock.Write(buffer, offset, length);
            byteBlock.Write(this.m_terminatorCode);

            try
            {
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
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes, bool isAsync)
        {
            int length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }
            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送的数据长度大于适配器设定的最大值，接收方可能会抛弃。");
            }
            int dataLen = length + this.m_terminatorCode.Length;
            ByteBlock byteBlock = BytePool.GetByteBlock(dataLen);
            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Array, item.Offset, item.Count);
            }

            byteBlock.Write(this.m_terminatorCode);

            try
            {
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
    }
}