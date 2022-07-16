//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System;
using System.Collections.Generic;
using System.Text;

/* 项目“TouchSocketPro (net5)”的未合并的更改
在此之前:
using System.Collections.Generic;
using System.Text;
using System;
在此之后:
using TouchSocket.Core;
using System.ByteManager;
using TouchSocket.Core.Extensions;
*/

/* 项目“TouchSocketPro (netcoreapp3.1)”的未合并的更改
在此之前:
using System.Collections.Generic;
using System.Text;
using System;
在此之后:
using TouchSocket.Core;
using System.ByteManager;
using TouchSocket.Core.Extensions;
*/

/* 项目“TouchSocketPro (netstandard2.0)”的未合并的更改
在此之前:
using System.Collections.Generic;
using System.Text;
using System;
在此之后:
using TouchSocket.Core;
using System.ByteManager;
using TouchSocket.Core.Extensions;
*/
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Extensions;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 终止字符数据包处理适配器，支持以任意字符、字节数组结尾的数据包。
    /// </summary>
    public class TerminatorPackageAdapter : DataHandlingAdapter
    {
        private int minSize = 0;

        private bool reserveTerminatorCode;

        private ByteBlock tempByteBlock;

        private byte[] terminatorCode;

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
            this.minSize = minSize;
            this.terminatorCode = terminatorCode;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => true;

        /// <summary>
        /// 即使找到了终止因子，也不会结束，默认0
        /// </summary>
        public int MinSize
        {
            get => this.minSize;
            set => this.minSize = value;
        }

        /// <summary>
        /// 保留终止因子
        /// </summary>
        public bool ReserveTerminatorCode
        {
            get => this.reserveTerminatorCode;
            set => this.reserveTerminatorCode = value;
        }

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            if (this.tempByteBlock != null)
            {
                this.tempByteBlock.Write(buffer, 0, r);
                buffer = this.tempByteBlock.Buffer;
                r = (int)this.tempByteBlock.Position;
            }

            List<int> indexes = buffer.IndexOfInclude(0, r, this.terminatorCode);
            if (indexes.Count == 0)
            {
                if (r > this.MaxPackageSize)
                {
                    if (this.tempByteBlock != null)
                    {
                        this.tempByteBlock.Dispose();
                        this.tempByteBlock = null;
                    }

                    throw new OverlengthException("在已接收数据大于设定值的情况下未找到终止因子，已放弃接收");
                }
                else if (this.tempByteBlock == null)
                {
                    this.tempByteBlock = BytePool.GetByteBlock(r * 2);
                    this.tempByteBlock.Write(buffer, 0, r);
                }
            }
            else
            {
                int startIndex = 0;
                foreach (int lastIndex in indexes)
                {
                    int length;
                    if (this.reserveTerminatorCode)
                    {
                        length = lastIndex - startIndex + 1;
                    }
                    else
                    {
                        length = lastIndex - startIndex - this.terminatorCode.Length + 1;
                    }

                    ByteBlock packageByteBlock = BytePool.GetByteBlock(length);
                    packageByteBlock.Write(buffer, startIndex, length);

                    string mes = Encoding.UTF8.GetString(packageByteBlock.Buffer, 0, (int)packageByteBlock.Position);

                    this.PreviewHandle(packageByteBlock);
                    startIndex = lastIndex + 1;
                }
                if (this.tempByteBlock != null)
                {
                    this.tempByteBlock.Dispose();
                    this.tempByteBlock = null;
                }
                if (startIndex < r)
                {
                    this.tempByteBlock = BytePool.GetByteBlock((r - startIndex) * 2);
                    this.tempByteBlock.Write(buffer, startIndex, r - startIndex);
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
            int dataLen = length - offset + this.terminatorCode.Length;
            ByteBlock byteBlock = BytePool.GetByteBlock(dataLen);
            byteBlock.Write(buffer, offset, length);
            byteBlock.Write(this.terminatorCode);

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
        protected override void PreviewSend(IList<TransferByte> transferBytes, bool isAsync)
        {
            int length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Length;
            }
            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送的数据长度大于适配器设定的最大值，接收方可能会抛弃。");
            }
            int dataLen = length + this.terminatorCode.Length;
            ByteBlock byteBlock = BytePool.GetByteBlock(dataLen);
            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Buffer, item.Offset, item.Length);
            }

            byteBlock.Write(this.terminatorCode);

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
        protected override void Reset()
        {
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
    }
}