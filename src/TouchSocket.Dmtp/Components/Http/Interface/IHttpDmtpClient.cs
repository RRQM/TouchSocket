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
    }
}