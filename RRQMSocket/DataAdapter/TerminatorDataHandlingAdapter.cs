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
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket
{
    /// <summary>
    /// 终止字符处理器
    /// </summary>
    public class TerminatorDataHandlingAdapter : DataHandlingAdapter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="terminator"></param>
        public TerminatorDataHandlingAdapter(int maxSize, string terminator) : this(maxSize, 0, Encoding.UTF8.GetBytes(terminator))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="terminator"></param>
        /// <param name="encoding"></param>
        public TerminatorDataHandlingAdapter(int maxSize, string terminator, Encoding encoding)
            : this(maxSize, 0, encoding.GetBytes(terminator))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="minSize"></param>
        /// <param name="terminatorCode"></param>
        public TerminatorDataHandlingAdapter(int maxSize, int minSize, byte[] terminatorCode)
        {
            this.maxSize = maxSize;
            this.minSize = minSize;
            this.terminatorCode = terminatorCode;
        }

        private byte[] terminatorCode;

        private int maxSize = 1024;

        /// <summary>
        /// 在未找到终止因子时，允许的最大长度，默认1024
        /// </summary>
        public int MaxSize
        {
            get { return maxSize; }
            set { maxSize = value; }
        }

        private int minSize = 0;

        /// <summary>
        /// 即使找到了终止因子，也不会结束，默认0
        /// </summary>
        public int MinSize
        {
            get { return minSize; }
            set { minSize = value; }
        }

        private bool reserveTerminatorCode;

        /// <summary>
        /// 保留终止因子
        /// </summary>
        public bool ReserveTerminatorCode
        {
            get { return reserveTerminatorCode; }
            set { reserveTerminatorCode = value; }
        }

        private ByteBlock tempByteBlock;

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

            List<int> indexes = this.IndexOfInclude(buffer, r, this.terminatorCode);
            if (indexes.Count == 0)
            {
                if (r > this.MaxSize)
                {
                    if (this.tempByteBlock != null)
                    {
                        this.tempByteBlock.Dispose();
                        this.tempByteBlock = null;
                    }

                    Logger.Debug(LogType.Error, this, "在已接收数据大于设定值的情况下未找到终止因子，已放弃接收");
                    return;
                }
                else if (this.tempByteBlock == null)
                {
                    this.tempByteBlock = this.BytePool.GetByteBlock(r * 2);
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

                    ByteBlock packageByteBlock = this.BytePool.GetByteBlock(length);
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
                    this.tempByteBlock = this.BytePool.GetByteBlock((r - startIndex) * 2);
                    this.tempByteBlock.Write(buffer, startIndex, r - startIndex);
                }
            }
        }

        private List<int> IndexOfInclude(byte[] srcByteArray, int length, byte[] subByteArray)
        {
            int subByteArrayLen = subByteArray.Length;
            List<int> indexes = new List<int>();
            if (length < subByteArrayLen)
            {
                return indexes;
            }
            int hitLength = 0;
            for (int i = 0; i < length; i++)
            {
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
                else
                {
                    hitLength = 0;
                }

                if (hitLength == subByteArray.Length)
                {
                    hitLength = 0;
                    if (indexes.Count == 0)
                    {
                        if (i >= this.minSize)
                        {
                            indexes.Add(i);
                        }
                    }
                    else if (i - indexes[indexes.Count - 1] >= this.minSize)
                    {
                        indexes.Add(i);
                    }
                }
            }

            return indexes;
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
            int dataLen = length - offset + this.terminatorCode.Length;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(dataLen);
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