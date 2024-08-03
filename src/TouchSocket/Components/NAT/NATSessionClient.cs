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
    /// 端口转发辅助
    /// </summary>
    public class NATSessionClient : TcpSessionClient
    {
        internal Func<NATSessionClient, ITcpClient, ClosedEventArgs, Task> m_internalDis;
        internal Func<NATSessionClient, ITcpClient, ReceivedDataEventArgs, Task<byte[]>> m_internalTargetClientRev;
        private readonly ConcurrentList<ITcpClient> m_targetClients = new ConcurrentList<ITcpClient>();

        /// <summary>
        /// 添加转发客户端。
        /// </summary>
        /// <param name="config">配置文件</param>
        /// <param name="setupAction">当完成配置，但是还未连接时回调。</param>
        /// <returns></returns>
        public async Task<ITcpClient> AddTargetClientAsync(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            var tcpClient = new TcpClient();
            tcpClient.Closed += this.TcpClient_Disconnected;
            tcpClient.Received += this.TcpClient_Received;
            await tcpClient.SetupAsync(config).ConfigureAwait(false);
            setupAction?.Invoke(tcpClient);
            await tcpClient.ConnectAsync().ConfigureAwait(false);

            this.m_targetClients.Add(tcpClient);
            return tcpClient;
        }

        /// <summary>
        /// 获取所有目标客户端
        /// </summary>
        /// <returns></returns>
        public ITcpClient[] GetTargetClients()
        {
            return this.m_targetClients.ToArray();
        }

        /// <summary>
        /// 发送数据到全部转发端。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public async Task SendToTargetClientAsync(Memory<byte> memory)
        {
            foreach (var socket in this.m_targetClients)
            {
                try
                {
                    await socket.SendAsync(memory).ConfigureAwait(false);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            foreach (var client in this.m_targetClients)
            {
                client.TryShutdown();
                client.SafeDispose();
            }
            await base.OnTcpClosed(e).ConfigureAwait(false);
        }

        private async Task TcpClient_Disconnected(ITcpSession client, ClosedEventArgs e)
        {
            foreach (var item in client.PluginManager.Plugins)
            {
                if (typeof(ReconnectionPlugin<>) == item.GetType().GetGenericTypeDefinition())
                {
                    await this.m_internalDis.Invoke(this, (ITcpClient)client, e).ConfigureAwait(false);
                    return;
                }
            }
            client.Dispose();
            this.m_targetClients.Remove((ITcpClient)client);
            await this.m_internalDis.Invoke(this, (ITcpClient)client, e).ConfigureAwait(false);
        }

        private async Task TcpClient_Received(ITcpClient client, ReceivedDataEventArgs e)
        {
            if (this.DisposedValue)
            {
                return;
            }

            try
            {
                var data = await this.m_internalTargetClientRev.Invoke(this, client, e).ConfigureAwait(false);
                if (data != null)
                {
                    if (this.Online)
                    {
                        await this.SendAsync(data).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
            }
        }
    }
}