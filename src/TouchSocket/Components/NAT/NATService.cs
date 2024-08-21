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

using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp端口转发服务器
    /// </summary>
    public class NatService : TcpService<NatSessionClient>
    {
        /// <inheritdoc/>
        protected override void ClientInitialized(NatSessionClient client)
        {
            base.ClientInitialized(client);
            // 设置当目标客户端断开时的处理方法
            client.m_internalDis = this.OnTargetClientClosed;
            // 设置当目标客户端接收到数据时的处理方法
            client.m_internalTargetClientRev = this.OnTargetClientReceived;
        }

        /// <inheritdoc/>
        protected override NatSessionClient NewClient()
        {
            return new NatSessionClient();
        }

        /// <summary>
        /// 在NAT服务器收到数据时。
        /// </summary>
        /// <param name="socketClient">NAT会话客户端</param>
        /// <param name="e">接收到的数据事件参数</param>
        /// <returns>需要转发的数据。</returns>
        protected virtual Task<byte[]> OnNatReceived(NatSessionClient socketClient, ReceivedDataEventArgs e)
        {
            return Task.FromResult(e.ByteBlock?.ToArray());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient">Nat会话客户端</param>
        /// <param name="e">接收到的数据事件参数</param>
        protected sealed override async Task OnTcpReceived(NatSessionClient socketClient, ReceivedDataEventArgs e)
        {
            // 从NAT服务器接收到数据后的处理
            var data = await this.OnNatReceived(socketClient, e).ConfigureAwait(false);
            if (data != null)
            {
                // 转发数据到目标客户端
                await socketClient.SendToTargetClientAsync(new System.Memory<byte>(data, 0, data.Length))
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 当目标客户端断开。
        /// </summary>
        /// <param name="socketClient">Nat会话客户端</param>
        /// <param name="tcpClient">断开的Tcp客户端</param>
        /// <param name="e">断开事件参数</param>
        /// <returns></returns>
        protected virtual Task OnTargetClientClosed(NatSessionClient socketClient, ITcpClient tcpClient,
            ClosedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 在目标客户端收到数据时。
        /// </summary>
        /// <param name="socketClient">Nat会话客户端</param>
        /// <param name="tcpClient">发送数据的Tcp客户端</param>
        /// <param name="e">接收到的数据事件参数</param>
        /// <returns></returns>
        protected virtual Task<byte[]> OnTargetClientReceived(NatSessionClient socketClient, ITcpClient tcpClient,
            ReceivedDataEventArgs e)
        {
            return Task.FromResult(e.ByteBlock?.ToArray());
        }
    }
}