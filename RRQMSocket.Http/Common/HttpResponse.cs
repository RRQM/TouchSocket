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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http响应
    /// </summary>
    public class HttpResponse : HttpBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public HttpResponse SetContent(byte[] content, Encoding encoding = null)
        {
            this.Content = content;
            this.Encoding = encoding != null ? encoding : Encoding.UTF8;
            this.Content_Length = content.Length;
            return this;
        }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public HttpResponse SetContent(string content, Encoding encoding = null)
        {
            //初始化内容
            encoding = encoding != null ? encoding : Encoding.UTF8;
            return SetContent(encoding.GetBytes(content), encoding);
        }

        /// <summary>
        /// 获取头数据
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetHeader(string fieldName)
        {
            return GetHeaderByKey(fieldName);
        }

        /// <summary>
        /// 设置头数据
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void SetHeader(HttpHeaders header, string value)
        {
            SetHeaderByKey(header, value);
        }

        /// <summary>
        /// 设置头数据
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void SetHeader(string fieldName, string value)
        {
            SetHeaderByKey(fieldName, value);
        }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        private void BuildHeader(ByteBlock byteBlock)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(StatusCode))
                stringBuilder.AppendLine($"HTTP/{this.ProtocolVersion} {StatusCode}");
            if (!string.IsNullOrEmpty(this.Content_Type))
                stringBuilder.AppendLine("Content-Type: " + this.Content_Type);
            stringBuilder.AppendLine("Content-Length: " + this.Content_Length);
            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.AppendLine(this.Headers[headerkey]);
            }

            stringBuilder.AppendLine();
            byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }

        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.Content_Length > 0)
            {
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
        /// 读取数据
        /// </summary>
        public override void ReadFromBase()
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
            string contentLength = this.GetHeader(HttpHeaders.ContentLength);
            int.TryParse(contentLength, out int content_Length);
            this.Content_Length = content_Length;
        }
    }
}