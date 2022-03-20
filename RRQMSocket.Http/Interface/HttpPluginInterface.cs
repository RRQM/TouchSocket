namespace RRQMSocket.Http
{
    /// <summary>
    /// Http接口
    /// </summary>
    public interface IHttpPlugin : IPlugin
    {
        /// <summary>
        /// 在收到其他Http请求时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnReceivedOtherHttpRequest(ITcpClientBase client, HttpContextEventArgs e);

        /// <summary>
        /// 在收到Get时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnGet(ITcpClientBase client, HttpContextEventArgs e);

        /// <summary>
        /// 在收到Put时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnPut(ITcpClientBase client, HttpContextEventArgs e);

        /// <summary>
        /// 在收到Delete时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnDelete(ITcpClientBase client, HttpContextEventArgs e);

        /// <summary>
        /// 在收到Post时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnPost(ITcpClientBase client, HttpContextEventArgs e);
    }
}
