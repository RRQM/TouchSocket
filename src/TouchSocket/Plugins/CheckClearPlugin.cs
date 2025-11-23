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
/// 检查清理连接插件。服务器与客户端均适用。
/// </summary>
[PluginOption(Singleton = true)]
public sealed class CheckClearPlugin<TClient> : PluginBase, ILoadedConfigPlugin
    where TClient : class, IDependencyClient, IClosableClient
{
    private static readonly DependencyProperty<bool> s_checkClearProperty =
        new("CheckClear", false);

    private readonly ILog m_logger;
    private readonly CheckClearType m_checkClearType;
    private readonly TimeSpan m_tick;
    private readonly Func<TClient, CheckClearType, Task> m_onClose;

    /// <summary>
    /// 检查清理连接插件。服务器与客户端均适用。
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="options">配置选项</param>
    public CheckClearPlugin(ILog logger, CheckClearOption<TClient> options)
    {
        ThrowHelper.ThrowIfNull(logger, nameof(logger));
        ThrowHelper.ThrowIfNull(options, nameof(options));

        this.m_logger = logger;
        this.m_checkClearType = options.CheckClearType;
        this.m_tick = options.Tick;

        if (options.OnClose != null)
        {
            this.m_onClose = options.OnClose;
        }
        else
        {
            this.m_onClose = this.DefaultOnClose;
        }
    }

    private async Task DefaultOnClose(TClient client, CheckClearType type)
    {
        switch (type)
        {
            case CheckClearType.OnlyReceive:
                await client.CloseAsync(TouchSocketResource.TimedoutWithoutReceiving).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;
            case CheckClearType.OnlySend:
                await client.CloseAsync(TouchSocketResource.TimedoutWithoutSending).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;
            case CheckClearType.All:
            default:
                await client.CloseAsync(TouchSocketResource.TimedoutWithoutAll).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;
        }
    }

    /// <summary>
    /// 清理统计类型。
    /// </summary>
    public CheckClearType CheckClearType => this.m_checkClearType;

    /// <summary>
    /// 清理无数据交互的Client。
    /// </summary>
    public TimeSpan Tick => this.m_tick;

    /// <inheritdoc/>
    public async Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
    {
        _ = EasyTask.SafeRun(this.Polling, sender);
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private void CheckWithSessionClient(TClient client)
    {
        if (client is null)
        {
            return;
        }
        if (client.GetValue(s_checkClearProperty))
        {
            return;
        }

        client.SetValue(s_checkClearProperty, true);

        _ = EasyTask.SafeRun(async () =>
        {
            var first = true;
            while (true)
            {
                if (first)
                {
                    await Task.Delay(this.m_tick).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    first = false;
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(this.m_tick.TotalMilliseconds / 10.0)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }

                if (client is IOnlineClient onlineClient && !onlineClient.Online)
                {
                    return;
                }

                if (this.m_checkClearType == CheckClearType.OnlyReceive)
                {
                    if (DateTimeOffset.UtcNow - client.LastReceivedTime > this.m_tick)
                    {
                        await this.CloseClientAsync(client, this.m_checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }
                }
                else if (this.m_checkClearType == CheckClearType.OnlySend)
                {
                    if (DateTimeOffset.UtcNow - client.LastSentTime > this.m_tick)
                    {
                        await this.CloseClientAsync(client, this.m_checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }
                }
                else
                {
                    if (DateTimeOffset.UtcNow - client.GetLastActiveTime() > this.m_tick)
                    {
                        await this.CloseClientAsync(client, this.m_checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }
                }
            }
        });
    }

    private async Task CheckWithClient(TClient client)
    {
        if (client is IOnlineClient onlineClient && !onlineClient.Online)
        {
            return;
        }

        if (this.m_checkClearType == CheckClearType.OnlyReceive)
        {
            if (DateTimeOffset.UtcNow - client.LastReceivedTime > this.m_tick)
            {
                await this.CloseClientAsync(client, this.m_checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
        }
        else if (this.m_checkClearType == CheckClearType.OnlySend)
        {
            if (DateTimeOffset.UtcNow - client.LastSentTime > this.m_tick)
            {
                await this.CloseClientAsync(client, this.m_checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
        }
        else
        {
            if (DateTimeOffset.UtcNow - client.GetLastActiveTime() > this.m_tick)
            {
                await this.CloseClientAsync(client, this.m_checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
        }
    }

    private async Task CloseClientAsync(TClient client, CheckClearType checkClearType)
    {
        if (this.m_onClose != null)
        {
            try
            {
                await this.m_onClose.Invoke(client, checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (Exception ex)
            {
                this.m_logger.Debug(this, ex.Message);
            }
        }
    }

    private async Task Polling(IConfigObject sender)
    {
        try
        {
            this.m_logger.Debug(this, $"{this.GetType()} begin polling");
            while (true)
            {
                if (this.DisposedValue)
                {
                    return;
                }
                try
                {

                    if (sender is IConnectableService connectableService)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(this.m_tick.TotalMilliseconds / 10.0)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        foreach (var client in connectableService.GetClients())
                        {
                            this.CheckWithSessionClient(client as TClient);
                        }
                    }
                    else if (sender is TClient client)
                    {
                        await Task.Delay(this.m_tick).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        await this.CheckWithClient(client).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.m_logger.Debug(this, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            this.m_logger.Debug(this, ex.Message);
        }
        finally
        {
            this.m_logger.Debug(this, $"{this.GetType()} end polling");
        }
    }
}