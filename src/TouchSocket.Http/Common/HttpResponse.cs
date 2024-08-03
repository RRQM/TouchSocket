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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http响应
    /// </summary>
    public class HttpResponse : HttpBase
    {
        #region 字段

        private readonly HttpClientBase m_httpClientBase;
        private readonly HttpSessionClient m_httpSessionClient;
        private readonly bool m_isServer;
        private bool m_canRead;
        private bool m_canWrite;
        private ReadOnlyMemory<byte> m_content;
        private bool m_sentHeader;
        private long m_sentLength;

        #endregion 字段

        /// <summary>
        /// Http响应
        /// </summary>
        /// <param name="httpClientBase"></param>
        internal HttpResponse(HttpClientBase httpClientBase)
        {
            this.m_isServer = false;
            this.m_canRead = true;
            this.m_canWrite = false;
            this.m_httpClientBase = httpClientBase;
        }

        /// <summary>
        /// 从<see cref="HttpRequest"/>创建一个Http响应
        /// </summary>
        /// <param name="request"></param>
        internal HttpResponse(HttpRequest request)
        {
            this.m_canRead = false;
            this.m_canWrite = true;
            this.m_isServer = true;
            this.m_httpSessionClient = (HttpSessionClient)request.Client;
            this.ProtocolVersion = request.ProtocolVersion;
            this.Protocols = request.Protocols;
            this.KeepAlive = request.KeepAlive;
        }

        #region 属性

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
        public override IClient Client => this.m_isServer ? this.m_httpSessionClient : this.m_httpClientBase;

        /// <summary>
        /// 是否分块
        /// </summary>
        public bool IsChunk { get; set; }

        /// <summary>
        /// 是否代理权限验证。
        /// </summary>
        public bool IsProxyAuthenticationRequired => this.StatusCode == 407;

        /// <summary>
        /// 是否重定向
        /// </summary>
        public bool IsRedirect => this.StatusCode == 301 || this.StatusCode == 302;

        /// <summary>
        /// 是否已经响应数据。
        /// </summary>
        public bool Responsed { get; private set; }

        /// <summary>
        /// 状态码，默认200
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// 状态消息，默认Success
        /// </summary>
        public string StatusMessage { get; set; } = "Success";

        #endregion 属性

        /// <summary>
        /// 构建数据并回应。
        /// <para>该方法仅在具有Client实例时有效。</para>
        /// </summary>
        public async Task AnswerAsync()
        {
            this.ThrowIfResponsed();
            using (var byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                await this.InternalSendAsync(byteBlock.Memory).ConfigureAwait(false);
                this.Responsed = true;
            }
        }

        /// <summary>
        ///  构建响应数据。
        /// <para>当数据较大时，不建议这样操作，可直接<see cref="Write(byte[], int, int)"/></para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="responsed"></param>
        public void Build(ByteBlock byteBlock, bool responsed = true)
        {
            this.ThrowIfResponsed();
            this.BuildHeader(byteBlock);
            this.BuildContent(byteBlock);
            this.Responsed = responsed;
        }

        /// <summary>
        /// 构建数据为字节数组。
        /// </summary>
        /// <returns></returns>
        public byte[] BuildAsBytes()
        {
            using (var byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }

        /// <summary>
        /// 当传输模式是Chunk时，用于结束传输。
        /// </summary>
        public async Task CompleteChunkAsync()
        {
            if (!this.m_canWrite)
            {
                return;
            }

            this.ThrowIfResponsed();
            this.m_canWrite = false;
            if (this.IsChunk)
            {
                using (var byteBlock = new ByteBlock())
                {
                    byteBlock.Write(Encoding.UTF8.GetBytes($"{0:X}\r\n"));
                    byteBlock.Write(Encoding.UTF8.GetBytes("\r\n"));
                    await this.InternalSendAsync(byteBlock.Memory).ConfigureAwait(false);
                    this.Responsed = true;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override async ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
        {
            if (!this.ContentComplated.HasValue)
            {
                if (!this.IsChunk && this.ContentLength == 0)
                {
                    this.m_content = new byte[0];
                    return this.m_content;
                }

                if (!this.IsChunk && this.ContentLength > MaxCacheSize)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(this.ContentLength), this.ContentLength, MaxCacheSize);
                }

                try
                {
                    using (var memoryStream = new MemoryStream(1024 * 1024))
                    {
                        while (true)
                        {
                            using (var blockResult = await this.ReadAsync(cancellationToken))
                            {
                                var segment = blockResult.Memory.GetArray();
                                if (blockResult.IsCompleted)
                                {
                                    break;
                                }
                                memoryStream.Write(segment.Array, segment.Offset, segment.Count);
                            }

                            if (memoryStream.Length > MaxCacheSize)
                            {
                                ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(this.ContentLength), this.ContentLength, MaxCacheSize);
                            }
                        }
                        this.ContentComplated = true;
                        this.m_content = memoryStream.ToArray();
                        return this.m_content;
                    }
                }
                catch
                {
                    this.ContentComplated = false;
                    this.m_content = null;
                    return this.m_content;
                }
                finally
                {
                    this.m_canRead = false;
                }
            }
            else
            {
                return this.ContentComplated == true ? this.m_content : default;
            }
        }

        public override async ValueTask<IBlockResult<byte>> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (this.ContentLength == 0 && !this.IsChunk)
            {
                return InternalBlockResult.Completed;
            }

            if (this.ContentComplated.HasValue && this.ContentComplated.Value)
            {
                return new InternalBlockResult(this.m_content, true);
            }
            var blockResult = await base.ReadAsync(cancellationToken);
            if (blockResult == InternalBlockResult.Completed)
            {
                this.ContentComplated = true;
            }
            return blockResult;
        }

        /// <inheritdoc/>
        public override void SetContent(in ReadOnlyMemory<byte> content)
        {
            this.m_content = content;
            this.ContentLength = content.Length;
            this.ContentComplated = true;
        }

        /// <summary>
        /// 输出
        /// </summary>
        public override string ToString()
        {
            using (var byteBlock = new ByteBlock())
            {
                this.Build(byteBlock, false);
                return byteBlock.ToString();
            }
        }

        #region Write

        /// <inheritdoc/>
        public override async Task WriteAsync(ReadOnlyMemory<byte> memory)
        {
            if (this.Responsed)
            {
                throw new Exception("该对象已被响应。");
            }

            if (!this.CanWrite)
            {
                throw new NotSupportedException("该对象不支持持续写入内容。");
            }

            if (!this.m_sentHeader)
            {
                using (var byteBlock = new ByteBlock())
                {
                    this.BuildHeader(byteBlock);
                    await this.InternalSendAsync(byteBlock.Memory).ConfigureAwait(false);
                }
                this.m_sentHeader = true;
            }

            var count = memory.Length;

            if (this.IsChunk)
            {
                using (var byteBlock = new ByteBlock(count + 1024))
                {
                    byteBlock.Write(Encoding.UTF8.GetBytes($"{count:X}\r\n"));
                    byteBlock.Write(memory.Span);
                    byteBlock.Write(Encoding.UTF8.GetBytes("\r\n"));
                    await this.InternalSendAsync(byteBlock.Memory).ConfigureAwait(false);
                    this.m_sentLength += count;
                }
            }
            else
            {
                if (this.m_sentLength + count <= this.ContentLength)
                {
                    await this.InternalSendAsync(memory);
                    this.m_sentLength += count;
                    if (this.m_sentLength == this.ContentLength)
                    {
                        this.m_canWrite = false;
                        this.Responsed = true;
                    }
                }
            }
        }

        #endregion Write

        internal override void ResetHttp()
        {
            base.ResetHttp();
            this.m_sentHeader = false;
            this.m_sentLength = 0;
            this.Responsed = false;
            this.IsChunk = false;
            this.StatusCode = 200;
            this.StatusMessage = "Success";
            if (this.m_isServer)
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

        /// <inheritdoc/>
        protected override void LoadHeaderProterties()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0)
            {
                var ps = first[0].Split('/');
                if (ps.Length == 2)
                {
                    this.Protocols = new Protocol(ps[0]);
                    this.ProtocolVersion = ps[1];
                }
            }
            if (first.Length > 1)
            {
                _ = int.TryParse(first[1], out var code);
                this.StatusCode = code;
            }
            var msg = string.Empty;
            for (var i = 2; i < first.Length; i++)
            {
                msg += first[i] + " ";
            }
            this.StatusMessage = msg;

            var transferEncoding = this.Headers.Get(HttpHeaders.TransferEncoding);
            if ("chunked".Equals(transferEncoding, StringComparison.OrdinalIgnoreCase))
            {
                this.IsChunk = true;
            }
        }

        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.ContentLength > 0)
            {
                byteBlock.Write(this.m_content.Span);
            }
        }


        private void BuildHeader(ByteBlock byteBlock)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"HTTP/{this.ProtocolVersion} {this.StatusCode} {this.StatusMessage}\r\n");

            if (this.IsChunk)
            {
                this.Headers.Add(HttpHeaders.TransferEncoding, "chunked");
            }
            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.Append(this.Headers[headerkey] + "\r\n");
            }

            stringBuilder.Append("\r\n");
            byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }

        private Task InternalSendAsync(in ReadOnlyMemory<byte> memory)
        {
            return this.m_isServer ? this.m_httpSessionClient.InternalSendAsync(memory) : this.m_httpClientBase.InternalSendAsync(memory);
        }

        private void ThrowIfResponsed()
        {
            if (this.Responsed)
            {
                throw new Exception("该对象已被响应。");
            }
        }
    }
}