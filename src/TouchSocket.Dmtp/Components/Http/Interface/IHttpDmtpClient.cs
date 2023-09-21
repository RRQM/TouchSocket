using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IHttpDmtpClient
    /// </summary>
    public interface IHttpDmtpClient : IHttpClient, IHttpDmtpClientBase
    {
        /// <summary>
        /// 建立Tcp，并发送Http请求，最后完成Dmtp握手连接。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IHttpDmtpClient Connect(CancellationToken token, int timeout = 5000);

        /// <summary>
        /// 建立Tcp，并发送Http请求，最后完成Dmtp握手连接。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<IHttpDmtpClient> ConnectAsync(CancellationToken token, int timeout = 5000);
    }
}