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

using System.Net.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// 定义WebSocket服务的接口，继承自IDisposable, IOnlineClient, IClosableClient接口。
/// </summary>
public interface IWebSocket : IDisposable, IOnlineClient, IClosableClient, IResolverObject, IClient
{
    /// <summary>
    /// 允许异步Read读取
    /// </summary>
    bool AllowAsyncRead { get; set; }

    /// <summary>
    /// 使用的Http客户端
    /// </summary>
    IHttpSession Client { get; }

    /// <summary>
    /// WebSocket版本
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 获取最后WebSocket关闭状态。
    /// </summary>
    WebSocketCloseStatus CloseStatus { get; }

    /// <summary>
    /// 异步关闭WebSocket连接。
    /// </summary>
    /// <param name="closeStatus">关闭状态。</param>
    /// <param name="statusDescription">状态描述。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个任务对象，表示异步操作的结果。</returns>
    Task<Result> CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步发送Ping请求。
    /// </summary>
    /// <returns>任务完成时返回。</returns>
    Task<Result> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步执行Pong操作。
    /// </summary>
    /// <returns>一个任务对象，表示异步操作的完成。</returns>
    Task<Result> PongAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步等待读取数据
    /// </summary>
    /// <param name="cancellationToken">用于取消异步读取操作的取消令牌</param>
    /// <returns>返回一个值任务，该任务完成后将包含WebSocket接收的结果</returns>
    ValueTask<IWebSocketReceiveResult> ReadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
    /// </summary>
    /// <param name="dataFrame">要发送的数据帧</param>
    /// <param name="endOfMessage">是否是消息的结束标志，默认为<see langword="true"/></param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个异步任务，用于指示发送操作的完成状态</returns>
    Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步发送文本消息。
    /// </summary>
    /// <param name="text">要发送的文本内容。</param>
    /// <param name="endOfMessage">指示是否是消息的结束。默认为<see langword="true"/>。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个任务对象，表示异步操作的结果。</returns>
    Task SendAsync(string text, bool endOfMessage = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步发送指定的字节内存数据。
    /// </summary>
    /// <param name="memory">要发送的字节数据，作为只读内存块。</param>
    /// <param name="endOfMessage">指示当前数据是否为消息的结束。默认为<see langword="true"/>。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <remarks>
    /// 此方法允许异步发送数据，通过指定是否为消息的结束来控制数据流。
    /// </remarks>
    Task SendAsync(ReadOnlyMemory<byte> memory, bool endOfMessage = true, CancellationToken cancellationToken = default);
}