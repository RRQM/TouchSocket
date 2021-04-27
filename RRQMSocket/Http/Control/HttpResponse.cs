using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http响应
    /// </summary>
    public class HttpResponse : BaseHeader
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


        public HttpResponse SetContent(byte[] content, Encoding encoding = null)
        {
            this.Content = content;
            this.Encoding = encoding != null ? encoding : Encoding.UTF8;
            this.Content_Length = content.Length;
            return this;
        }

        public HttpResponse SetContent(string content, Encoding encoding = null)
        {
            //初始化内容
            encoding = encoding != null ? encoding : Encoding.UTF8;
            return SetContent(encoding.GetBytes(content), encoding);
        }

        public string GetHeader(ResponseHeader header)
        {
            return GetHeaderByKey(header);
        }

        public string GetHeader(string fieldName)
        {
            return GetHeaderByKey(fieldName);
        }

        public void SetHeader(ResponseHeader header, string value)
        {
            SetHeaderByKey(header, value);
        }

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
                stringBuilder.AppendLine($"HTTP/1.1 {StatusCode}");
            if (!string.IsNullOrEmpty(this.Content_Type))
                stringBuilder.AppendLine("Content-Type: " + this.Content_Type);
            if (this.Content_Length > 0)
                stringBuilder.AppendLine("Content-Length: " + this.Content_Length);
            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.AppendLine(this.Headers[headerkey]);
            }
            stringBuilder.AppendLine("Server: RRQMSocket");
            stringBuilder.AppendLine("Date: " + DateTime.Now.ToGMTString("r"));
            stringBuilder.AppendLine();
            byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }
        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.Content_Length>0)
            {
                byteBlock.Write(this.Content);
            }
        }
        public void Build(ByteBlock byteBlock)
        {
            BuildHeader(byteBlock);
            BuildContent(byteBlock);
        }
    }
}
