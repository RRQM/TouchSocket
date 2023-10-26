using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    internal class TcpServerJsonRpcClient : JsonRpcActionClientBase
    {
        private ISocketClient m_client;

        public TcpServerJsonRpcClient(ISocketClient socketClient)
        {
            this.m_client = socketClient;
        }

        protected override void SendJsonString(string jsonString)
        {
            this.m_client.Send(jsonString);
        }

        protected override Task SendJsonStringAsync(string jsonString)
        {
            return this.m_client.SendAsync(jsonString);
        }
    }
}