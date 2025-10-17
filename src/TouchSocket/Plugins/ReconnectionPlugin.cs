// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// 重连插件
/// </summary>
public class ReconnectionPlugin<TClient> : PluginBase, ILoadedConfigPlugin
    where TClient : IConnectableClient, IOnlineClient, IDependencyClient
{
    private readonly CancellationTokenSource m_cts = new CancellationTokenSource();
    private readonly TimeSpan m_pollingInterval;
    private readonly Func<TClient, int, Task<ConnectionCheckResult>> m_checkAction;
    private readonly Func<TClient, CancellationToken, Task<bool>> m_connectAction;
    private readonly int m_tryCount;
    private readonly int m_sleepTime;
    private readonly bool m_printLog;
    private readonly Action<TClient>? m_successCallback;
    private readonly Func<TClient, int, Exception, bool>? m_failCallback;

    /// <summary>
    /// 重连插件
    /// </summary>
    /// <param name="options">重连配置选项</param>
    public ReconnectionPlugin(ReconnectionOptions<TClient> options)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(options, nameof(options));
        // 接收所有配置成员到全局变量
        this.m_pollingInterval = options.PollingInterval;
        this.m_tryCount = options.TryCount;
        this.m_sleepTime = options.SleepTime;
        this.m_printLog = options.PrintLog;
        this.m_successCallback = options.SuccessCallback;
        this.m_failCallback = options.FailCallback;

        // 初始化默认的检查动作
        this.m_checkAction = options.CheckAction ?? ((c, count) =>
        {
            if (c.Online)
            {
                return Task.FromResult(ConnectionCheckResult.Alive);
            }
            else
            {
                return Task.FromResult(ConnectionCheckResult.Dead);
            }
        });

        // 初始化默认的连接动作
        this.m_connectAction = options.ConnectAction ?? this.BuildDefaultConnectAction();
    }

    /// <summary>
    /// 获取用于取消操作的 <see cref="CancellationToken"/>
    /// </summary>
    public CancellationToken CancellationToken => this.m_cts.Token;

    /// <summary>
    /// 轮询时间间隔
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

    private Func<TClient, CancellationToken, Task<bool>> BuildDefaultConnectAction()
    {
        return async (client, cancellationToken) =>
        {
            var tryT = this.m_tryCount;
            var tryCount = 0;

            while (this.m_tryCount < 0 || tryT-- > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return true;
                }

                try
                {
                    if (client.Online)
                    {
                        return true;
                    }

                    await Task.Delay(1000, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    await client.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                    this.m_successCallback?.Invoke(client);
                    return true;
                }
                catch (Exception ex)
                {
                    if (this.m_printLog)
                    {
                        client.Logger?.Exception(this, ex);
                    }

                    if (this.m_failCallback != null)
                    {
                        if (!this.m_failCallback.Invoke(client, ++tryCount, ex))
                        {
                            return true;
                        }
                    }

                    await Task.Delay(this.m_sleepTime, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }

            return true;
        };
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
                    var checkResult = await this.m_checkAction.Invoke(client, failCount).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    switch (checkResult)
                    {
                        case ConnectionCheckResult.Skip:
                            continue;
                        case ConnectionCheckResult.Dead:
                            if (await this.m_connectAction.Invoke(client, this.m_cts.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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