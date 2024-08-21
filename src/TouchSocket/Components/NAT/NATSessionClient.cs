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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 端口转发辅助类，继承自TcpSessionClient。
    /// </summary>
    public class NatSessionClient : TcpSessionClient, INatSessionClient
    {
        // 定义一个处理断开连接事件的委托方法。
        internal Func<NatSessionClient, ITcpClient, ClosedEventArgs, Task> m_internalDis;

        // 定义一个处理接收数据事件的委托方法。
        internal Func<NatSessionClient, ITcpClient, ReceivedDataEventArgs, Task<byte[]>> m_internalTargetClientRev;

        // 保存所有目标客户端的列表。
        private readonly ConcurrentList<ITcpClient> m_targetClients = new ConcurrentList<ITcpClient>();

        /// <inheritdoc/>
        public async Task<ITcpClient> AddTargetClientAsync(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            // 创建一个新的TcpClient实例。
            var tcpClient = new TcpClient();
            // 注册断开连接事件处理程序。
            tcpClient.Closed += this.TcpClient_Disconnected;
            // 注册接收数据事件处理程序。
            tcpClient.Received += this.TcpClient_Received;
            // 异步设置TcpClient的配置。
            await tcpClient.SetupAsync(config).ConfigureAwait(false);
            // 执行回调操作，如果提供了的话。
            setupAction?.Invoke(tcpClient);
            // 异步连接TcpClient。
            await tcpClient.ConnectAsync().ConfigureAwait(false);

            // 将新的TcpClient添加到目标客户端列表中。
            this.m_targetClients.Add(tcpClient);
            return tcpClient;
        }

        /// <inheritdoc/>
        public ITcpClient[] GetTargetClients()
        {
            return this.m_targetClients.ToArray();
        }

        /// <inheritdoc/>
        public async Task SendToTargetClientAsync(ReadOnlyMemory<byte> memory)
        {
            // 遍历目标客户端列表，尝试发送数据。
            foreach (var socket in this.m_targetClients)
            {
                try
                {
                    await socket.SendAsync(memory).ConfigureAwait(false);
                }
                catch
                {
                    // 如果发送失败，捕获异常但不进行处理。
                }
            }
        }

        /// <summary>
        /// 当Tcp连接关闭时的事件处理程序。
        /// </summary>
        /// <param name="e">包含关闭信息的事件参数。</param>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            // 遍历目标客户端列表，关闭并释放资源。
            foreach (var client in this.m_targetClients)
            {
                client.TryShutdown();
                client.SafeDispose();
            }
            // 调用基类的事件处理程序。
            await base.OnTcpClosed(e).ConfigureAwait(false);
        }

        /// <summary>
        /// 处理TcpClient断开连接事件。
        /// </summary>
        /// <param name="client">断开连接的客户端。</param>
        /// <param name="e">包含断开连接信息的事件参数。</param>
        private async Task TcpClient_Disconnected(ITcpSession client, ClosedEventArgs e)
        {
            // 遍历客户端的插件，尝试调用断开连接的委托方法。
            foreach (var item in client.PluginManager.Plugins)
            {
                if (typeof(ReconnectionPlugin<>) == item.GetType().GetGenericTypeDefinition())
                {
                    await this.m_internalDis.Invoke(this, (ITcpClient)client, e).ConfigureAwait(false);
                    return;
                }
            }
            // 如果没有找到相应的插件，释放资源并从目标列表中移除客户端。
            client.Dispose();
            this.m_targetClients.Remove((ITcpClient)client);
            await this.m_internalDis.Invoke(this, (ITcpClient)client, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 处理接收数据事件。
        /// </summary>
        /// <param name="client">接收到数据的客户端。</param>
        /// <param name="e">包含接收数据信息的事件参数。</param>
        private async Task TcpClient_Received(ITcpClient client, ReceivedDataEventArgs e)
        {
            // 如果当前对象已被释放，则不处理接收到的数据。
            if (this.DisposedValue)
            {
                return;
            }

            try
            {
                // 调用接收数据的委托方法处理数据。
                var data = await this.m_internalTargetClientRev.Invoke(this, client, e).ConfigureAwait(false);
                if (data != null)
                {
                    // 如果当前对象在线，则发送数据。
                    if (this.Online)
                    {
                        await this.SendAsync(data).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                // 如果处理数据时发生异常，捕获异常但不进行处理。
            }
        }
    }
}