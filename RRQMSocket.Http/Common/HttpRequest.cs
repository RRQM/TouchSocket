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
using RRQMCore.Exceptions;
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RRQMSocket.Http
{
    /// <summary>
    /// HTTP请求定义
    /// </summary>
    public class HttpRequest : HttpBase
    {
        /// <summary>
        /// 表单参数
        /// </summary>
        public Dictionary<string, string> Forms { get; set; }

        /// <summary>
        /// 获取时候保持连接
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// HTTP请求方式
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// URL参数
        /// </summary>
        public Dictionary<string, string> Params { get; set; }

        /// <summary>
        /// url参数
        /// </summary>
        public Dictionary<string, string> Query { get; set; }

        /// <summary>
        /// 相对路径（不含参数）
        /// </summary>
        public string RelativeURL { get; set; }

        /// <summary>
        /// HTTP(S)地址
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        private void BuildHeader(ByteBlock byteBlock)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{this.Method} / HTTP/{this.ProtocolVersion}");

            this.SetHeader(HttpHeaders.UserAgent, "RRQMHTTP");

            if (!string.IsNullOrEmpty(this.Content_Type))
                stringBuilder.AppendLine("Content-Type: " + this.Content_Type);
            stringBuilder.AppendLine("Content-Length: " + this.BodyLength);

            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.AppendLine(this.Headers[headerkey]);
            }

            stringBuilder.AppendLine();
            byteBlock.Write(this.Encoding.GetBytes(stringBuilder.ToString()));
        }

        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.BodyLength > 0)
            {
                if (this.BodyLength != this.Content.Length)
                {
                    throw new RRQMException("内容实际长度与设置长度不相等");
                }
                byteBlock.Write(this.Content);
            }
        }

        /// <summary>
        /// 构建响应数据
        /// </summary>
        /// <param name="byteBlock"></param>
        public override void Build(ByteBlock byteBlock)
        {
            BuildHeader(byteBlock);
            BuildContent(byteBlock);
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
        /// 从内存中读取
        /// </summary>
        public override void ReadFromBase()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0) this.Method = first[0];
            if (first.Length > 1)
            {
                this.URL = Uri.UnescapeDataString(first[1]);
                this.RelativeURL = first[1].Split('?')[0];
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

            string contentLength = this.GetHeader(HttpHeaders.ContentLength);
            int.TryParse(contentLength, out int content_Length);
            this.BodyLength = content_Length;

            if (this.Method == "GET")
            {
                var isUrlencoded = this.URL.Contains('?');
                if (isUrlencoded) this.Query = GetRequestParameters(URL.Split('?')[1]);
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
                this.Content_Type = GetHeader(HttpHeaders.ContentType);
                if (this.Content_Type == @"application/x-www-form-urlencoded")
                {
                    this.Params = GetRequestParameters(this.Body);
                }
            }
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void SetHeader(HttpHeaders header, string value)
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
    }
}