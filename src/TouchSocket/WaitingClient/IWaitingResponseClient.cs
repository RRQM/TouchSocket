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

using System.Text;

namespace TouchSocket.Sockets;

/// <summary>
/// 支持"发送并等待响应"的客户端。
/// <para>
/// 同一客户端同一时刻只处理一个等待请求，并发调用会排队。请求挂起期间收到的数据先交给过滤器匹配，
/// 匹配的数据作为响应返回，不再触发 Received 与插件；不匹配的数据照常进入 Received 与插件。
/// </para>
/// </summary>
public interface IWaitingResponseClient
{
    /// <summary>
    /// 发送数据并等待响应。
    /// </summary>
    /// <param name="memory">要发送的数据。</param>
    /// <param name="waitingOptions">等待选项，为 <see langword="null"/> 时第一条收到的数据即为响应。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>响应数据，使用完毕后需要释放。</returns>
    /// <exception cref="ClientNotConnectedException">发送前未连接，或等待期间连接断开。</exception>
    /// <exception cref="OperationCanceledException">等待被取消。</exception>
    Task<ResponsedData> SendThenResponseAsync(ReadOnlyMemory<byte> memory, WaitingOptions waitingOptions = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送请求信息并等待响应。
    /// </summary>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <param name="waitingOptions">等待选项，为 <see langword="null"/> 时第一条收到的数据即为响应。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>响应数据，使用完毕后需要释放。</returns>
    /// <exception cref="ClientNotConnectedException">发送前未连接，或等待期间连接断开。</exception>
    /// <exception cref="OperationCanceledException">等待被取消。</exception>
    Task<ResponsedData> SendThenResponseAsync(IRequestInfo requestInfo, WaitingOptions waitingOptions = default, CancellationToken cancellationToken = default);
}

/// <summary>
/// <see cref="IWaitingResponseClient"/> 的扩展方法。
/// </summary>
public static class WaitingResponseClientExtension
{
    /// <summary>
    /// 发送数据并等待响应，超时抛出 <see cref="TimeoutException"/>。
    /// </summary>
    /// <param name="client">等待响应客户端。</param>
    /// <param name="memory">要发送的数据。</param>
    /// <param name="millisecondsTimeout">超时时间（毫秒）。</param>
    /// <param name="waitingOptions">等待选项。</param>
    /// <returns>响应数据，使用完毕后需要释放。</returns>
    /// <exception cref="TimeoutException">等待超时。</exception>
    public static async Task<ResponsedData> SendThenResponseAsync(this IWaitingResponseClient client, ReadOnlyMemory<byte> memory, int millisecondsTimeout, WaitingOptions waitingOptions = default)
    {
        using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            try
            {
                return await client.SendThenResponseAsync(memory, waitingOptions, tokenSource.Token).ConfigureDefaultAwait();
            }
            catch (OperationCanceledException) when (tokenSource.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }
    }

    /// <summary>
    /// 发送请求信息并等待响应，超时抛出 <see cref="TimeoutException"/>。
    /// </summary>
    /// <param name="client">等待响应客户端。</param>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <param name="millisecondsTimeout">超时时间（毫秒）。</param>
    /// <param name="waitingOptions">等待选项。</param>
    /// <returns>响应数据，使用完毕后需要释放。</returns>
    /// <exception cref="TimeoutException">等待超时。</exception>
    public static async Task<ResponsedData> SendThenResponseAsync(this IWaitingResponseClient client, IRequestInfo requestInfo, int millisecondsTimeout, WaitingOptions waitingOptions = default)
    {
        using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            try
            {
                return await client.SendThenResponseAsync(requestInfo, waitingOptions, tokenSource.Token).ConfigureDefaultAwait();
            }
            catch (OperationCanceledException) when (tokenSource.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }
    }

    /// <summary>
    /// 以 UTF-8 编码发送字符串并等待响应，超时抛出 <see cref="TimeoutException"/>。
    /// </summary>
    /// <param name="client">等待响应客户端。</param>
    /// <param name="value">要发送的字符串。</param>
    /// <param name="millisecondsTimeout">超时时间（毫秒）。</param>
    /// <param name="waitingOptions">等待选项。</param>
    /// <returns>响应数据，使用完毕后需要释放。</returns>
    /// <exception cref="TimeoutException">等待超时。</exception>
    public static Task<ResponsedData> SendThenResponseAsync(this IWaitingResponseClient client, string value, int millisecondsTimeout = 5000, WaitingOptions waitingOptions = default)
    {
        return client.SendThenResponseAsync(Encoding.UTF8.GetBytes(value), millisecondsTimeout, waitingOptions);
    }
}
