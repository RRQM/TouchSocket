using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IHttpDmtpService
    /// </summary>
    public interface IHttpDmtpService<TClient> : IHttpDmtpServiceBase, IHttpService<TClient> where TClient : IHttpDmtpSocketClient
    {
    }

    /// <summary>
    /// IHttpDmtpService
    /// </summary>
    public interface IHttpDmtpService : IHttpDmtpService<HttpDmtpSocketClient>
    {

    }
}
