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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http基础头部
    /// </summary>
    public abstract class HttpBase : IRequestInfo
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public static readonly string ServerVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static readonly byte[] s_rnrnCode = Encoding.UTF8.GetBytes("\r\n\r\n");

        private readonly InternalHttpHeader m_headers = new InternalHttpHeader();
        private readonly HttpBlockSegment m_httpBlockSegment = new HttpBlockSegment();
        public const int MaxCacheSize = 1024 * 1024 * 100;

        /// <summary>
        /// 能否写入。
        /// </summary>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// 能否读取。
        /// </summary>
        public abstract bool CanRead { get; }

        /// <summary>
        /// 客户端
        /// </summary>
        public abstract IClient Client { get; }

        /// <summary>
        /// 内容填充完成
        /// </summary>
        public bool? ContentComplated { get; protected set; } = null;

        /// <summary>
        /// 内容长度
        /// </summary>
        public long ContentLength
        {
            get
            {
                var contentLength = this.m_headers.Get(HttpHeaders.ContentLength);
                return contentLength.IsNullOrEmpty() ? 0 : long.TryParse(contentLength, out var value) ? value : 0;
            }
            set => this.m_headers.Add(HttpHeaders.ContentLength, value.ToString());
        }

        /// <summary>
        /// 保持连接。
        /// <para>
        /// 一般的，当是http1.1时，如果没有显式的Connection: close，即返回true。当是http1.0时，如果没有显式的Connection: Keep-Alive，即返回false。
        /// </para>
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                var keepalive = this.Headers.Get(HttpHeaders.Connection);
                return this.ProtocolVersion == "1.0"
                    ? !keepalive.IsNullOrEmpty() && keepalive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase)
                    : keepalive.IsNullOrEmpty() || keepalive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase);
            }
            set
            {
                if (this.ProtocolVersion == "1.0")
                {
                    if (value)
                    {
                        this.m_headers.Add(HttpHeaders.Connection, "Keep-Alive");
                    }
                    else
                    {
                        this.m_headers.Add(HttpHeaders.Connection, "close");
                    }
                }
                else
                {
                    if (!value)
                    {
                        this.m_headers.Add(HttpHeaders.Connection, "close");
                    }
                }
            }
        }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType
        {
            get => this.m_headers.Get(HttpHeaders.ContentType);
            set => this.m_headers.Add(HttpHeaders.ContentType, value);
        }

        /// <summary>
        /// 允许编码
        /// </summary>
        public string AcceptEncoding
        {
            get => this.m_headers.Get(HttpHeaders.AcceptEncoding);
            set => this.m_headers.Add(HttpHeaders.AcceptEncoding, value);
        }

        /// <summary>
        /// 可接受MIME类型
        /// </summary>
        public string Accept
        {
            get => this.m_headers.Get(HttpHeaders.Accept);
            set => this.m_headers.Add(HttpHeaders.Accept, value);
        }

        /// <summary>
        /// 请求头集合
        /// </summary>
        public IHttpHeader Headers => this.m_headers;

        /// <summary>
        /// 协议名称，默认HTTP
        /// </summary>
        public Protocol Protocols { get; protected set; } = Protocol.Http;

        /// <summary>
        /// HTTP协议版本，默认1.1
        /// </summary>
        public string ProtocolVersion { get; set; } = "1.1";

        /// <summary>
        /// 请求行
        /// </summary>
        public string RequestLine { get; private set; }

        internal bool ParsingHeader<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            var index = byteBlock.Span.Slice(byteBlock.Position).IndexOf(s_rnrnCode);
            if (index > 0)
            {
                var headerLength = index - byteBlock.Position;
                this.ReadHeaders(byteBlock.Span.Slice(byteBlock.Position, headerLength));
                byteBlock.Position += headerLength;
                byteBlock.Position += 4;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ReadHeaders(ReadOnlySpan<byte> span)
        {
            var data = span.ToString(Encoding.UTF8);
            var rows = Regex.Split(data, "\r\n");

            //Request URL & Method & Version
            this.RequestLine = rows[0];

            //Request Headers
            this.GetRequestHeaders(rows);
            this.LoadHeaderProterties();
        }

        #region Content

        /// <summary>
        /// 设置一次性内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public abstract void SetContent(in ReadOnlyMemory<byte> content);

        /// <summary>
        /// 获取一次性内容。
        /// </summary>
        /// <returns></returns>
        public abstract ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取一次性内容。
        /// </summary>
        /// <returns></returns>
        public virtual ReadOnlyMemory<byte> GetContent(CancellationToken cancellationToken = default)
        {
            return Task.Run(async () => await this.GetContentAsync(cancellationToken)).GetFalseAwaitResult();
        }

        #endregion Content

        internal Task InternalInputAsync(ReadOnlyMemory<byte> memory)
        {
            return this.m_httpBlockSegment.InternalInputAsync(memory);
        }

        internal Task CompleteInput()
        {
            return this.m_httpBlockSegment.InternalComplete();
        }

        /// <summary>
        /// 读取信息
        /// </summary>
        protected abstract void LoadHeaderProterties();

        private void GetRequestHeaders(string[] rows)
        {
            this.m_headers.Clear();
            if (rows == null || rows.Length <= 0)
            {
                return;
            }

            foreach (var item in rows)
            {
                var kv = item.SplitFirst(':');
                if (kv.Length == 2)
                {
                    var key = kv[0].ToLower();
                    this.m_headers.Add(key, kv[1]);
                }
            }
        }

        internal virtual void ResetHttp()
        {
            this.m_headers.Clear();
            this.ContentComplated = null;
            this.RequestLine = default;

            this.m_httpBlockSegment.InternalReset();
        }

        #region Read

        public virtual ValueTask<IBlockResult<byte>> ReadAsync(CancellationToken cancellationToken)
        {
            return this.m_httpBlockSegment.InternalValueWaitAsync(cancellationToken);
        }

        public async Task ReadCopyToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                using (var blockResult = await this.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!blockResult.Memory.Equals(ReadOnlyMemory<byte>.Empty))
                    {
                        var memory = blockResult.Memory;
#if NET6_0_OR_GREATER
                        await stream.WriteAsync(memory, cancellationToken);
#else
                        var segment = memory.GetArray();
                        await stream.WriteAsync(segment.Array, segment.Offset, segment.Count);
#endif
                    }
                    if (blockResult.IsCompleted)
                    {
                        break;
                    }
                }
            }
        }

        #endregion Read

        #region Write

        public abstract Task WriteAsync(ReadOnlyMemory<byte> memory);

        #endregion Write

        #region Class

        private class HttpBlockSegment : BlockSegment<byte>
        {
            internal Task InternalComplete()
            {
                return base.Complete(string.Empty);
            }

            internal Task InternalInputAsync(ReadOnlyMemory<byte> memory)
            {
                return base.InputAsync(memory);
            }

            internal void InternalReset()
            {
                base.Reset();
            }

            internal ValueTask<IBlockResult<byte>> InternalValueWaitAsync(CancellationToken cancellationToken)
            {
                return base.ValueWaitAsync(cancellationToken);
            }
        }

        #endregion Class
    }
}