//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using TouchSocket.Core;

//namespace TouchSocket.Http.WebSockets
//{
//    /// <summary>
//    /// WebSocketExtension
//    /// </summary>
//    public static class WebSocketExtension
//    {
//        #region 同步发送

//        /// <summary>
//        /// 采用WebSocket协议，发送二进制流数据。
//        /// </summary>
//        /// <param name="webSocket"></param>
//        /// <param name="buffer"></param>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        public static void Send(this IWebSocket webSocket,byte[] buffer, int offset, int length)
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Binary })
//            {
//                if (offset == 0)
//                {
//                    frame.PayloadData = new ByteBlock(buffer, length);
//                }
//                else
//                {
//                    frame.AppendBinary(buffer, offset, length);
//                }
//                webSocket.Send(frame);
//            }
//        }

//        /// <summary>
//        /// 采用WebSocket协议，发送二进制流数据。
//        /// </summary>
//        /// <param name="webSocket"></param>
//        /// <param name="byteBlock"></param>
//        public static void Send(this IWebSocket webSocket,ByteBlock byteBlock)
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Binary })
//            {
//                frame.PayloadData = byteBlock;
//                webSocket.Send(frame);
//            }
//        }

//        /// <summary>
//        /// 采用WebSocket协议，发送二进制流数据。
//        /// </summary>
//        /// <param name="webSocket"></param>
//        /// <param name="buffer"></param>
//        public static void Send(this IWebSocket webSocket,byte[] buffer)
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Binary })
//            {
//                frame.PayloadData = new ByteBlock(buffer);
//                webSocket.Send(frame);
//            }
//        }

//        /// <summary>
//        /// 采用WebSocket协议，发送文本数据。
//        /// </summary>
//        /// <param name="webSocket"></param>
//        /// <param name="text"></param>
//        public static void Send(this IWebSocket webSocket,string text)
//        {
//            using (var dataFrame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Text })
//            {
//                dataFrame.AppendText(text);
//                webSocket.Send(dataFrame);
//            }
//        }



//        #endregion 同步发送

//        #region 异步发送

//        /// <summary>
//        /// 采用WebSocket协议，发送二进制流数据。
//        /// </summary>
//        /// <param name="webSocket"></param>
//        /// <param name="buffer"></param>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        public static Task SendAsync(this IWebSocket webSocket, byte[] buffer, int offset, int length)
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Binary })
//            {
//                if (offset == 0)
//                {
//                    frame.PayloadData = new ByteBlock(buffer, length);
//                }
//                else
//                {
//                    frame.AppendBinary(buffer, offset, length);
//                }
//                return webSocket.SendAsync(frame);
//            }
//        }

//        /// <summary>
//        /// 采用WebSocket协议，发送二进制流数据。
//        /// </summary>
//        /// <param name="webSocket"></param>
//        /// <param name="buffer"></param>
//        public static Task SendAsync(this IWebSocket webSocket,byte[] buffer)
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Binary })
//            {
//                frame.PayloadData = new ByteBlock(buffer);
//                return webSocket.SendAsync(frame);
//            }
//        }

//        /// <summary>
//        /// 采用WebSocket协议，发送文本数据。
//        /// </summary>
//        /// <param name="webSocket"></param>
//        /// <param name="text"></param>
//        public static Task SendAsync(this IWebSocket webSocket,string text)
//        {
//            using (var dataFrame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Text })
//            {
//                dataFrame.AppendText(text);
//                return webSocket.SendAsync(dataFrame);
//            }
//        }

//        #endregion 异步发送
//    }
//}
