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
/// <typeparam name="TClient">客户端类型</typeparam>
public class ReconnectionPlugin<TClient> : PluginBase, ILoadedConfigPlugin
    where TClient : IConnectableClient, IOnlineClient, IDependencyClient, IClosableClient
{
    private readonly CancellationTokenSource m_cts = new CancellationTokenSource();
    private readonly ReconnectionOption<TClient> m_options;
    private bool m_hasGivenUp;

    /// <summary>
    /// 重连插件
    /// </summary>
    /// <param name="options">重连配置选项</param>
    public ReconnectionPlugin(ReconnectionOption<TClient> options)
    {
        ThrowHelper.ThrowIfNull(options, nameof(options));
        this.m_options = options;
    }

    /// <summary>
    /// 获取用于取消操作的 <see cref="CancellationToken"/>
    /// </summary>
    public CancellationToken CancellationToken => this.m_cts.Token;

    /// <summary>
    /// 重连选项
    /// </summary>
    public ReconnectionOption<TClient> Options => this.m_options;

    /// <summary>
    /// 轮询时间间隔
    /// </summary>
    public TimeSpan PollingInterval => this.m_options.PollingInterval;

    /// <inheritdoc/>
    public async Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
    {
        if (sender is TClient client)
        {
            _ = EasyTask.SafeRun(this.StartReconnectionLoop, client);
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

    private async Task StartReconnectionLoop(TClient client)
    {
        // 初始延时，避免过快重连
        await Task.Delay(1000, CancellationToken.None);

        if (this.m_options.LogReconnection)
        {
            client.Logger?.Debug(this, TouchSocketResource.PollingBegin.Format(this.PollingInterval));
        }

        try
        {
            while (!this.DisposedValue && !this.m_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(this.PollingInterval, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (client.PauseReconnection)
                {
                    if (this.m_options.LogReconnection)
                    {
                        client.Logger?.Debug(this, TouchSocketResource.PauseReconnection);
                    }
                    continue;
                }

                try
                {
                    var checkResult = await this.m_options.CheckAction.Invoke(client).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                    switch (checkResult)
                    {
                        case ConnectionCheckResult.Skip:
                            continue;
                        case ConnectionCheckResult.Dead:
                            {
                                if (!this.m_hasGivenUp)
                                {
                                    await this.TryReconnectWithRetry(client).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                }
                                break;
                            }
                        case ConnectionCheckResult.Alive:
                            this.m_hasGivenUp = false;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (this.m_options.LogReconnection)
                    {
                        client.Logger?.Exception(this, ex);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消，不记录错误
        }
        catch (Exception ex)
        {
            if (this.m_options.LogReconnection)
            {
                client.Logger?.Exception(this, ex);
            }
        }
        finally
        {
            if (this.m_options.LogReconnection)
            {
                client.Logger?.Debug(this, TouchSocketResource.PollingEnd);
            }
        }
    }

    private async Task TryReconnectWithRetry(TClient client)
    {
        var attempts = 0;

        while (this.m_options.MaxRetryCount < 0 || attempts < this.m_options.MaxRetryCount)
        {
            if (client.DisposedValue)
            {
                return;
            }

            if (client.PauseReconnection)
            {
                if (this.m_options.LogReconnection)
                {
                    client.Logger?.Debug(this, TouchSocketResource.PauseReconnection);
                }
                await Task.Delay(this.m_options.BaseInterval, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                continue;
            }

            attempts++;

            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(this.m_cts.Token))
                {
                    cts.CancelAfter(this.m_options.ConnectTimeout);
                    await this.m_options.ConnectAction.Invoke(client, cts.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }

                this.m_hasGivenUp = false;
                this.m_options.OnSuccessed?.Invoke(client);

                if (this.m_options.LogReconnection)
                {
                    client.Logger?.Info(this, $"重连成功，尝试次数: {attempts}");
                }
                return;
            }
            catch (Exception ex)
            {
                this.m_options.OnFailed?.Invoke(client, attempts, ex);

                if (this.m_options.LogReconnection)
                {
                    client.Logger?.Warning(this, $"重连失败，尝试次数: {attempts}，错误: {ex.Message}");
                }

                if (this.m_options.MaxRetryCount > 0 && attempts >= this.m_options.MaxRetryCount)
                {
                    this.m_hasGivenUp = true;
                    this.m_options.OnGiveUp?.Invoke(client, attempts);
                    if (this.m_options.LogReconnection)
                    {
                        client.Logger?.Error(this, $"达到最大重连次数 {this.m_options.MaxRetryCount}，放弃重连");
                    }
                    return;
                }

                var nextInterval = this.CalculateNextInterval(attempts);
                await Task.Delay(nextInterval, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
    }

    private TimeSpan CalculateNextInterval(int attemptCount)
    {
        return this.m_options.Strategy switch
        {
            ReconnectionStrategy.Simple => this.m_options.BaseInterval,
            ReconnectionStrategy.ExponentialBackoff => TimeSpan.FromMilliseconds(Math.Min(
                this.m_options.BaseInterval.TotalMilliseconds * Math.Pow(this.m_options.BackoffMultiplier, attemptCount - 1),
                this.m_options.MaxInterval.TotalMilliseconds)),
            ReconnectionStrategy.LinearBackoff => TimeSpan.FromMilliseconds(Math.Min(
                this.m_options.BaseInterval.TotalMilliseconds + (attemptCount - 1) * this.m_options.BackoffMultiplier,
                this.m_options.MaxInterval.TotalMilliseconds)),
            _ => this.m_options.BaseInterval
        };
    }
}