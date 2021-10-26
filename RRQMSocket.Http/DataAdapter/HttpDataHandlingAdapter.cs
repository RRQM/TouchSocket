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
using RRQMCore.ByteManager;
using RRQMCore.Helper;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http数据处理适配器
    /// </summary>
    public class HttpDataHandlingAdapter : DataHandlingAdapter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="httpType"></param>
        public HttpDataHandlingAdapter(int maxSize, HttpType httpType)
        {
            this.MaxSize = maxSize;
            this.terminatorCode = Encoding.UTF8.GetBytes("\r\n\r\n");
            this.httpType = httpType;
        }

        private HttpType httpType;
        private byte[] terminatorCode;

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public int MaxSize { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        private ByteBlock tempByteBlock;
        private ByteBlock bodyByteBlock;

        private HttpBase httpBase;

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
                buffer = this.tempByteBlock.ToArray();
                r = this.tempByteBlock.Pos;
                this.tempByteBlock.Dispose();
                this.tempByteBlock = null;
            }
            if (this.httpBase == null)
            {
                this.Split(buffer, 0, r);
            }
            else
            {
                int surLen = this.httpBase.Content_Length - this.bodyByteBlock.Len;
                if (r == surLen)
                {
                    this.bodyByteBlock.Write(buffer, 0, surLen);
                    this.PreviewHandle(this.httpBase);
                }
                else if (r > surLen)
                {
                    this.bodyByteBlock.Write(buffer, 0, surLen);
                    this.PreviewHandle(this.httpBase);
                    Split(buffer, surLen, r - surLen);
                }
                else
                {
                    this.bodyByteBlock.Write(buffer, 0, r);
                }
            }
        }

        private void Split(byte[] buffer, int offset, int length)
        {
            int index = buffer.IndexOfFirst(offset, length, this.terminatorCode);
            if (index > 0)
            {
                switch (this.httpType)
                {
                    case HttpType.Server:
                        this.httpBase = new HttpRequest();
                        break;

                    case HttpType.Client:
                        this.httpBase = new HttpResponse();
                        break;
                }

                this.httpBase.ReadHeaders(buffer, 0, index);

                if (this.httpBase.Content_Length > 0)
                {
                    this.bodyByteBlock = this.BytePool.GetByteBlock(this.httpBase.Content_Length);
                    int surLength = length - (index + 1);
                    if (surLength - this.httpBase.Content_Length == 0)
                    {
                        this.bodyByteBlock.Write(buffer, index + 1, this.httpBase.Content_Length);
                        this.PreviewHandle(this.httpBase);
                    }
                    else if (surLength - this.httpBase.Content_Length > 0)
                    {
                        this.bodyByteBlock.Write(buffer, index + 1, this.httpBase.Content_Length);
                        int indexBuffer = index + 1 + this.httpBase.Content_Length;
                        this.PreviewHandle(this.httpBase);
                        this.Split(buffer, indexBuffer, length);
                    }
                    else
                    {
                        this.bodyByteBlock.Write(buffer, index + 1, surLength);
                    }
                }
                else
                {
                    this.PreviewHandle(this.httpBase);
                }
            }
            else if (length > this.MaxSize)
            {
                if (this.tempByteBlock != null)
                {
                    this.tempByteBlock.Dispose();
                    this.tempByteBlock = null;
                }

                Logger.Debug(LogType.Error, this, "在已接收数据大于设定值的情况下未找到终止符号，已放弃接收");
                return;
            }
            else if (this.tempByteBlock == null)
            {
                this.tempByteBlock = this.BytePool.GetByteBlock(length * 2);
                this.tempByteBlock.Write(buffer, offset, length - offset);
            }
        }

        private void PreviewHandle(HttpBase httpBase)
        {
            this.httpBase = null;
            try
            {
                if (this.bodyByteBlock != null)
                {
                    httpBase.SetContent(this.bodyByteBlock.ToArray());
                    this.bodyByteBlock.Dispose();
                    this.bodyByteBlock = null;
                }
                httpBase.ReadFromBase();
                this.GoReceived(null, httpBase);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, "处理数据错误", ex);
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
            this.GoSend(buffer, offset, length, isAsync);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(IList<TransferByte> transferBytes, bool isAsync)
        {
            throw new NotImplementedException();
        }
    }
}