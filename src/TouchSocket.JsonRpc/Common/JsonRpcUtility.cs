using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcUtility
    /// </summary>
    public static class JsonRpcUtility
    {
        /// <summary>
        /// TcpJsonRpc
        /// </summary>
        public static Protocol TcpJsonRpc { get; private set; } = new Protocol("TcpJsonRpc");
    }
}