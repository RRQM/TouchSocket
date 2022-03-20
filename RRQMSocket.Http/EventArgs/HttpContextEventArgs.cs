using RRQMCore;
using System;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http请求事件类
    /// </summary>
    public class HttpContextEventArgs : RRQMEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="request"></param>
        public HttpContextEventArgs(HttpRequest request)
        {
            this.Request = request;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public HttpContextEventArgs(HttpRequest request, HttpResponse response) : this(request)
        {
            this.Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        /// <summary>
        /// Http请求
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// Http响应。
        /// </summary>
        public HttpResponse Response { get; set; }
    }
}
