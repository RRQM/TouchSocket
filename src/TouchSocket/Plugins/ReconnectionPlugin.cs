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
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 重连插件
    /// </summary>
    public abstract class ReconnectionPlugin<TClient> : PluginBase where TClient : IConnectableClient,IOnlineClient,ILoggerObject
    {
        private bool m_polling;
        private TimeSpan m_tick = TimeSpan.FromSeconds(1);
        private Task m_beginReconnectTask;

        /// <summary>
        /// 重连插件
        /// </summary>
        public ReconnectionPlugin()
        {
            this.ActionForConnect = async (c) =>
            {
                try
                {
                    await c.ConnectAsync().ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            };
        }

        /// <summary>
        /// 每个周期可执行的委托。用于检验客户端活性。返回true表示存活，返回
        /// </summary>
        public abstract Func<TClient, int, Task<bool?>> ActionForCheck { get; set; }

        /// <summary>
        /// ActionForConnect
        /// </summary>
        public Func<TClient, Task<bool>> ActionForConnect { get; set; }

        /// <summary>
        /// 检验时间间隔
        /// </summary>
        public TimeSpan Tick => this.m_tick;

        /// <summary>
        /// 每个周期可执行的委托。返回值为True标识客户端存活。返回False，表示失活，立即重连。返回null时，表示跳过此次检验。
        /// </summary>
        /// <param name="actionForCheck"></param>
        /// <returns></returns>
        public ReconnectionPlugin<TClient> SetActionForCheck(Func<TClient, int, Task<bool?>> actionForCheck)
        {
            this.ActionForCheck = actionForCheck;
            return this;
        }

        /// <summary>
        /// 每个周期可执行的委托。返回值为True标识客户端存活。返回False，表示失活，立即重连。返回null时，表示跳过此次检验。
        /// </summary>
        /// <param name="actionForCheck"></param>
        /// <returns></returns>
        public ReconnectionPlugin<TClient> SetActionForCheck(Func<TClient, int, bool?> actionForCheck)
        {
            this.ActionForCheck = async (c, i) =>
            {
                await EasyTask.CompletedTask.ConfigureAwait(false);
                return actionForCheck.Invoke(c, i);
            };
            return this;
        }

        /// <summary>
        /// 设置连接动作
        /// </summary>
        /// <param name="tryConnect"></param>
        /// <returns>无论如何，只要返回True，则结束本轮尝试</returns>
        public ReconnectionPlugin<TClient> SetConnectAction(Func<TClient, Task<bool>> tryConnect)
        {
            this.ActionForConnect = tryConnect;
            return this;
        }

        /// <summary>
        /// 设置连接动作
        /// </summary>
        /// <param name="sleepTime">失败时间隔时间</param>
        /// <param name="failCallback">失败时回调（参数依次为：客户端，本轮尝试重连次数，异常信息）。如果回调为null或者返回false，则终止尝试下次连接。</param>
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
                            await Task.Delay(1000).ConfigureAwait(false);
                            await client.ConnectAsync().ConfigureAwait(false);
                        }

                        successCallback?.Invoke(client);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(sleepTime).ConfigureAwait(false);
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
                            await Task.Delay(1000).ConfigureAwait(false);
                            await client.ConnectAsync().ConfigureAwait(false);
                        }
                        successCallback?.Invoke(client);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (printLog)
                        {
                            client.Logger?.Exception(ex);
                        }
                        await Task.Delay(sleepTime).ConfigureAwait(false);
                    }
                }
                return true;
            });
            return this;
        }

        /// <summary>
        /// 设置连接动作
        /// </summary>
        /// <param name="tryConnect"></param>
        /// <returns>无论如何，只要返回True，则结束本轮尝试</returns>
        public ReconnectionPlugin<TClient> SetConnectAction(Func<TClient, bool> tryConnect)
        {
            this.ActionForConnect = (c) => Task.FromResult(tryConnect.Invoke(c));
            return this;
        }

        /// <summary>
        /// 使用轮询保持活性。
        /// </summary>
        public ReconnectionPlugin<TClient> UsePolling(TimeSpan tick)
        {
            this.m_tick = tick;
            this.m_polling = true;
            return this;
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            pluginManager.Add<IConfigObject, ConfigEventArgs>(typeof(ILoadedConfigPlugin), this.OnLoadedConfig);
        }

        private async Task BeginReconnect(object sender)
        {
            if (!this.m_polling)
            {
                return;
            }

            if (sender is not TClient client)
            {
                return;
            }

            var failCount = 0;

            while (true)
            {
                if (this.DisposedValue)
                {
                    return;
                }
                await Task.Delay(this.Tick).ConfigureAwait(false);
                try
                {
                    var b = await this.ActionForCheck.Invoke(client, failCount).ConfigureAwait(false);
                    if (b == null)
                    {
                        continue;
                    }
                    else if (b == false)
                    {
                        if (await this.ActionForConnect.Invoke(client).ConfigureAwait(false))
                        {
                            failCount = 0;
                        }
                    }
                    else
                    {
                        failCount = 0;
                    }
                }
                catch
                {
                }
            }
        }

        private async Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
        {
            this.m_beginReconnectTask= Task.Factory.StartNew(this.BeginReconnect, sender, TaskCreationOptions.LongRunning);

            await e.InvokeNext();
        }


        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_beginReconnectTask.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}