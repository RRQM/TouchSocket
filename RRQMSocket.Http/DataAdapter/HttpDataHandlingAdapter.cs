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
        public HttpDataHandlingAdapter(int maxSize)
        {
            this.MaxSize = maxSize;
            this.terminatorCode = Encoding.UTF8.GetBytes("\r\n\r\n");
        }

        private byte[] terminatorCode;

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public int MaxSize { get; private set; }

        private ByteBlock tempByteBlock;

        private HttpRequest httpRequest;

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

            if (this.httpRequest == null)
            {
                int index = buffer.IndexOfFirst(0, r, this.terminatorCode);
                if (index > 0)
                {
                    this.httpRequest = new HttpRequest();
                    this.httpRequest.ReadHeaders(buffer, 0, index);

                    if (this.httpRequest.Content_Length > 0)
                    {
                        this.httpRequest.Body = this.BytePool.GetByteBlock(this.httpRequest.Content_Length);
                        this.httpRequest.Body.Write(buffer, index + 1, r - (index + 1));
                        if (this.httpRequest.Body.Length == this.httpRequest.Content_Length)
                        {
                            this.PreviewHandle(this.httpRequest);
                        }
                    }
                    else
                    {
                        this.PreviewHandle(this.httpRequest);
                    }
                }
                else if (r > this.MaxSize)
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
                    this.tempByteBlock = this.BytePool.GetByteBlock(r * 2);
                    this.tempByteBlock.Write(buffer, 0, r);
                }
            }
            else
            {
                if (r >= this.httpRequest.Content_Length - this.httpRequest.Body.Length)
                {
                    this.httpRequest.Body.Write(buffer, 0, this.httpRequest.Content_Length - (int)this.httpRequest.Body.Length);
                    this.PreviewHandle(this.httpRequest);
                }
            }
        }

        private void PreviewHandle(HttpRequest httpRequest)
        {
            this.httpRequest = null;
            try
            {
                httpRequest.Build();
                this.GoReceived(null, httpRequest);
            }
            finally
            {
                httpRequest.Dispose();
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
            this.GoSend(buffer, offset, length);
        }
    }
}