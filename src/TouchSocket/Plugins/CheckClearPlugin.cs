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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 检查清理连接插件。服务器与客户端均适用。
    /// </summary>
    [PluginOption(Singleton = true)]
    public sealed class CheckClearPlugin<TClient> : PluginBase where TClient : IClient, IClosableClient
    {
        private static readonly DependencyProperty<bool> s_checkClearProperty =
            new("CheckClear", false);

        private readonly ILog m_logger;

        /// <summary>
        /// 检查清理连接插件。服务器与客户端均适用。
        /// </summary>
        public CheckClearPlugin(ILog logger)
        {
            this.OnClose = (client, type) =>
            {
                if (this.CheckClearType == CheckClearType.OnlyReceive)
                {
                    client.CloseAsync(TouchSocketResource.TimedoutWithoutReceiving);
                }
                else if (this.CheckClearType == CheckClearType.OnlySend)
                {
                    client.CloseAsync(TouchSocketResource.TimedoutWithoutSending);
                }
                else
                {
                    client.CloseAsync(TouchSocketResource.TimedoutWithoutAll);
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
        public Action<TClient, CheckClearType> OnClose { get; set; }

        /// <summary>
        /// 获取或设置清理无数据交互的Client，默认60秒。
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(60);

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
            this.OnClose = action;
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

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            pluginManager.Add<IConfigObject, ConfigEventArgs>(typeof(ILoadedConfigPlugin), this.OnLoadedConfig);
            base.Loaded(pluginManager);
        }

        private void CheckClient(TClient client)
        {
            if (client.GetValue(s_checkClearProperty))
            {
                return;
            }

            client.SetValue(s_checkClearProperty, true);

            _ = Task.Run(async () =>
            {
                var first = true;
                while (true)
                {
                    if (first)
                    {
                        await Task.Delay(this.Tick).ConfigureAwait(false);
                        first = false;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(this.Tick.TotalMilliseconds / 10.0)).ConfigureAwait(false);
                    }

                    if (client is IOnlineClient onlineClient && !onlineClient.Online)
                    {
                        return;
                    }
                    //else if (client is IOnlineClient handshakeObject && !handshakeObject.Online)
                    //{
                    //    return;
                    //}

                    if (this.CheckClearType == CheckClearType.OnlyReceive)
                    {
                        if (DateTime.UtcNow - client.LastReceivedTime > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                    else if (this.CheckClearType == CheckClearType.OnlySend)
                    {
                        if (DateTime.UtcNow - client.LastSentTime > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                    else
                    {
                        if (DateTime.UtcNow - client.GetLastActiveTime() > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                }
            });
        }

        private Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
        {
            Task.Factory.StartNew(this.Polling, sender);
            return EasyTask.CompletedTask;
        }

        private async Task Polling(object sender)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(this.Tick.TotalMilliseconds / 10.0)).ConfigureAwait(false);
                    if (sender is IConnectableService connectableService)
                    {
                        foreach (var client in connectableService.GetClients())
                        {
                            this.CheckClient((TClient)client);
                        }
                    }
                    else if (sender is IClient client)
                    {
                        this.CheckClient((TClient)client);
                    }
                }
                catch (Exception ex)
                {
                    this.m_logger.Exception(ex);
                }
            }
        }
    }
}