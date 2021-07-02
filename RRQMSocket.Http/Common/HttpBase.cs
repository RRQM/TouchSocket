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
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http基础头部
    /// </summary>
    public abstract class HttpBase
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public static readonly string ServerVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private string requestLine;

        /// <summary>
        /// 请求体字符数据
        /// </summary>
        public string BodyString { get; set; }

        /// <summary>
        /// 内容编码
        /// </summary>
        public string Content_Encoding { get; set; }

        /// <summary>
        /// 内容长度
        /// </summary>
        public int Content_Length { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string Content_Type { get; set; }

        /// <summary>
        /// 内容语言
        /// </summary>
        public string ContentLanguage { get; set; }

        private Encoding encoding=Encoding.UTF8;
        /// <summary>
        /// 编码方式
        /// </summary>
        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        /// <summary>
        /// 传递标识
        /// </summary>
        public object Flag { get; set; }

        /// <summary>
        /// 请求头集合
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 协议名称
        /// </summary>
        public string Protocols { get; set; }

        /// <summary>
        /// HTTP协议版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 请求行
        /// </summary>
        public string RequestLine
        {
            get { return requestLine; }
        }

        /// <summary>
        /// 构建数据
        /// </summary>
        /// <param name="byteBlock"></param>
        public abstract void Build(ByteBlock byteBlock);

        /// <summary>
        /// 获取头值
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public string GetHeader(HttpHeaders header)
        {
            return GetHeaderByKey(header);
        }

        /// <summary>
        /// 读取信息
        /// </summary>
        public abstract void ReadFromBase();
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
            this.requestLine = rows[0];

            //Request Headers
            this.Headers = GetRequestHeaders(rows);

            string contentLength = this.GetHeader(HttpHeaders.ContentLength);
            int.TryParse(contentLength, out int content_Length);
            this.Content_Length = content_Length;
        }

        /// <summary>
        /// 获取头集合的值
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        protected string GetHeaderByKey(Enum header)
        {
            var fieldName = header.GetDescription();
            if (fieldName == null) return null;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return Headers[fieldName];
        }

        /// <summary>
        /// 获取头集合的值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected string GetHeaderByKey(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return null;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return Headers[fieldName];
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        protected void SetHeaderByKey(Enum header, string value)
        {
            var fieldName = header.GetDescription();
            if (fieldName == null) return;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) Headers.Add(fieldName, value);
            Headers[fieldName] = value;
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        protected void SetHeaderByKey(string fieldName, string value)
        {
            if (string.IsNullOrEmpty(fieldName)) return;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) Headers.Add(fieldName, value);
            Headers[fieldName] = value;
        }

        private Dictionary<string, string> GetRequestHeaders(IEnumerable<string> rows)
        {
            if (rows == null || rows.Count() <= 0)
            {
                return null;
            }
            Dictionary<string, string> header = new Dictionary<string, string>();
            foreach (var item in rows)
            {
                string[] kv = item.SplitFirst(':');
                if (kv.Length == 2)
                {
                    header.Add(kv[0], kv[1]);
                }
            }

            return header;
        }
    }
}