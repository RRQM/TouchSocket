using System;
using System.Threading.Tasks;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;

namespace TouchSocket.JsonRpc
{
    internal sealed class WebSocketServerJsonRpcClient : JsonRpcActionClientBase
    {
        private readonly IHttpSocketClient m_client;

        public WebSocketServerJsonRpcClient(IHttpSocketClient client)
        {
            if (client.Protocol != Sockets.Protocol.WebSocket)
            {
                throw new Exception("必须完成WebSocket连接");
            }
            this.m_client = client;
        }

        protected override void SendJsonString(string jsonString)
        {
            this.m_client.SendWithWS(jsonString);
        }

        protected override Task SendJsonStringAsync(string jsonString)
        {
            return this.m_client.SendWithWSAsync(jsonString);
        }
    }
}