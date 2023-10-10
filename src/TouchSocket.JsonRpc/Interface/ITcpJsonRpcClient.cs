using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的JsonRpc客户端。
    /// </summary>
    public interface ITcpJsonRpcClient : IJsonRpcClient, ITcpClient,IRpcParser
    {
    }
}