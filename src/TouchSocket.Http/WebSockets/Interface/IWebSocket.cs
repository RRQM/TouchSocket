//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading.Tasks;
//using TouchSocket.Core;

//namespace TouchSocket.Http.WebSockets
//{
//    /// <summary>
//    /// IWebSocket
//    /// </summary>
//    public interface IWebSocket
//    {
//        /// <summary>
//        /// 表示当前WebSocket是否已经完成连接。
//        /// </summary>
//        bool IsHandshaked { get; }

//        /// <summary>
//        /// 发送Close报文。
//        /// </summary>
//        /// <param name="msg"></param>
//        void Close(string msg);

//        /// <summary>
//        /// 发送Ping报文。
//        /// </summary>
//        void Ping();

//        /// <summary>
//        /// 发送Pong报文。
//        /// </summary>
//        void Pong();

//        /// <summary>
//        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
//        /// </summary>
//        /// <param name="dataFrame"></param>
//        void Send(WSDataFrame dataFrame);

//        /// <summary>
//        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
//        /// </summary>
//        /// <param name="dataFrame"></param>
//        /// <returns></returns>
//        Task SendAsync(WSDataFrame dataFrame);
//    }
//}