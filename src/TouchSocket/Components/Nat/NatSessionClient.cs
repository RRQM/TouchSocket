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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 端口转发辅助类，继承自<see cref="TcpSessionClient"/>。
    /// </summary>
    public abstract class NatSessionClient : TcpSessionClientBase, INatSessionClient
    {
        // 保存所有目标客户端的列表。
        private readonly ConcurrentList<NatTargetClient> m_targetClients = new ConcurrentList<NatTargetClient>();

        /// <inheritdoc/>
        public async Task AddTargetClientAsync(NatTargetClient client)
        {
            if (this.m_targetClients.Contains(client))
            {
                return;
            }
            //此处的逻辑是以防重复订阅
            //注册断开连接事件处理程序。
            client.m_internalClosed -= this.NatTargetClient_Closed;
            client.m_internalClosed += this.NatTargetClient_Closed;

            // 注册接收数据事件处理程序。
            client.m_internalReceived -= this.NatTargetClient_Received;
            client.m_internalReceived += this.NatTargetClient_Received;

            if (!client.Online)
            {
                await client.ConnectAsync().ConfigureAwait(false);
            }

            // 将新的TcpClient添加到目标客户端列表中。
            this.m_targetClients.Add(client);
        }

        /// <inheritdoc/>
        public async Task AddTargetClientAsync(Action<TouchSocketConfig> setupAction)
        {
            // 创建一个新的TcpClient实例。
            var client = new NatTargetClient();
            var config = new TouchSocketConfig();
            setupAction.Invoke(config);

            await client.SetupAsync(config).ConfigureAwait(false);
            await this.AddTargetClientAsync(client).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IEnumerable<NatTargetClient> GetTargetClients()
        {
            return this.m_targetClients;
        }

        /// <inheritdoc/>
        public bool RemoveTargetClient(NatTargetClient client)
        {
            if (this.m_targetClients.Remove(client))
            {
                if (!client.StandBy)
                {
                    client.TryShutdown();
                    client.SafeDispose();
                }

                client.m_internalClosed -= this.NatTargetClient_Closed;
                client.m_internalReceived -= this.NatTargetClient_Received;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 当NAT连接关闭时的虚拟方法。
        /// </summary>
        /// <param name="e">关闭事件的参数。</param>
        /// <returns>一个等待完成的任务。</returns>
        protected virtual Task OnNatClosed(ClosedEventArgs e)
        {
            // 遍历目标客户端列表，关闭并释放资源。
            foreach (var client in this.m_targetClients)
            {
                // 移除并处理目标客户端。
                this.RemoveTargetClient(client);
            }

            // 返回已完成的任务，表示处理完毕。
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当建立NAT连接时触发的抽象异步事件处理方法。
        /// </summary>
        /// <param name="e">包含连接信息的事件参数。</param>
        /// <returns>一个异步任务，表示事件处理的完成。</returns>
        protected abstract Task OnNatConnected(ConnectedEventArgs e);

        /// <summary>
        /// 在Nat服务器收到数据时。
        /// </summary>
        /// <param name="e">接收到的数据事件参数</param>
        /// <returns>需要转发的数据。</returns>
        protected virtual async Task OnNatReceived(ReceivedDataEventArgs e)
        {
            foreach (var client in this.GetTargetClients())
            {
                if (!client.Online)
                {
                    continue;
                }

                if (e.ByteBlock != null)
                {
                    // 转发数据到目标客户端

                    try
                    {
                        await client.SendAsync(e.ByteBlock.Memory).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Exception(ex);
                    }
                }

                if (e.RequestInfo != null)
                {
                    // 转发数据到目标客户端
                    try
                    {
                        await client.SendAsync(e.RequestInfo).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Exception(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 当目标客户端断开。
        /// </summary>
        /// <param name="client">断开的Tcp客户端</param>
        /// <param name="e">断开事件参数</param>
        /// <returns></returns>
        protected abstract Task OnTargetClientClosed(NatTargetClient client, ClosedEventArgs e);

        /// <summary>
        /// 在目标客户端收到数据时。
        /// </summary>
        /// <param name="client">发送数据的Tcp客户端</param>
        /// <param name="e">接收到的数据事件参数</param>
        /// <returns></returns>
        protected virtual async Task OnTargetClientReceived(NatTargetClient client, ReceivedDataEventArgs e)
        {
            if (!this.Online)
            {
                return;
            }

            if (e.ByteBlock != null)
            {
                // 转发数据到当前客户端

                try
                {
                    await this.ProtectedSendAsync(e.ByteBlock.Memory).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.Logger?.Exception(ex);
                }
            }

            if (e.RequestInfo != null)
            {
                // 转发数据到当前客户端
                try
                {
                    await this.ProtectedSendAsync(e.RequestInfo).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.Logger?.Exception(ex);
                }
            }
        }

        /// <inheritdoc/>
        protected override sealed async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.OnNatClosed(e).ConfigureAwait(false);

            // 调用基类的事件处理程序。
            await base.OnTcpClosed(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpConnected(ConnectedEventArgs e)
        {
            await base.OnTcpConnected(e).ConfigureAwait(false);
            await this.OnNatConnected(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override sealed async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            await base.OnTcpReceived(e).ConfigureAwait(false);
            await this.OnNatReceived(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 处理TcpClient断开连接事件。
        /// </summary>
        /// <param name="client">断开连接的客户端。</param>
        /// <param name="e">包含断开连接信息的事件参数。</param>
        private async Task NatTargetClient_Closed(ITcpSession client, ClosedEventArgs e)
        {
            await this.OnTargetClientClosed((NatTargetClient)client, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 处理接收数据事件。
        /// </summary>
        /// <param name="client">接收到数据的客户端。</param>
        /// <param name="e">包含接收数据信息的事件参数。</param>
        private async Task NatTargetClient_Received(NatTargetClient client, ReceivedDataEventArgs e)
        {
            // 如果当前对象已被释放，则不处理接收到的数据。
            if (this.DisposedValue || !this.Online)
            {
                return;
            }

            // 调用接收数据的委托方法处理数据。
            await this.OnTargetClientReceived(client, e).ConfigureAwait(false);
        }
    }
}