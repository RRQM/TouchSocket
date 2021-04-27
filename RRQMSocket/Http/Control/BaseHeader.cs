using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http基础头部
    /// </summary>
    public class BaseHeader : IDisposable
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public static readonly string ServerVersion= System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// 请求体流数据
        /// </summary>
        public ByteBlock Body { get; set; }

        /// <summary>
        /// 请求体字符数据
        /// </summary>
        public string BodyString { get; set; }

        /// <summary>
        /// 编码方式
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string Content_Type { get; set; }

        /// <summary>
        /// 内容长度
        /// </summary>
        public int Content_Length { get; set; }

        /// <summary>
        /// 内容编码
        /// </summary>
        public string Content_Encoding { get; set; }

        /// <summary>
        /// 内容语言
        /// </summary>
        public string ContentLanguage { get; set; }

        /// <summary>
        /// 请求头集合
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// HTTP协议版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 协议名称
        /// </summary>
        public string Protocols { get; set; }


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

        /// <summary>
        /// 释放所占资源
        /// </summary>
        public void Dispose()
        {
            if (this.Body != null)
            {
                this.Body.Dispose();
                this.Body = null;
            }
        }

    }
}
