using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http服务器终端接口
    /// </summary>
    public interface IHttpSocketClient : IHttpClientBase, ISocketClient
    {
    }
}