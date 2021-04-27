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
        /// <param name="header"></param>
        /// <returns></returns>
        public string GetHeader(ResponseHeader header)
        {
            return GetHeaderByKey(header);
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
        public void SetHeader(ResponseHeader header, string value)
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
            stringBuilder.AppendLine($"Server: RRQMSocket.Http {ServerVersion}");
            stringBuilder.AppendLine("Date: " + DateTime.Now.ToGMTString("r"));
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
        public void Build(ByteBlock byteBlock)
        {
            BuildHeader(byteBlock);
            BuildContent(byteBlock);
        }
    }
}
