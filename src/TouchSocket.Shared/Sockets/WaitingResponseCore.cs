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

using System.Net;

namespace TouchSocket.Sockets;

/// <summary>
/// "发送并等待响应"的公共实现，供 <see cref="WaitingTcpClient"/>、<see cref="WaitingTcpSessionClient"/>、<see cref="WaitingUdpSession"/> 复用。
/// <para>同一时刻只允许一个等待请求；请求挂起期间收到的数据先尝试匹配，不匹配的数据由调用方按常规路径继续处理。</para>
/// </summary>
internal sealed class WaitingResponseCore
{
    private readonly SemaphoreSlim m_requestLocker = new SemaphoreSlim(1, 1);
    private PendingRequest m_pending;

    /// <summary>
    /// 发送数据并等待匹配的响应。
    /// </summary>
    /// <param name="sendAsync">实际的发送操作。</param>
    /// <param name="filterFuncAsync">响应过滤器，为 <see langword="null"/> 时第一条数据即为响应。</param>
    /// <param name="remoteEndPoint">只接受来自该终结点的响应，为 <see langword="null"/> 时不按地址过滤。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task<ResponsedData> SendThenResponseAsync(Func<CancellationToken, Task> sendAsync, Func<ResponsedData, Task<bool>> filterFuncAsync, EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        await this.m_requestLocker.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            // 先挂起等待，再发送，避免响应先于等待到达而丢失。
            var pending = new PendingRequest(filterFuncAsync, remoteEndPoint);
            Volatile.Write(ref this.m_pending, pending);
            try
            {
                using (cancellationToken.Register(() => pending.TrySetCanceled(cancellationToken)))
                {
                    try
                    {
                        await sendAsync(cancellationToken).ConfigureDefaultAwait();
                    }
                    catch
                    {
                        // 发送失败时结束等待；若响应已先一步到达，释放它。
                        if (!pending.TrySetCanceled(CancellationToken.None) && pending.Task.Status == TaskStatus.RanToCompletion)
                        {
                            pending.Task.Result.Dispose();
                        }
                        throw;
                    }

                    return await pending.Task.ConfigureDefaultAwait();
                }
            }
            finally
            {
                Interlocked.CompareExchange(ref this.m_pending, null, pending);
            }
        }
        finally
        {
            this.m_requestLocker.Release();
        }
    }

    /// <summary>
    /// 尝试用收到的数据完成挂起的等待请求。
    /// </summary>
    /// <returns>数据被等待请求取走时返回 <see langword="true"/>，否则调用方应按常规路径继续处理数据。</returns>
    public ValueTask<bool> TryHandleAsync(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
    {
        var pending = Volatile.Read(ref this.m_pending);
        if (pending == null)
        {
            return new ValueTask<bool>(false);
        }
        return pending.TryCompleteAsync(remoteEndPoint, memory, requestInfo);
    }

    /// <summary>
    /// 连接断开或服务停止时，让挂起的等待请求以异常结束。
    /// </summary>
    public void Fail(Exception exception)
    {
        Volatile.Read(ref this.m_pending)?.TrySetException(exception);
    }

    private sealed class PendingRequest
    {
        private readonly Func<ResponsedData, Task<bool>> m_filterFuncAsync;
        private readonly EndPoint m_remoteEndPoint;
        private readonly TaskCompletionSource<ResponsedData> m_taskSource = new TaskCompletionSource<ResponsedData>(TaskCreationOptions.RunContinuationsAsynchronously);

        public PendingRequest(Func<ResponsedData, Task<bool>> filterFuncAsync, EndPoint remoteEndPoint)
        {
            this.m_filterFuncAsync = filterFuncAsync;
            this.m_remoteEndPoint = remoteEndPoint;
        }

        public Task<ResponsedData> Task => this.m_taskSource.Task;

        public bool TrySetCanceled(CancellationToken cancellationToken)
        {
            return this.m_taskSource.TrySetCanceled(cancellationToken);
        }

        public bool TrySetException(Exception exception)
        {
            return this.m_taskSource.TrySetException(exception);
        }

        public async ValueTask<bool> TryCompleteAsync(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
        {
            if (this.m_taskSource.Task.IsCompleted)
            {
                return false;
            }

            if (!EndPointMatches(this.m_remoteEndPoint, remoteEndPoint))
            {
                return false;
            }

            var response = new ResponsedData(memory, requestInfo);
            bool matched;
            try
            {
                var filterFuncAsync = this.m_filterFuncAsync;
                matched = filterFuncAsync == null || await filterFuncAsync.Invoke(response).ConfigureDefaultAwait();
            }
            catch (Exception ex)
            {
                // 过滤器异常交给等待方，数据仍按常规路径继续处理。
                response.Dispose();
                this.m_taskSource.TrySetException(ex);
                return false;
            }

            if (matched && this.m_taskSource.TrySetResult(response))
            {
                return true;
            }

            // 不匹配（或请求已结束）的数据不会交给等待方，需要归还其缓冲区。
            response.Dispose();
            return false;
        }

        private static bool EndPointMatches(EndPoint expected, EndPoint actual)
        {
            // 未指定远程地址，或指定的不是 IP 终结点（例如尚未解析的域名）时，不按地址过滤。
            if (expected is not IPEndPoint expectedIp)
            {
                return true;
            }

            if (actual is not IPEndPoint actualIp)
            {
                return false;
            }

            return expectedIp.Port == actualIp.Port && Normalize(expectedIp.Address).Equals(Normalize(actualIp.Address));
        }

        private static IPAddress Normalize(IPAddress address)
        {
            return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
        }
    }
}
