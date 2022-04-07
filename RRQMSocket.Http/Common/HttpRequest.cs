//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Extensions;
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
        /// 构建响应数据
        /// </summary>
        /// <param name="byteBlock"></param>
        public override void Build(ByteBlock byteBlock)
        {
            this.BuildHeader(byteBlock);
            this.BuildContent(byteBlock);
        }

        /// <summary>
        /// 获取内容
        /// </summary>
        /// <returns></returns>
        public override byte[] GetContent()
        {
            return this.content;
        }

        /// <summary>
        /// 获取头值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetHeader(string fieldName)
        {
            return this.GetHeaderByKey(fieldName);
        }

        byte[] content;

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="content"></param>
        public override void SetContent(byte[] content)
        {
            this.content = content;
            this.ContentLength = content.Length;
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void SetHeader(HttpHeaders header, string value)
        {
            this.SetHeaderByKey(header, value);
        }

        /// <summary>
        /// 设置头值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void SetHeader(string fieldName, string value)
        {
            this.SetHeaderByKey(fieldName.ToLower(), value);
        }

        /// <summary>
        /// 输出
        /// </summary>
        public override string ToString()
        {
            using (ByteBlock byteBlock=new ByteBlock())
            {
                this.Build(byteBlock);
                return byteBlock.ToString();
            }
        }

        /// <summary>
        /// 从内存中读取
        /// </summary>
        protected override void LoadHeaderProterties()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0) this.Method = first[0];
            if (first.Length > 1)
            {
                this.URL = Uri.UnescapeDataString(first[1]);
                this.RelativeURL = this.URL.Split('?')[0];
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

            if (this.Method == "GET")
            {
                var isUrlencoded = this.URL.Contains('?');
                if (isUrlencoded) this.Query = this.GetRequestParameters(this.URL.Split('?')[1]);
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
                    this.Params = this.GetRequestParameters(this.GetBody());
                }
            }
        }
        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.ContentLength > 0)
            {
                if (this.ContentLength != this.GetContent().Length)
                {
                    throw new RRQMException("内容实际长度与设置长度不相等");
                }
                byteBlock.Write(this.GetContent());
            }
        }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        private void BuildHeader(ByteBlock byteBlock)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(this.URL))
            {
                stringBuilder.AppendLine($"{this.Method} / HTTP/{this.ProtocolVersion}");
            }
            else
            {
                stringBuilder.AppendLine($"{this.Method} {this.URL} HTTP/{this.ProtocolVersion}");
            }
            if (this.ContentLength > 0)
            {
                this.SetHeaderByKey(HttpHeaders.ContentLength, this.ContentLength.ToString());
            }
            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.AppendLine(this.Headers[headerkey]);
            }

            stringBuilder.AppendLine();
            byteBlock.Write(this.Encoding.GetBytes(stringBuilder.ToString()));
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