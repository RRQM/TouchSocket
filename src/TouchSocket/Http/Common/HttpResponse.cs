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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http响应
    /// </summary>
    public class HttpResponse : HttpBase
    {
        private bool m_canRead;
        private bool m_canWrite;
        private ITcpClientBase m_client;
        private byte[] m_content;
        private GetContentResult m_getContentResult;
        private bool m_responsed;
        private bool m_sentHeader;
        private long m_sentLength;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        /// <param name="isServer"></param>
        public HttpResponse(ITcpClientBase client, bool isServer = true)
        {
            this.m_client = client;
            if (isServer)
            {
                this.m_canRead = false;
                this.m_canWrite = true;
            }
            else
            {
                this.m_canRead = true;
                this.m_canWrite = false;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpResponse()
        {
            this.m_canRead = false;
            this.m_canWrite = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanRead => this.m_canRead;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanWrite => this.m_canWrite;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override ITcpClientBase Client => this.m_client;

        /// <summary>
        /// 是否分块
        /// </summary>
        public bool IsChunk { get; set; }

        /// <summary>
        /// 是否已经响应数据。
        /// </summary>
        public bool Responsed => this.m_responsed;

        /// <summary>
        /// 状态码，默认200
        /// </summary>
        public string StatusCode { get; set; } = "200";

        /// <summary>
        /// 状态消息，默认Success
        /// </summary>
        public string StatusMessage { get; set; } = "Success";

        /// <summary>
        /// 构建数据并回应。
        /// <para>该方法仅在具有Client实例时有效。</para>
        /// </summary>
        public void Answer()
        {
            using (ByteBlock byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                this.m_client.DefaultSend(byteBlock);
                this.m_responsed = true;
            }
        }

        /// <summary>
        ///  构建响应数据。
        /// <para>当数据较大时，不建议这样操作，可直接<see cref="WriteContent(byte[], int, int)"/></para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="responsed"></param>
        public void Build(ByteBlock byteBlock, bool responsed = true)
        {
            if (this.m_responsed)
            {
                throw new Exception("该对象已被响应。");
            }

            this.BuildHeader(byteBlock);
            this.BuildContent(byteBlock);
            this.m_responsed = responsed;
        }

        /// <summary>
        /// 构建数据为字节数组。
        /// </summary>
        /// <returns></returns>
        public byte[] BuildAsBytes()
        {
            using (ByteBlock byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }

        /// <summary>
        /// 当传输模式是Chunk时，用于结束传输。
        /// </summary>
        public void Complete()
        {
            this.m_canWrite = false;
            if (this.IsChunk)
            {
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    byteBlock.Write(this.Encoding.GetBytes($"{0.ToString("X")}\r\n"));
                    byteBlock.Write(this.Encoding.GetBytes("\r\n"));
                    this.m_client.DefaultSend(byteBlock);
                    this.m_responsed = true;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="content"></param>
        public override void SetContent(byte[] content)
        {
            this.m_content = content;
            this.ContentLength = content.Length;
            this.m_getContentResult = GetContentResult.Success;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool TryGetContent(out byte[] content)
        {
            switch (this.m_getContentResult)
            {
                case GetContentResult.Default:
                    {
                        if (!this.IsChunk && this.m_contentLength == 0)
                        {
                            this.m_content = new byte[0];
                            content = this.m_content;
                            return true;
                        }

                        try
                        {
                            using (ByteBlock block1 = new ByteBlock(1024 * 1024))
                            {
                                using (ByteBlock block2 = new ByteBlock())
                                {
                                    byte[] buffer = block2.Buffer;
                                    while (true)
                                    {
                                        int r = this.Read(buffer, 0, buffer.Length);
                                        if (r == 0)
                                        {
                                            break;
                                        }
                                        block1.Write(buffer, 0, r);
                                    }
                                    this.m_content = block1.ToArray();
                                    content = this.m_content;
                                    this.m_getContentResult = GetContentResult.Success;
                                    return true;
                                }
                            }
                        }
                        catch
                        {
                            this.m_getContentResult = GetContentResult.Fail;
                            content = null;
                            return false;
                        }
                        finally
                        {
                            this.m_canRead = false;
                        }
                    }
                case GetContentResult.Success:
                    content = this.m_content;
                    return true;

                case GetContentResult.Fail:
                default:
                    content = null;
                    return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void WriteContent(byte[] buffer, int offset, int count)
        {
            if (this.m_responsed)
            {
                throw new Exception("该对象已被响应。");
            }

            if (!this.CanWrite)
            {
                throw new NotSupportedException("该对象不支持持续写入内容。");
            }

            if (!this.m_sentHeader)
            {
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    this.BuildHeader(byteBlock);
                    this.m_client.DefaultSend(byteBlock);
                }
                this.m_sentHeader = true;
            }
            if (this.IsChunk)
            {
                using (ByteBlock byteBlock = new ByteBlock(count + 1024))
                {
                    byteBlock.Write(this.Encoding.GetBytes($"{count.ToString("X")}\r\n"));
                    byteBlock.Write(buffer, offset, count);
                    byteBlock.Write(this.Encoding.GetBytes("\r\n"));
                    this.m_client.DefaultSend(byteBlock);
                    this.m_sentLength += count;
                }
            }
            else
            {
                if (this.m_sentLength + count <= this.m_contentLength)
                {
                    this.m_client.DefaultSend(buffer, offset, count);
                    this.m_sentLength += count;
                    if (this.m_sentLength == this.ContentLength)
                    {
                        this.m_canWrite = false;
                        this.m_responsed = true;
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_client = null;
            base.Dispose(disposing);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        protected override void LoadHeaderProterties()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0)
            {
                string[] ps = first[0].Split('/');
                if (ps.Length == 2)
                {
                    this.Protocols = ps[0];
                    this.ProtocolVersion = ps[1];
                }
            }
            if (first.Length > 1)
            {
                this.StatusCode = first[1];
            }
            string msg = string.Empty;
            for (int i = 2; i < first.Length; i++)
            {
                msg += first[i] + " ";
            }
            this.StatusMessage = msg;

            string transferEncoding = this.GetHeader(HttpHeaders.TransferEncoding);
            if ("chunked".Equals(transferEncoding, StringComparison.OrdinalIgnoreCase))
            {
                this.IsChunk = true;
            }
        }

        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.ContentLength > 0)
            {
                byteBlock.Write(this.m_content);
            }
        }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        private void BuildHeader(ByteBlock byteBlock)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"HTTP/{this.ProtocolVersion} {this.StatusCode} {this.StatusMessage}\r\n");

            if (this.ContentLength > 0)
            {
                this.SetHeader(HttpHeaders.ContentLength, this.ContentLength.ToString());
            }
            if (this.IsChunk)
            {
                this.SetHeader(HttpHeaders.TransferEncoding, "chunked");
            }
            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.Append(this.Headers[headerkey] + "\r\n");
            }

            stringBuilder.Append("\r\n");
            byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }
    }
}