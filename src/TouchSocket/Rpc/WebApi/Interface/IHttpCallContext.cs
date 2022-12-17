using TouchSocket.Http;

namespace TouchSocket.Rpc.WebApi
{
    /// <summary>
    /// IHttpCallContext
    /// </summary>
    public interface IHttpCallContext : ICallContext
    {
        /// <summary>
        /// Http上下文
        /// </summary>
        HttpContext HttpContext { get; }
    }
}
