using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe.Plugins
{
    /// <summary>
    /// 重连插件
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = true)]
    public sealed class ReconnectionPlugin<NamedPipeClientBase> : PluginBase where NamedPipeClientBase : class, INamedPipeClientBase
    {
        /// <inheritdoc/>
        protected override void Loaded(IPluginsManager pluginsManager)
        {
            base.Loaded(pluginsManager);
            pluginsManager.Add<object, ConfigEventArgs>(nameof(ILoadedConfigPlugin.OnLoadedConfig), this.OnLoadedConfig);
            pluginsManager.Add<NamedPipeClientBase, DisconnectEventArgs>(nameof(INamedPipeDisconnectedPlugin.OnNamedPipeDisconnected), this.OnNamedPipeDisconnected);
        }

        private bool m_polling;

        /// <summary>
        /// 每个周期可执行的委托。用于检验客户端活性。返回true表示存活，返回
        /// </summary>
        public Func<NamedPipeClientBase, int, Task<bool?>> ActionForCheck { get; set; }

        /// <summary>
        /// ActionForConnect
        /// </summary>
        public Func<NamedPipeClientBase, Task<bool>> ActionForConnect { get; set; }

        /// <summary>
        /// 检验时间间隔
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(1);

        private Task OnLoadedConfig(object sender, ConfigEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!this.m_polling)
                {
                    return;
                }
                if (sender is NamedPipeClientBase client)
                {
                    var failCount = 0;
                    while (true)
                    {
                        if (this.DisposedValue)
                        {
                            return;
                        }
                        await Task.Delay(this.Tick);
                        try
                        {
                            var b = await this.ActionForCheck.Invoke(client, failCount);
                            if (b == null)
                            {
                                continue;
                            }
                            else if (b == false)
                            {
                                if (await this.ActionForConnect.Invoke(client))
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
            });

            return e.InvokeNext();
        }

        private Task OnNamedPipeDisconnected(NamedPipeClientBase client, DisconnectEventArgs e)
        {
            Task.Run(async () =>
            {
                if (e.Manual)
                {
                    return;
                }

                while (true)
                {
                    if (await this.ActionForConnect.Invoke(client))
                    {
                        return;
                    }
                }
            });

            return e.InvokeNext();
        }

        /// <summary>
        /// 每个周期可执行的委托。返回值为True标识客户端存活。返回False，表示失活，立即重连。返回null时，表示跳过此次检验。
        /// </summary>
        /// <param name="actionForCheck"></param>
        /// <returns></returns>
        public ReconnectionPlugin<NamedPipeClientBase> SetActionForCheck(Func<NamedPipeClientBase, int, Task<bool?>> actionForCheck)
        {
            this.ActionForCheck = actionForCheck;
            return this;
        }

        /// <summary>
        /// 设置连接动作
        /// </summary>
        /// <param name="tryConnect"></param>
        /// <returns>无论如何，只要返回True，则结束本轮尝试</returns>
        public ReconnectionPlugin<NamedPipeClientBase> SetConnectAction(Func<NamedPipeClientBase, Task<bool>> tryConnect)
        {
            this.ActionForConnect = tryConnect;
            return this;
        }

        /// <summary>
        /// 检验时间间隔
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public ReconnectionPlugin<NamedPipeClientBase> SetTick(TimeSpan tick)
        {
            this.Tick = tick;
            return this;
        }

        /// <summary>
        /// 使用轮询保持活性。
        /// </summary>
        public ReconnectionPlugin<NamedPipeClientBase> UsePolling()
        {
            this.ActionForCheck = (client, failCount) =>
            {
                return Task.FromResult(client?.Online);
            };
            this.m_polling = true;
            return this;
        }
    }
}