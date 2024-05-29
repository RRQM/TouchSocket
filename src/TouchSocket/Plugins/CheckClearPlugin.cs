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
            DependencyProperty<bool>.Register("CheckClear", false);

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
        /// 清理统计类型。默认为：<see cref="CheckClearType.All"/>。当设置为<see cref="CheckClearType.OnlySend"/>时，
        /// 则只检验发送方向是否有数据流动。没有的话则会断开连接。
        /// </summary>
        /// <param name="clearType"></param>
        /// <returns></returns>
        public CheckClearPlugin<TClient> SetCheckClearType(CheckClearType clearType)
        {
            this.CheckClearType = clearType;
            return this;
        }

        /// <summary>
        /// 当因为超出时间限定而关闭。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public CheckClearPlugin<TClient> SetOnClose(Action<TClient, CheckClearType> action)
        {
            this.OnClose = action;
            return this;
        }

        /// <summary>
        /// 设置清理无数据交互的Client，默认60秒。
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
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
                        await Task.Delay(this.Tick).ConfigureFalseAwait();
                        first = false;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(this.Tick.TotalMilliseconds / 10.0)).ConfigureFalseAwait();
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
                        if (DateTime.Now - client.LastReceivedTime > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                    else if (this.CheckClearType == CheckClearType.OnlySend)
                    {
                        if (DateTime.Now - client.LastSentTime > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                    else
                    {
                        if (DateTime.Now - client.GetLastActiveTime() > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                }
            });
        }

        private Task OnLoadedConfig(IConfigObject sender, ConfigEventArgs e)
        {
            Task.Factory.StartNew(this.Polling,sender);
            return EasyTask.CompletedTask;
        }

        private async Task Polling(object sender)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(this.Tick.TotalMilliseconds / 10.0)).ConfigureFalseAwait();
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