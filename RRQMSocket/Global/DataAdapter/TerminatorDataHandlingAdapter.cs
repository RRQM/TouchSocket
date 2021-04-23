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
        public TerminatorDataHandlingAdapter(int maxSize, string terminator)
        {
            this.MaxSize = maxSize;
            this.Terminator = terminator;
            this.Encoding = Encoding.UTF8;
            this.terminatorCode = this.Encoding.GetBytes(terminator);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="terminator"></param>
        /// <param name="encoding"></param>
        public TerminatorDataHandlingAdapter(int maxSize, string terminator, Encoding encoding)
        {
            this.MaxSize = maxSize;
            this.Terminator = terminator;
            this.Encoding = encoding;
            this.terminatorCode = this.Encoding.GetBytes(terminator);
        }

        private byte[] terminatorCode;

        /// <summary>
        /// 在未找到终止因子时，允许的最大长度
        /// </summary>
        public int MaxSize { get; private set; }

        /// <summary>
        /// 终止因子
        /// </summary>
        public string Terminator { get; private set; }

        /// <summary>
        /// 编码格式
        /// </summary>
        public Encoding Encoding { get; private set; }

        private ByteBlock tempByteBlock;

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
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
                    int length = lastIndex - startIndex - this.terminatorCode.Length + 1;
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
                    indexes.Add(i);
                }
                //if (length - i < subByteArray.Length)
                //{
                //    break;
                //}
            }

            return indexes;
        }

        private void PreviewHandle(ByteBlock byteBlock)
        {
            try
            {
                this.GoReceived(byteBlock,null);
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
        protected override void PreviewSend(byte[] buffer, int offset, int length)
        {
            int dataLen = length - offset + this.terminatorCode.Length;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(dataLen);
            byteBlock.Write(buffer, offset, length);
            byteBlock.Write(this.terminatorCode);
            this.GoSend(byteBlock.Buffer, 0, (int)byteBlock.Position);
            byteBlock.Dispose();
        }
    }
}