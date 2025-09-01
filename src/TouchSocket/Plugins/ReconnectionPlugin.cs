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
using System.Net.Sockets;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// 重连插件
/// </summary>
public abstract class ReconnectionPlugin<TClient> : PluginBase, ILoadedConfigPlugin where TClient : IDisposableObject, IConnectableClient, IOnlineClient, ILoggerObject
{
    private bool m_polling;
    private TimeSpan m_tick = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 重连插件
    /// </summary>
    protected ReconnectionPlugin()
    {
        this.ActionForConnect = async (c) =>
        {
            try
            {
                await c.ConnectAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return true;
            }
            catch
            {
                return false;
            }
        };
    }

    /// <summary>
    /// 每个周期可执行的委托。用于检验客户端活性。
    /// <list type="bullet">
    /// <item>返回<see langword="true"/>表示存活</item>
    /// <item>返回<see langword="false"/>表示失活，需要马上重连</item>
    /// <item>当返回<see langword="null"/>时，表示跳过此次检查</item>
    /// </list>
    /// </summary>
    public abstract Func<TClient, int, Task<bool?>> ActionForCheck { get; set; }

    public Func<TClient, Task<bool>> ActionForConnect { get; set; }

    /// <summary>
    /// 检验时间间隔
    /// </summary>
    public TimeSpan Tick => this.m_tick;

    /// <inheritdoc/>
    public async Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
    {
        if (sender is TClient client && this.m_polling)
        {
            _ = EasyTask.SafeRun(this.BeginReconnect, client);
        }

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 设置一个周期性执行的委托，用于检查客户端状态。
    /// </summary>
    /// <param name="actionForCheck">一个委托，接受客户端实例和周期次数作为参数，返回一个任务，该任务结果为布尔值。</param>
    /// <returns>返回当前ReconnectionPlugin实例，以便链式调用。</returns>
    public ReconnectionPlugin<TClient> SetActionForCheck(Func<TClient, int, Task<bool?>> actionForCheck)
    {
        this.ActionForCheck = actionForCheck;
        return this;
    }

    /// <summary>
    /// 设置连接动作
    /// </summary>
    /// <param name="tryConnect">一个异步方法，尝试建立连接，并返回一个布尔值指示连接是否成功</param>
    /// <returns>返回当前实例，以便支持链式调用</returns>
    public ReconnectionPlugin<TClient> SetConnectAction(Func<TClient, Task<bool>> tryConnect)
    {
        // 将提供的尝试连接方法赋值给内部变量，以供后续连接使用
        this.ActionForConnect = tryConnect;
        // 返回当前实例，支持链式调用
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
        this.SetConnectAction(async client =>
        {
            var tryT = 0;
            while (true)
            {
                try
                {
                    if (client.Online)
                    {
                        return true;
                    }
                    else
                    {
                        await Task.Delay(1000).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        await client.ConnectAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }

                    successCallback?.Invoke(client);
                    return true;
                }
                catch (Exception ex)
                {
                    await Task.Delay(sleepTime).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (failCallback?.Invoke(client, ++tryT, ex) != true)
                    {
                        return true;
                    }
                }
            }
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
        this.SetConnectAction(async client =>
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
                        await Task.Delay(1000).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        await client.ConnectAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                    await Task.Delay(sleepTime).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            return true;
        });
        return this;
    }

    /// <summary>
    /// 使用轮询保持活性。
    /// </summary>
    /// <param name="tick">轮询的时间间隔。</param>
    /// <returns>返回当前的ReconnectionPlugin实例，用于链式调用。</returns>
    public ReconnectionPlugin<TClient> UsePolling(TimeSpan tick)
    {
        // 设置轮询时间间隔
        this.m_tick = tick;
        // 标记为使用轮询方式保持活性
        this.m_polling = true;
        // 返回当前实例，支持链式调用
        return this;
    }

    private async Task BeginReconnect(TClient client)
    {
        client.Logger?.Debug(this, TouchSocketResource.PollingBegin.Format(this.Tick));

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
                await Task.Delay(this.Tick).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                try
                {
                    var b = await this.ActionForCheck.Invoke(client, failCount).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (b == null)
                    {
                        continue;
                    }
                    else if (b == false)
                    {
                        if (await this.ActionForConnect.Invoke(client).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                        {
                            failCount = 0;
                        }
                    }
                    else
                    {
                        failCount = 0;
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

    protected async Task ExecuteConnectLoop(TClient client)
    {
        while (true)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (await this.ActionForConnect.Invoke(client).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                return;
            }
        }
    }
}