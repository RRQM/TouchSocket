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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RRQMCore.Helper;

namespace RRQMSocket.Http
{
    /// <summary>
    /// HTTP请求定义
    /// </summary>
    public class HttpRequest : BaseHeader
    {
        /// <summary>
        /// url参数
        /// </summary>
        public Dictionary<string, string> Query { get; set; }

        /// <summary>
        /// 表单参数
        /// </summary>
        public Dictionary<string, string> Forms { get; set; }

        /// <summary>
        /// request 参数
        /// </summary>
        public Dictionary<string, string> Parmas { get; set; }

        /// <summary>
        /// URL参数
        /// </summary>
        public Dictionary<string, string> Params { get; private set; }

        /// <summary>
        /// HTTP请求方式
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// HTTP(S)地址
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 获取时候保持连接
        /// </summary>
        public bool KeepAlive { get; private set; }

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
            var first = Regex.Split(rows[0], @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0) this.Method = first[0];
            if (first.Length > 1) this.URL = Uri.UnescapeDataString(first[1]);
            if (first.Length > 2)
            {
                string[] ps = first[2].Split('/');
                if (ps.Length == 2)
                {
                    this.Protocols = ps[0];
                    this.ProtocolVersion = ps[1];
                }
            }

            //Request Headers
            this.Headers = GetRequestHeaders(rows);

            string contentLength = this.GetHeader(RequestHeaders.ContentLength);
            int.TryParse(contentLength, out int content_Length);
            this.Content_Length = content_Length;

            if (this.Method == "GET")
            {
                var isUrlencoded = this.URL.Contains('?');
                if (isUrlencoded) this.Query = GetRequestParameters(URL.Split('?')[1]);
            }
            if (this.ProtocolVersion == "1.1")
            {
                if (this.GetHeader(RequestHeaders.Connection) == "keep-alive")
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
        }

        /// <summary>
        /// 获取头值
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public string GetHeader(RequestHeaders header)
        {
            return GetHeaderByKey(header);
        }

        /// <summary>
        /// 获取头值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetHeader(string fieldName)
        {
            return GetHeaderByKey(fieldName);
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void SetHeader(RequestHeaders header, string value)
        {
            SetHeaderByKey(header, value);
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void SetHeader(string fieldName, string value)
        {
            SetHeaderByKey(fieldName, value);
        }

        /// <summary>
        /// 构建完整请求
        /// </summary>
        public void Build()
        {
            this.BodyString = this.Body == null ? null : Encoding.UTF8.GetString(this.Body.Buffer, 0, (int)this.Body.Length);
            if (this.Method == "POST")
            {
                var contentType = GetHeader(RequestHeaders.ContentType);
                var isUrlencoded = contentType == @"application/x-www-form-urlencoded";

                if (isUrlencoded) this.Params = GetRequestParameters(this.BodyString);
            }
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

        private Dictionary<string, string> GetRequestParameters(string row)
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
        /// 传递标识
        /// </summary>
        public object Flag { get; set; }
    }
}