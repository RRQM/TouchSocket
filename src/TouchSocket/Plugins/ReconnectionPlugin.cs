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

using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// 重连插件
/// </summary>
public class ReconnectionPlugin<TClient> : PluginBase, ILoadedConfigPlugin
    where TClient : IConnectableClient, IOnlineClient, IDependencyClient
{
    private readonly CancellationTokenSource m_cts = new CancellationTokenSource();
    private TimeSpan m_pollingInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 重连插件
    /// </summary>
    public ReconnectionPlugin()
    {
        this.ActionForConnect = async (c, cancellationToken) =>
        {
            try
            {
                await c.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return true;
            }
            catch
            {
                return false;
            }
        };

        this.ActionForCheck = (c, count) =>
        {
            if (c.Online)
            {
                return Task.FromResult(ConnectionCheckResult.Alive);
            }
            else
            {
                return Task.FromResult(ConnectionCheckResult.Dead);
            }
        };
    }

    /// <summary>
    /// 每个周期可执行的委托。用于检验客户端活性。
    /// <list type="bullet">
    /// <item>返回<see cref="ConnectionCheckResult.Alive"/>表示存活</item>
    /// <item>返回<see cref="ConnectionCheckResult.Dead"/>表示失活，需要马上重连</item>
    /// <item>返回<see cref="ConnectionCheckResult.Skip"/>表示跳过此次检查</item>
    /// </list>
    /// </summary>
    public Func<TClient, int, Task<ConnectionCheckResult>> ActionForCheck { get; set; }

    /// <summary>
    /// 尝试连接的委托。参数为客户端实例和取消令牌，返回连接是否成功的异步结果。
    /// </summary>
    /// <remarks>
    /// 注意：此方法应当处理连接失败的异常，并返回<see langword="false"/>，而不是抛出异常。
    /// </remarks>
    public Func<TClient, CancellationToken, Task<bool>> ActionForConnect { get; set; }

    /// <summary>
    /// 获取用于取消操作的 <see cref="CancellationToken"/>。
    /// </summary>
    public CancellationToken CancellationToken => this.m_cts.Token;

    /// <summary>
    /// 检验时间间隔
    /// </summary>
    public TimeSpan PollingInterval => this.m_pollingInterval;

    /// <inheritdoc/>
    public async Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
    {
        if (sender is TClient client)
        {
            _ = EasyTask.SafeRun(this.BeginReconnect, client);
        }

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 设置一个周期性执行的委托，用于检查客户端状态。
    /// </summary>
    /// <param name="actionForCheck">一个委托，接受客户端实例和周期次数作为参数，返回一个任务，该任务结果为连接检查结果枚举。</param>
    /// <returns>返回当前ReconnectionPlugin实例，以便链式调用。</returns>
    public ReconnectionPlugin<TClient> SetActionForCheck(Func<TClient, int, Task<ConnectionCheckResult>> actionForCheck)
    {
        this.ActionForCheck = actionForCheck;
        return this;
    }

    /// <summary>
    /// 设置连接动作
    /// </summary>
    /// <param name="tryConnect">一个异步方法，尝试建立连接，并返回一个布尔值指示连接是否成功</param>
    /// <returns>返回当前实例，以便支持链式调用</returns>
    public ReconnectionPlugin<TClient> SetConnectAction(Func<TClient, CancellationToken, Task<bool>> tryConnect)
    {
        this.ActionForConnect = tryConnect;

        return this;
    }

    /// <summary>
    /// 设置连接动作
    /// </summary>
    /// <param name="sleepTime">失败时间隔时间</param>
    /// <param name="failCallback">失败时回调（参数依次为：客户端，本轮尝试重连次数，异常信息）。如果回调为<see langword="null"/>或者返回<see langword="false"/>，则终止尝试下次连接。</param>
    /// <param name="successCallback">成功连接时回调</param>
    /// <returns></returns>
    public ReconnectionPlugin<TClient> SetConnectAction(TimeSpan sleepTime,
        Func<TClient, int, Exception, bool> failCallback = default,
        Action<TClient> successCallback = default)
    {
        this.SetConnectAction(async (client, cancellationToken) =>
        {
            var tryT = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (client.Online)
                    {
                        return true;
                    }
                    else
                    {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        await client.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }

                    successCallback?.Invoke(client);
                    return true;
                }
                catch (Exception ex)
                {
                    await Task.Delay(sleepTime, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (failCallback?.Invoke(client, ++tryT, ex) != true)
                    {
                        return true;
                    }
                }
            }
            return true;
        });
        return this;
    }

    /// <summary>
    /// 设置连接动作
    /// </summary>
    /// <param name="tryCount">尝试重连次数，设为-1时则永远尝试连接</param>
    /// <param name="printLog">是否输出日志。</param>
    /// <param name="sleepTime">失败时，停留时间</param>
    /// <param name="successCallback">成功回调函数</param>
    /// <returns></returns>
    public ReconnectionPlugin<TClient> SetConnectAction(int tryCount = 10, bool printLog = false, int sleepTime = 1000, Action<TClient> successCallback = null)
    {
        this.SetConnectAction(async (client, cancellationToken) =>
        {
            var tryT = tryCount;
            while (tryCount < 0 || tryT-- > 0)
            {
                try
                {
                    if (client.Online)
                    {
                        return true;
                    }
                    else
                    {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        await client.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    successCallback?.Invoke(client);
                    return true;
                }
                catch (Exception ex)
                {
                    if (printLog)
                    {
                        client.Logger?.Exception(this, ex);
                    }
                    await Task.Delay(sleepTime, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            return true;
        });
        return this;
    }

    /// <summary>
    /// 设置轮询的时间间隔。
    /// </summary>
    /// <param name="tick">轮询的时间间隔。</param>
    /// <returns>返回当前 <see cref="ReconnectionPlugin{TClient}"/> 实例，以便链式调用。</returns>
    public ReconnectionPlugin<TClient> SetPollingTick(TimeSpan tick)
    {
        this.m_pollingInterval = tick;
        return this;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_cts.SafeCancel();
            this.m_cts.SafeDispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 执行连接循环，直到连接成功或被取消。
    /// </summary>
    /// <param name="client">要重连的客户端实例。</param>
    /// <returns>表示异步操作的任务。</returns>
    protected async Task ExecuteConnectLoop(TClient client)
    {
        var cancellationToken = this.m_cts.Token;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (await this.ActionForConnect.Invoke(client, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                return;
            }
        }
    }

    private async Task BeginReconnect(TClient client)
    {
        client.Logger?.Debug(this, TouchSocketResource.PollingBegin.Format(this.PollingInterval));

        var failCount = 0;

        try
        {
            while (true)
            {
                if (this.DisposedValue)
                {
                    client.Logger?.Debug(this, TouchSocketResource.PollingWillEnd);
                    return;
                }

                await Task.Delay(this.PollingInterval).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (client.PauseReconnection)
                {
                    client.Logger?.Debug(this, TouchSocketResource.PauseReconnection);
                    continue;
                }
                try
                {
                    var checkResult = await this.ActionForCheck.Invoke(client, failCount).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    switch (checkResult)
                    {
                        case ConnectionCheckResult.Skip:
                            continue;
                        case ConnectionCheckResult.Dead:
                            if (await this.ActionForConnect.Invoke(client, this.m_cts.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                            {
                                failCount = 0;
                            }
                            break;

                        case ConnectionCheckResult.Alive:
                            failCount = 0;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    client.Logger?.Exception(this, ex);
                }
            }
        }
        catch (Exception ex)
        {
            client.Logger?.Exception(this, ex);
        }
        finally
        {
            client.Logger?.Debug(this, TouchSocketResource.PollingEnd);
        }
    }
}