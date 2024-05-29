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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 终止字符数据包处理适配器，支持以任意字符、字节数组结尾的数据包。
    /// </summary>
    public class TerminatorPackageAdapter : SingleStreamDataHandlingAdapter
    {
        private readonly byte[] m_terminatorCode;
        private ByteBlock m_tempByteBlock;

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
            this.MinSize = minSize;
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
        public int MinSize { get; set; } = 0;

        /// <summary>
        /// 保留终止因子
        /// </summary>
        public bool ReserveTerminatorCode { get; set; }

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
        {
            if (this.CacheTimeoutEnable && DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.Reset();
            }
            var array = byteBlock.Memory.GetArray();
            var buffer = array.Array;
            var cacheLength = byteBlock.Length;
            if (this.m_tempByteBlock != null)
            {
                this.m_tempByteBlock.Write(buffer, 0, cacheLength);
                buffer = this.m_tempByteBlock.Memory.GetArray().Array;
                cacheLength = this.m_tempByteBlock.Position;
            }

            var indexes = buffer.IndexOfInclude(0, cacheLength, this.m_terminatorCode);
            if (indexes.Count == 0)
            {
                if (cacheLength > this.MaxPackageSize)
                {
                    throw new Exception(TouchSocketCoreResource.ValueMoreThan.Format(nameof(cacheLength), this.MaxPackageSize));
                }
                else if (this.m_tempByteBlock == null)
                {
                    this.m_tempByteBlock = new ByteBlock(cacheLength * 2);
                    this.m_tempByteBlock.Write(buffer, 0, cacheLength);
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                }
            }
            else
            {
                var startIndex = 0;
                foreach (var lastIndex in indexes)
                {
                    var length = this.ReserveTerminatorCode ? lastIndex - startIndex + 1 : lastIndex - startIndex - this.m_terminatorCode.Length + 1;
                    var packageByteBlock = new ByteBlock(length);
                    packageByteBlock.Write(buffer, startIndex, length);

                    //var mes = Encoding.UTF8.GetString(packageByteBlock.Buffer, 0, packageByteBlock.Position);

                    await this.PreviewHandle(packageByteBlock).ConfigureFalseAwait();
                    startIndex = lastIndex + 1;
                }
                this.Reset();
                if (startIndex < cacheLength)
                {
                    this.m_tempByteBlock = new ByteBlock((cacheLength - startIndex) * 2);
                    this.m_tempByteBlock.Write(buffer, startIndex, cacheLength - startIndex);
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                }
            }
        }

        ///// <summary>
        ///// 预处理
        ///// </summary>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        //protected override void PreviewSend(byte[] buffer, int offset, int length)
        //{
        //    if (length > this.MaxPackageSize)
        //    {
        //        throw new Exception(TouchSocketCoreResource.ValueMoreThan.Format(nameof(length), this.MaxPackageSize));
        //    }
        //    var dataLen = length - offset + this.m_terminatorCode.Length;
        //    var byteBlock = new ByteBlock(dataLen);
        //    byteBlock.Write(buffer, offset, length);
        //    byteBlock.Write(this.m_terminatorCode);

        //    try
        //    {
        //        this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
        //    }
        //    finally
        //    {
        //        byteBlock.Dispose();
        //    }
        //}

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="transferBytes"></param>
        //protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        //{
        //    var length = 0;
        //    foreach (var item in transferBytes)
        //    {
        //        length += item.Count;
        //    }
        //    if (length > this.MaxPackageSize)
        //    {
        //        throw new Exception(TouchSocketCoreResource.ValueMoreThan.Format(nameof(length), this.MaxPackageSize));
        //    }
        //    var dataLen = length + this.m_terminatorCode.Length;
        //    var byteBlock = new ByteBlock(dataLen);
        //    foreach (var item in transferBytes)
        //    {
        //        byteBlock.Write(item.Array, item.Offset, item.Count);
        //    }

        //    byteBlock.Write(this.m_terminatorCode);

        //    try
        //    {
        //        this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
        //    }
        //    finally
        //    {
        //        byteBlock.Dispose();
        //    }
        //}

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(ReadOnlyMemory<byte> memory)
        {
            if (memory.Length > this.MaxPackageSize)
            {
                throw new Exception(TouchSocketCoreResource.ValueMoreThan.Format(nameof(memory.Length), this.MaxPackageSize));
            }
            var dataLen = memory.Length + this.m_terminatorCode.Length;
            var byteBlock = new ByteBlock(dataLen);
            byteBlock.Write(memory.Span);
            byteBlock.Write(this.m_terminatorCode);

            try
            {
                await this.GoSendAsync(byteBlock.Memory).ConfigureFalseAwait();
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }
            if (length > this.MaxPackageSize)
            {
                throw new Exception(TouchSocketCoreResource.ValueMoreThan.Format(nameof(length), this.MaxPackageSize));
            }
            var dataLen = length + this.m_terminatorCode.Length;
            var byteBlock = new ByteBlock(dataLen);
            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Array, item.Offset, item.Count);
            }

            byteBlock.Write(this.m_terminatorCode);

            try
            {
                await this.GoSendAsync(byteBlock.Memory).ConfigureFalseAwait();
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
            this.m_tempByteBlock.SafeDispose();
            this.m_tempByteBlock = null;
            base.Reset();
        }

        private async Task PreviewHandle(ByteBlock byteBlock)
        {
            try
            {
                byteBlock.Position = 0;
                await this.GoReceivedAsync(byteBlock, null).ConfigureFalseAwait();
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}