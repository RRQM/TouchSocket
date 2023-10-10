using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            m_client.Send(jsonString);
        }

        protected override Task SendJsonStringAsync(string jsonString)
        {
            return m_client.SendAsync(jsonString);
        }
    }
}
