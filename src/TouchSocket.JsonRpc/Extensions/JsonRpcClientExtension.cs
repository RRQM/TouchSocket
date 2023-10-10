using Newtonsoft.Json.Linq;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcClientExtension
    /// </summary>
    public static class JsonRpcClientExtension
    {
        /// <summary>
        /// 标识是否为JsonRpc
        /// </summary>
        public static readonly DependencyProperty<bool> IsJsonRpcProperty =
            DependencyProperty<bool>.Register("IsJsonRpc", false);

        /// <summary>
        /// IJsonRpcActionClient
        /// </summary>
        public static readonly DependencyProperty<IJsonRpcActionClient> JsonRpcActionClientProperty =
            DependencyProperty<IJsonRpcActionClient>.Register("JsonRpcActionClient", default);

        /// <summary>
        /// 获取<see cref="IsJsonRpcProperty"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool GetIsJsonRpc(this ITcpClientBase client)
        {
            return client.GetValue(IsJsonRpcProperty);
        }

        /// <summary>
        /// 设置<see cref="IsJsonRpcProperty"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="value"></param>
        public static void SetIsJsonRpc(this ITcpClientBase client, bool value = true)
        {
            client.SetValue(IsJsonRpcProperty, value);
        }

        /// <summary>
        /// 获取基于Tcp协议或者WebSocket协议的双工JsonRpc端
        /// </summary>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        public static IJsonRpcActionClient GetJsonRpcActionClient(this ISocketClient socketClient)
        {
            if (socketClient.TryGetValue(JsonRpcActionClientProperty, out var actionClient))
            {
                return actionClient;
            }

            if (socketClient.Protocol== Sockets.Protocol.Tcp)
            {
                actionClient = new TcpServerJsonRpcClient(socketClient);
                socketClient.SetValue(JsonRpcActionClientProperty, actionClient);
                return actionClient;
            }
            else if (socketClient.Protocol == Sockets.Protocol.WebSocket)
            {
                actionClient = new WebSocketServerJsonRpcClient((IHttpSocketClient)socketClient);
                socketClient.SetValue(JsonRpcActionClientProperty, actionClient);
                return actionClient;
            }
            else
            {
                throw new System.Exception("SocketClient必须是Tcp协议，或者完成WebSocket连接");
            }
           
        }
    }
}