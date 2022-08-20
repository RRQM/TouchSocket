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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Extensions;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// HTTP请求定义
    /// </summary>
    public class HttpRequest : HttpBase
    {
        private bool m_canRead;
        private ITcpClientBase m_client;
        private byte[] m_content;
        private GetContentResult m_getContentResult;
        private Dictionary<string, string> m_params;
        private Dictionary<string, string> m_query;
        private string m_relativeURL;
        private bool m_sentHeader;
        private int m_sentLength;
        private string m_uRL;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        /// <param name="isServer"></param>
        public HttpRequest(ITcpClientBase client, bool isServer = false)
        {
            this.m_client = client;
            if (isServer)
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
        /// 获取时候保持连接
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// HTTP请求方式。
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Body参数
        /// </summary>
        public Dictionary<string, string> Params
        {
            get
            {
                if (this.m_params == null)
                {
                    this.m_params = new Dictionary<string, string>();
                }
                return this.m_params;
            }
        }

        /// <summary>
        /// url参数
        /// </summary>
        public Dictionary<string, string> Query
        {
            get
            {
                if (this.m_query == null)
                {
                    this.m_query = new Dictionary<string, string>();
                }
                return this.m_query;
            }
        }

        /// <summary>
        /// 相对路径（不含参数）
        /// </summary>
        public string RelativeURL => this.m_relativeURL;

        /// <summary>
        /// Url全地址，包含参数
        /// </summary>
        public string URL => this.m_uRL;

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
            using (ByteBlock byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="content"></param>
        public override void SetContent(byte[] content)
        {
            this.m_content = content;
            this.ContentLength = content.Length;
            this.m_getContentResult = GetContentResult.Success;
        }

        /// <summary>
        /// 设置Url，必须以“/”开头，可带参数
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpRequest SetUrl(string url)
        {
            if (url.StartsWith("/"))
            {
                this.m_uRL = url;
            }
            else
            {
                this.m_uRL = "/" + url;
            }
            this.ParseUrl();
            return this;
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
                        if (this.m_contentLength == 0)
                        {
                            this.m_content = new byte[0];
                            content = this.m_content;
                            this.m_getContentResult = GetContentResult.Success;
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
            if (this.m_sentLength + count <= this.m_contentLength)
            {
                this.m_client.DefaultSend(buffer, offset, count);
                this.m_sentLength += count;
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
        /// 从内存中读取
        /// </summary>
        protected override void LoadHeaderProterties()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0) this.Method = first[0].Trim().ToUpper();
            if (first.Length > 1)
            {
                this.SetUrl(Uri.UnescapeDataString(first[1]));
            }
            if (first.Length > 2)
            {
                string[] ps = first[2].Split('/');
                if (ps.Length == 2)
                {
                    this.Protocols = ps[0];
                    this.ProtocolVersion = ps[1];
                }
            }

            if (this.ProtocolVersion == "1.1")
            {
                if (this.GetHeader(HttpHeaders.Connection) == "keep-alive")
                {
                    this.KeepAlive = true;
                }
                else
                {
                    this.KeepAlive = false;
                }
            }
            else
            {
                this.KeepAlive = false;
            }

            if (this.Method == "POST")
            {
                this.ContentType = this.GetHeader(HttpHeaders.ContentType);
                if (this.ContentType == @"application/x-www-form-urlencoded")
                {
                    this.m_params = this.GetParameters(this.GetBody());
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
            StringBuilder stringBuilder = new StringBuilder();

            string url = null;
            if (!string.IsNullOrEmpty(this.m_relativeURL))
            {
                if (this.m_query == null)
                {
                    url = this.m_relativeURL;
                }
                else
                {
                    StringBuilder urlBuilder = new StringBuilder();
                    urlBuilder.Append(this.m_relativeURL);
                    urlBuilder.Append("?");
                    int i = 0;
                    foreach (var item in this.m_query)
                    {
                        urlBuilder.Append($"{item.Key}={item.Value}");
                        if (++i < this.m_query.Count)
                        {
                            urlBuilder.Append("&");
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
            if (this.ContentLength > 0)
            {
                this.SetHeader(HttpHeaders.ContentLength, this.ContentLength.ToString());
            }
            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.Append(this.Headers[headerkey] + "\r\n");
            }

            stringBuilder.Append("\r\n");
            byteBlock.Write(this.Encoding.GetBytes(stringBuilder.ToString()));
        }

        private Dictionary<string, string> GetParameters(string row)
        {
            if (string.IsNullOrEmpty(row))
            {
                return null;
            }
            string[] kvs = row.Split('&');
            if (kvs == null || kvs.Count() == 0)
            {
                return null;
            }

            Dictionary<string, string> pairs = new Dictionary<string, string>();
            foreach (var item in kvs)
            {
                string[] kv = item.SplitFirst('=');
                if (kv.Length == 2)
                {
                    pairs.Add(kv[0], kv[1]);
                }
            }

            return pairs;
        }
        /// <summary>
        /// 输出
        /// </summary>
        public override string ToString()
        {
            using (ByteBlock byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                return byteBlock.ToString();
            }
        }


        private void ParseUrl()
        {
            if (this.m_uRL.Contains("?"))
            {
                string[] urls = this.m_uRL.Split('?');
                if (urls.Length > 0)
                {
                    this.m_relativeURL = urls[0];
                }
                if (urls.Length > 1)
                {
                    this.m_query = this.GetParameters(urls[1]);
                }
            }
            else
            {
                this.m_relativeURL = this.m_uRL;
            }
        }
    }
}