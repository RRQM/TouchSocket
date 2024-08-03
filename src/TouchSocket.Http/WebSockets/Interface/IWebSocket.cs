//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebSocket
    /// </summary>
    public interface IWebSocket : IDisposable, IOnlineClient, IClosableClient
    {
        /// <summary>
        /// WebSocket版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 允许异步Read读取
        /// </summary>
        bool AllowAsyncRead { get; set; }

        /// <summary>
        /// 使用的Http客户端
        /// </summary>
        IHttpSession Client { get; }

        /// <summary>
        /// 发送Ping报文
        /// </summary>
        /// <returns></returns>
        Task PingAsync();

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
        Task<IWebSocketReceiveResult> ReadAsync(CancellationToken token);

        /// <summary>
        /// 值异步等待读取数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        ValueTask<IWebSocketReceiveResult> ValueReadAsync(CancellationToken token);

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
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(ReadOnlyMemory<byte> memory, bool endOfMessage = true);
    }
}