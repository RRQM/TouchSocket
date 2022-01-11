//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// NAT服务器
    /// </summary>
    public class NATService : TcpService<NATSocketClient>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(ServiceConfig serviceConfig)
        {
            base.LoadConfig(serviceConfig);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed void OnConnecting(NATSocketClient socketClient, ClientOperationEventArgs e)
        {
            IPHost iPHost = this.ServiceConfig.GetValue<IPHost>(NATServiceConfig.TargetIPHostProperty);
            Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(iPHost.EndPoint);
            socketClient.BeginRunTargetSocket(socket);

            base.OnConnecting(socketClient, e);
        }
    }
}