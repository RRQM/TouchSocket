using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// WebApiEventArgs
    /// </summary>
    public partial class WebApiEventArgs : PluginEventArgs
    {
        /// <summary>
        /// WebApiEventArgs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public WebApiEventArgs(HttpRequest request, HttpResponse response)
        {
            this.Request = request;
            this.Response = response;
            this.IsHttpMessage = false;
        }

        /// <summary>
        /// 是否以HttpMessage请求
        /// </summary>
        public bool IsHttpMessage { get; set; }

        /// <summary>
        /// Http请求
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        /// Http响应
        /// </summary>
        public HttpResponse Response { get; }
    }

#if !NET45
    public partial class WebApiEventArgs
    {
        /// <summary>
        /// Http请求
        /// </summary>
        public System.Net.Http.HttpRequestMessage RequestMessage { get; }

        /// <summary>
        /// WebApiEventArgs
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="responseMessage"></param>
        public WebApiEventArgs(System.Net.Http.HttpRequestMessage requestMessage, System.Net.Http.HttpResponseMessage responseMessage)
        {
            this.RequestMessage = requestMessage;
            this.ResponseMessage = responseMessage;
            this.IsHttpMessage = true;
        }

        /// <summary>
        /// Http响应
        /// </summary>
        public System.Net.Http.HttpResponseMessage ResponseMessage { get; }
    }
#endif
}