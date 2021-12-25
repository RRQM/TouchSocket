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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// 简单WebSocket实现。
    /// </summary>
    public class SimpleWSService : WSService<SimpleWSSocketClient>
    {
        /// <summary>
        /// 收到WebSocket数据
        /// </summary>
        public event WSDataFrameEventHandler<SimpleWSSocketClient> Received;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(SimpleWSSocketClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.Received += SocketClient_Received;
            base.OnConnecting(socketClient, e);
        }

        private void SocketClient_Received(SimpleWSSocketClient client, WSDataFrame dataFrame)
        {
            this.Received?.Invoke(client,dataFrame);
        }
    }
}
