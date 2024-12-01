using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 表示一个 Web API 请求。
    /// </summary>
    public class WebApiRequest
    {
        /// <summary>
        /// 获取或设置 HTTP 方法类型。
        /// </summary>
        public HttpMethodType Method { get; set; }

        /// <summary>
        /// 获取或设置请求的主体。
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        /// 获取或设置请求的内容类型。
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 获取或设置请求的头部信息。
        /// </summary>
        public KeyValuePair<string, string>[] Headers { get; set; }

        /// <summary>
        /// 获取或设置请求的查询参数。
        /// </summary>
        public KeyValuePair<string, string>[] Querys { get; set; }

        /// <summary>
        /// 获取或设置请求的表单数据。
        /// </summary>
        public KeyValuePair<string, string>[] Forms { get; set; }
    }
}
