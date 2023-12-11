using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// IHttpService
    /// </summary>
    public interface IHttpService<TClient> : IHttpServiceBase, ITcpService<TClient> where TClient : IHttpSocketClient
    {
    }

    /// <summary>
    /// IHttpService
    /// </summary>
    public interface IHttpService : IHttpService<HttpSocketClient>
    {

    }
}
