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
using TouchSocket.Core.IO;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http基础头部
    /// </summary>
    public abstract class HttpBase : BlockReader, IRequestInfo
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public static readonly string ServerVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// 内容长度
        /// </summary>
        protected long m_contentLength;

        private static readonly byte[] m_rnrnCode = Encoding.UTF8.GetBytes("\r\n\r\n");

        private readonly Dictionary<string, string> m_headers;
        private Encoding encoding = Encoding.UTF8;
        private string m_requestLine;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpBase()
        {
            this.m_headers = new Dictionary<string, string>();
            this.ReadTimeout = 1000 * 60;
        }

        /// <summary>
        /// 客户端
        /// </summary>
        public abstract ITcpClientBase Client { get; }

        /// <summary>
        /// int类型，内容长度
        /// </summary>
        public int ContentLen
        {
            get => (int)this.m_contentLength;
            set => this.m_contentLength = value;
        }

        /// <summary>
        /// 能否写入。
        /// </summary>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// 内容长度
        /// </summary>
        public long ContentLength
        {
            get => this.m_contentLength;
            set => this.m_contentLength = value;
        }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 编码方式
        /// </summary>
        public Encoding Encoding
        {
            get => this.encoding;
            set => this.encoding = value;
        }

        /// <summary>
        /// 传递标识
        /// </summary>
        public object Flag { get; set; }

        /// <summary>
        /// 请求头集合
        /// </summary>
        public Dictionary<string, string> Headers => this.m_headers;

        /// <summary>
        /// 协议名称，默认HTTP
        /// </summary>
        public string Protocols { get; set; } = "HTTP";

        /// <summary>
        /// HTTP协议版本，默认1.1
        /// </summary>
        public string ProtocolVersion { get; set; } = "1.1";

        /// <summary>
        /// 请求行
        /// </summary>
        public string RequestLine => this.m_requestLine;

        /// <summary>
        /// 获取头值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetHeader(string fieldName)
        {
            return this.GetHeaderByKey(fieldName);
        }

        /// <summary>
        /// 获取头集合的值
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public string GetHeader(HttpHeaders header)
        {
            var fieldName = header.GetDescription();
            if (fieldName == null) return null;
            var hasKey = this.Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return this.Headers[fieldName];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool ParsingHeader(ByteBlock byteBlock, int length)
        {
            int index = byteBlock.Buffer.IndexOfFirst(byteBlock.Pos, length, m_rnrnCode);
            if (index > 0)
            {
                int headerLength = index - byteBlock.Pos;
                this.ReadHeaders(byteBlock.Buffer, byteBlock.Pos, headerLength);
                byteBlock.Pos += headerLength;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException("该对象不支持持续读取内容。");
            }
            return base.Read(buffer, offset, count);
        }

        /// <summary>
        /// 从内存中读取
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void ReadHeaders(byte[] buffer, int offset, int length)
        {
            string data = Encoding.UTF8.GetString(buffer, offset, length);
            string[] rows = Regex.Split(data, Environment.NewLine);

            //Request URL & Method & Version
            this.m_requestLine = rows[0];

            //Request Headers
            this.GetRequestHeaders(rows);
            long.TryParse(this.GetHeader(HttpHeaders.ContentLength), out this.m_contentLength);
            this.LoadHeaderProterties();
        }

        /// <summary>
        /// 设置一次性内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public abstract void SetContent(byte[] content);

        /// <summary>
        /// 设置请求头
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void SetHeaderByKey(string fieldName, string value)
        {
            if (string.IsNullOrEmpty(fieldName)) return;
            var hasKey = this.Headers.ContainsKey(fieldName);
            if (!hasKey) this.Headers.Add(fieldName, value);
            this.Headers[fieldName] = value;
        }

        /// <summary>
        /// 获取一次性内容。
        /// </summary>
        /// <returns></returns>
        public abstract bool TryGetContent(out byte[] content);

        /// <summary>
        /// 持续写入内容。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public abstract void WriteContent(byte[] buffer, int offset, int count);

        internal bool InternalInput(byte[] buffer, int offset, int length)
        {
            return this.Input(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!this.m_disposedValue && this.CanRead)
            {
                this.TryGetContent(out _);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 读取信息
        /// </summary>
        protected abstract void LoadHeaderProterties();

        private string GetHeaderByKey(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return null;
            var hasKey = this.Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return this.Headers[fieldName];
        }

        private void GetRequestHeaders(IEnumerable<string> rows)
        {
            this.m_headers.Clear();
            if (rows == null || rows.Count() <= 0)
            {
                return;
            }

            foreach (var item in rows)
            {
                string[] kv = item.SplitFirst(':');
                if (kv.Length == 2)
                {
                    if (!this.m_headers.ContainsKey(kv[0]))
                    {
                        this.m_headers.Add(kv[0].ToLower(), kv[1]);
                    }
                }
            }
        }
    }
}