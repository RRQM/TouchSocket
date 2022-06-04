//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using RRQMCore;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// TCP端口转发服务器
    /// </summary>
    public class NATService : TcpService<NATSocketClient>
    {
        private IPHost[] iPHosts;

        private NATMode mode;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(RRQMConfig serviceConfig)
        {
            this.iPHosts = this.Config.GetValue<IPHost[]>(RRQMConfigExtensions.TargetIPHostsProperty);
            if (this.iPHosts == null || this.iPHosts.Length == 0)
            {
                throw new RRQMException("目标地址未设置");
            }
            this.mode = this.Config.GetValue<NATMode>(RRQMConfigExtensions.NATModeProperty);
            if (this.mode == NATMode.OneWayToListen)
            {
                serviceConfig.ReceiveType = ReceiveType.None;
            }
            base.LoadConfig(serviceConfig);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected sealed override void OnConnecting(NATSocketClient socketClient, ClientOperationEventArgs e)
        {
            List<Socket> sockets = new List<Socket>();
            foreach (var iPHost in this.iPHosts)
            {
                try
                {
                    Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(iPHost.EndPoint);
                    sockets.Add(socket);
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(RRQMCore.Log.LogType.Error, this, ex.Message, ex);
                }
            }
            if (sockets.Count == 0)
            {
                this.Logger.Debug(RRQMCore.Log.LogType.Error, this, "转发地址均无法建立，已拒绝本次连接。", null);
                e.RemoveOperation(Operation.Permit);
                return;
            }

            socketClient.BeginRunTargetSocket(this.mode, sockets.ToArray());

            base.OnConnecting(socketClient, e);
        }
    }
}