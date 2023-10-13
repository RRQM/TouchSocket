using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebSocket
    /// </summary>
    public interface IWebSocket : IDisposable
    {
        /// <summary>
        /// 表示当前WebSocket是否已经完成连接。
        /// </summary>
        bool IsHandshaked { get; }

        /// <summary>
        /// WebSocket版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 发送Close报文。
        /// </summary>
        /// <param name="msg"></param>
        void Close(string msg);

        /// <summary>
        /// 发送Close报文
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task CloseAsync(string msg);

        /// <summary>
        /// 发送Ping报文。
        /// </summary>
        void Ping();

        /// <summary>
        /// 发送Ping报文
        /// </summary>
        /// <returns></returns>
        Task PingAsync();

        /// <summary>
        /// 发送Pong报文。
        /// </summary>
        void Pong();

        /// <summary>
        /// 发送Pong报文
        /// </summary>
        /// <returns></returns>
        Task PongAsync();

        /// <summary>
        /// 异步等待读取数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<WebSocketReceiveResult> ReadAsync(CancellationToken token);
#if NET6_0_OR_GREATER
        /// <summary>
        /// 值异步等待读取数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ValueTask<WebSocketReceiveResult> ValueReadAsync(CancellationToken token);
#endif
        /// <summary>
        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="endOfMessage"></param>
        void Send(WSDataFrame dataFrame, bool endOfMessage = true);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        void Send(string text, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="endOfMessage"></param>
        void Send(byte[] buffer, int offset, int length, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="endOfMessage"></param>
        void Send(ByteBlock byteBlock, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="endOfMessage"></param>
        void Send(byte[] buffer, bool endOfMessage = true);

        /// <summary>
        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(string text, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(byte[] buffer, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(byte[] buffer, int offset, int length, bool endOfMessage = true);
    }
}