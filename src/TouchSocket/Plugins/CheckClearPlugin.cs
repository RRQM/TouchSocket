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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// 检查清理连接插件。服务器与客户端均适用。
/// </summary>
[PluginOption(Singleton = true)]
public sealed class CheckClearPlugin<TClient> : PluginBase, ILoadedConfigPlugin where TClient : class, IDependencyClient, IClosableClient,IOnlineClient
{
    private static readonly DependencyProperty<bool> s_checkClearProperty =
        new("CheckClear", false);

    private readonly ILog m_logger;

    /// <summary>
    /// 检查清理连接插件。服务器与客户端均适用。
    /// </summary>
    public CheckClearPlugin(ILog logger)
    {
        this.OnClose = async (client, type) =>
        {
            switch (this.CheckClearType)
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
        };
        this.m_logger = logger;
    }

    /// <summary>
    /// 清理统计类型。默认为：<see cref="CheckClearType.All"/>。当设置为<see cref="CheckClearType.OnlySend"/>时，
    /// 则只检验发送方向是否有数据流动。没有的话则会断开连接。
    /// </summary>
    public CheckClearType CheckClearType { get; set; } = CheckClearType.All;

    /// <summary>
    /// 当因为超出时间限定而关闭。
    /// </summary>
    public Func<TClient, CheckClearType, Task> OnClose { get; set; }

    /// <summary>
    /// 获取或设置清理无数据交互的Client，默认60秒。
    /// </summary>
    public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(60);

    /// <inheritdoc/>
    public async Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
    {
        _ = EasyTask.SafeRun(this.Polling, sender);
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 设置清理统计类型。此方法允许指定在何种情况下应清理统计信息。
    /// 默认情况下，清理类型设置为<see cref="CheckClearType.All"/>，表示所有情况都进行清理。
    /// 如果设置为<see cref="CheckClearType.OnlySend"/>，则仅检验发送方向是否有数据流动，
    /// 若没有数据流动，则断开连接。
    /// </summary>
    /// <param name="clearType">要设置的清理统计类型。</param>
    /// <returns>返回当前<see cref="CheckClearPlugin{TClient}"/>实例，以支持链式调用。</returns>
    public CheckClearPlugin<TClient> SetCheckClearType(CheckClearType clearType)
    {
        this.CheckClearType = clearType;
        return this;
    }

    /// <summary>
    /// 设置在超出时间限定而关闭时的回调操作。
    /// </summary>
    /// <param name="action">一个Action委托，包含客户端对象和检查清除类型作为参数，在关闭操作执行时会被调用。</param>
    /// <returns>返回当前的CheckClearPlugin实例，以支持链式调用。</returns>
    public CheckClearPlugin<TClient> SetOnClose(Action<TClient, CheckClearType> action)
    {
        Task Func(TClient client, CheckClearType checkClearType)
        {
            action.Invoke(client, checkClearType);
            return EasyTask.CompletedTask;
        }
        this.SetOnClose(Func);
        return this;
    }

    /// <summary>
    /// 设置在超出时间限定而关闭时的回调操作。
    /// </summary>
    /// <param name="func">一个Func委托，包含客户端对象和检查清除类型作为参数，在关闭操作执行时会被调用。</param>
    /// <returns>返回当前的CheckClearPlugin实例，以支持链式调用。</returns>
    public CheckClearPlugin<TClient> SetOnClose(Func<TClient, CheckClearType, Task> func)
    {
        this.OnClose = func;
        return this;
    }

    /// <summary>
    /// 设置清理无数据交互的Client，默认60秒。
    /// </summary>
    /// <param name="timeSpan">清理无数据交互的Client的时间间隔</param>
    /// <returns>返回配置后的实例，支持链式调用</returns>
    public CheckClearPlugin<TClient> SetTick(TimeSpan timeSpan)
    {
        this.Tick = timeSpan;
        return this;
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
                    await Task.Delay(this.Tick).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    first = false;
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(this.Tick.TotalMilliseconds / 10.0)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }

                if (client is IOnlineClient onlineClient && !onlineClient.Online)
                {
                    return;
                }

                if (this.CheckClearType == CheckClearType.OnlyReceive)
                {
                    if (DateTimeOffset.UtcNow - client.LastReceivedTime > this.Tick)
                    {
                        await this.CloseClientAsync(client, this.CheckClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }
                }
                else if (this.CheckClearType == CheckClearType.OnlySend)
                {
                    if (DateTimeOffset.UtcNow - client.LastSentTime > this.Tick)
                    {
                        await this.CloseClientAsync(client, this.CheckClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }
                }
                else
                {
                    if (DateTimeOffset.UtcNow - client.GetLastActiveTime() > this.Tick)
                    {
                        await this.CloseClientAsync(client, this.CheckClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }
                }
            }
        });
    }

    private async Task CheckWithClient(TClient client)
    {
        if (!client.Online)
        {
            return;
        }

        if (this.CheckClearType == CheckClearType.OnlyReceive)
        {
            if (DateTimeOffset.UtcNow - client.LastReceivedTime > this.Tick)
            {
                await this.CloseClientAsync(client, this.CheckClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
        }
        else if (this.CheckClearType == CheckClearType.OnlySend)
        {
            if (DateTimeOffset.UtcNow - client.LastSentTime > this.Tick)
            {
                await this.CloseClientAsync(client, this.CheckClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
        }
        else
        {
            if (DateTimeOffset.UtcNow - client.GetLastActiveTime() > this.Tick)
            {
                await this.CloseClientAsync(client, this.CheckClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
        }
    }

    private async Task CloseClientAsync(TClient client, CheckClearType checkClearType)
    {
        if (this.OnClose != null)
        {
            try
            {
                await this.OnClose.Invoke(client, checkClearType).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch(Exception ex)
            {
                this.m_logger.Debug(this, ex.Message);
            }
        }
    }

    private async Task Polling(IConfigObject sender)
    {
        try
        {
            this.m_logger.Debug(this,$"{this.GetType()} begin polling");
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
                        await Task.Delay(TimeSpan.FromMilliseconds(this.Tick.TotalMilliseconds / 10.0)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        foreach (var client in connectableService.GetClients())
                        {
                            this.CheckWithSessionClient(client as TClient);
                        }
                    }
                    else if (sender is TClient client)
                    {
                        await Task.Delay(this.Tick).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        await this.CheckWithClient(client).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.m_logger.Debug(this,ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            this.m_logger.Debug(this,ex.Message);
        }
        finally
        {
            this.m_logger.Debug(this,$"{this.GetType()} end polling");
        }
    }
}