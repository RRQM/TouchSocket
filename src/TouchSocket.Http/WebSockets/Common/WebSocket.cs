//using System.Threading.Tasks;
//using TouchSocket.Core;

//namespace TouchSocket.Http.WebSockets
//{
//    public class WebSocket : IWebSocket
//    {
//        private readonly IHttpClientBase m_client;
//        private readonly bool m_isServer;

//        public WebSocket(IHttpClientBase client, bool isServer)
//        {
//            this.m_client = client;
//            this.m_isServer = isServer;
//        }

//        public bool IsHandshaked => this.m_client.GetHandshaked();

//        public void Close(string msg)
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close }.AppendText(msg))
//            {
//                this.Send(frame);
//            }
//        }

//        public void Ping()
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping })
//            {
//                this.Send(frame);
//            }
//        }

//        public void Pong()
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong })
//            {
//                this.Send(frame);
//            }
//        }

//        public void Send(WSDataFrame dataFrame)
//        {
//            using (var byteBlock = new ByteBlock(dataFrame.PayloadLength + 1024))
//            {
//                if (this.m_isServer)
//                {
//                    dataFrame.BuildResponse(byteBlock);
//                }
//                else
//                {
//                    dataFrame.BuildRequest(byteBlock);
//                }

//                this.m_client.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
//            }
//        }

//        /// <summary>
//        /// 采用WebSocket协议，发送WS数据。
//        /// </summary>
//        /// <param name="dataFrame"></param>
//        public Task SendAsync(WSDataFrame dataFrame)
//        {
//            using (var byteBlock = new ByteBlock(dataFrame.PayloadLength + 1024))
//            {
//                if (this.m_isServer)
//                {
//                    dataFrame.BuildResponse(byteBlock);
//                }
//                else
//                {
//                    dataFrame.BuildRequest(byteBlock);
//                }

//                return this.m_client.DefaultSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
//            }
//        }
//    }
//}