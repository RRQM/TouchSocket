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
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// HTTP请求定义
    /// </summary>
    public class HttpRequest : HttpBase
    {
        private readonly bool m_isServer;
        private bool m_canRead;
        private ITcpClientBase m_client;
        private byte[] m_content;
        private InternalHttpParams m_forms;
        private InternalHttpParams m_params;
        private readonly InternalHttpParams m_query = new InternalHttpParams();
        private bool m_sentHeader;
        private int m_sentLength;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        public HttpRequest(ITcpClientBase client)
        {
            this.m_client = client;
            this.m_isServer = !client.IsClient;
            if (this.m_isServer)
            {
                this.m_canRead = true;
                this.CanWrite = false;
            }
            else
            {
                this.m_canRead = false;
                this.CanWrite = true;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpRequest()
        {
            this.m_canRead = false;
            this.CanWrite = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanRead => this.m_canRead;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanWrite { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override ITcpClientBase Client => this.m_client;

        /// <summary>
        /// 表单数据
        /// </summary>
        public IHttpParams Forms
        {
            get
            {
                if (this.m_isServer)
                {
                    this.m_forms ??= new InternalHttpParams();
                    if (this.ContentType == @"application/x-www-form-urlencoded")
                    {
                        GetParameters(this.GetBody(), this.m_forms);
                    }
                    return this.m_forms;
                }
                else
                {
                    return this.m_forms ??= new InternalHttpParams();
                }
            }
        }

        /// <summary>
        /// HTTP请求方式。
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Body参数
        /// </summary>
        public IHttpParams Params
        {
            get
            {
                this.m_params ??= new InternalHttpParams();
                return this.m_params;
            }
        }

        /// <summary>
        /// url参数
        /// </summary>
        public IHttpParams Query
        {
            get
            {
                return this.m_query;
            }
        }

        /// <summary>
        /// 相对路径（不含参数）
        /// </summary>
        public string RelativeURL { get; private set; } = "/";

        /// <summary>
        /// Url全地址，包含参数
        /// </summary>
        public string URL { get; private set; } = "/";

        /// <summary>
        ///  构建响应数据。
        /// <para>当数据较大时，不建议这样操作，可直接<see cref="WriteContent(byte[], int, int)"/></para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Build(ByteBlock byteBlock)
        {
            this.BuildHeader(byteBlock);
            this.BuildContent(byteBlock);
        }

        /// <summary>
        /// 构建数据为字节数组。
        /// </summary>
        /// <returns></returns>
        public byte[] BuildAsBytes()
        {
            using var byteBlock = new ByteBlock();
            this.Build(byteBlock);
            return byteBlock.ToArray();
        }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="content"></param>
        public override void SetContent(byte[] content)
        {
            this.m_content = content;
            this.ContentLength = content.Length;
            this.ContentComplated = true;
        }

        /// <summary>
        /// 设置代理Host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public HttpRequest SetProxyHost(string host)
        {
            this.URL = host;
            return this;
        }

        /// <summary>
        /// 设置Url，可带参数
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpRequest SetUrl(string url)
        {
            this.URL = url.StartsWith("/") ? url : $"/{url}";
            this.ParseUrl();
            return this;
        }

        /// <summary>
        /// 输出
        /// </summary>
        public override string ToString()
        {
            using (var byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                return byteBlock.ToString();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool TryGetContent(out byte[] content)
        {
            if (!this.ContentComplated.HasValue)
            {
                if (this.ContentLength == 0)
                {
                    this.m_content = new byte[0];
                    content = this.m_content;
                    this.ContentComplated = true;
                    return true;
                }
                try
                {
                    using var block1 = new MemoryStream();
                    using var block2 = new ByteBlock();
                    var buffer = block2.Buffer;
                    while (true)
                    {
                        var r = this.Read(buffer, 0, buffer.Length);
                        if (r == 0)
                        {
                            break;
                        }
                        block1.Write(buffer, 0, r);
                    }
                    this.ContentComplated = true;
                    this.m_content = block1.ToArray();
                    content = this.m_content;
                    return true;
                }
                catch
                {
                    this.ContentComplated = false;
                    content = null;
                    return false;
                }
                finally
                {
                    this.m_canRead = false;
                }
            }
            else if (this.ContentComplated == true)
            {
                content = this.m_content;
                return true;
            }
            else
            {
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
            if (!this.CanWrite)
            {
                throw new NotSupportedException("该对象不支持持续写入内容。");
            }
            if (!this.m_sentHeader)
            {
                using (var byteBlock = new ByteBlock())
                {
                    this.BuildHeader(byteBlock);
                    this.m_client.DefaultSend(byteBlock);
                }
                this.m_sentHeader = true;
            }
            if (this.m_sentLength + count <= this.ContentLength)
            {
                this.m_client.DefaultSend(buffer, offset, count);
                this.m_sentLength += count;
            }
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            this.m_canRead = true;
            this.m_content = null;
            this.m_sentHeader = false;
            this.RelativeURL = "/";
            this.URL = "/";
            this.m_sentLength = 0;
            this.m_params?.Clear();
            this.m_query.Clear();
            this.m_forms?.Clear();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_client = null;
            base.Dispose(disposing);
        }

        /// <summary>
        /// 从内存中读取
        /// </summary>
        protected override void LoadHeaderProterties()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0) this.Method = new HttpMethod(first[0].Trim());
            if (first.Length > 1)
            {
                this.SetUrl(Uri.UnescapeDataString(first[1]));
            }
            if (first.Length > 2)
            {
                var ps = first[2].Split('/');
                if (ps.Length == 2)
                {
                    this.Protocols = new Protocol(ps[0]);
                    this.ProtocolVersion = ps[1];
                }
            }
        }

        private static void GetParameters(string row, in InternalHttpParams pairs)
        {
            if (string.IsNullOrEmpty(row))
            {
                return;
            }

            var kvs = row.Split('&');
            if (kvs == null || kvs.Length == 0)
            {
                return;
            }

            foreach (var item in kvs)
            {
                var kv = item.SplitFirst('=');
                if (kv.Length == 2)
                {
                    pairs.AddOrUpdate(kv[0], kv[1]);
                }
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
            var stringBuilder = new StringBuilder();

            string url = null;
            if (!string.IsNullOrEmpty(this.RelativeURL))
            {
                if (this.m_query.Count == 0)
                {
                    url = this.RelativeURL;
                }
                else
                {
                    var urlBuilder = new StringBuilder();
                    urlBuilder.Append(this.RelativeURL);
                    urlBuilder.Append('?');
                    var i = 0;
                    foreach (var item in this.m_query.Keys)
                    {
                        urlBuilder.Append($"{item}={this.m_query[item]}");
                        if (++i < this.m_query.Count)
                        {
                            urlBuilder.Append('&');
                        }
                    }
                    url = urlBuilder.ToString();
                }
            }

            if (string.IsNullOrEmpty(url))
            {
                stringBuilder.Append($"{this.Method} / HTTP/{this.ProtocolVersion}\r\n");
            }
            else
            {
                stringBuilder.Append($"{this.Method} {url} HTTP/{this.ProtocolVersion}\r\n");
            }

            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.Append(this.Headers[headerkey] + "\r\n");
            }

            stringBuilder.Append("\r\n");
            byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.ContentLength == 0)
            {
                return 0;
            }
            return base.Read(buffer, offset, count);
        }

        private void ParseUrl()
        {
            if (this.URL.Contains('?'))
            {
                var urls = this.URL.Split('?');
                if (urls.Length > 0)
                {
                    this.RelativeURL = urls[0];
                }
                if (urls.Length > 1)
                {
                    this.m_query.Clear();
                    GetParameters(urls[1], this.m_query);
                }
            }
            else
            {
                this.RelativeURL = this.URL;
            }
        }
    }
}