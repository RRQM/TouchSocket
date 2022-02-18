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
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;

namespace RRQMSocket
{
    /// <summary>
    /// 协议服务器
    /// </summary>
    public class ProtocolService<TClient> : TokenService<TClient> where TClient : ProtocolSocketClient, new()
    {
        private bool canResetID;

        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        public override void ResetID(WaitSetID waitSetID)
        {
            if (!this.canResetID)
            {
                throw new RRQMException("服务器不允许修改ID");
            }
            base.ResetID(waitSetID);
            if (this.TryGetSocketClient(waitSetID.NewID, out TClient client))
            {
                waitSetID.Status = 1;
                client.ChangeID(waitSetID);
            }
            else
            {
                throw new RRQMException("新ID不可用，请清理客户端重新修改ID");
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected override void LoadConfig(ServiceConfig serverConfig)
        {
            base.LoadConfig(serverConfig);
            this.canResetID = (bool)serverConfig.GetValue(ProtocolServiceConfig.CanResetIDProperty);
        }
    }

    /// <summary>
    /// 简单协议服务器
    /// </summary>
    public class ProtocolService : ProtocolService<SimpleProtocolSocketClient>
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<SimpleProtocolSocketClient> Received;

        /// <summary>
        /// 预处理流
        /// </summary>
        public event RRQMStreamOperationEventHandler<SimpleProtocolSocketClient> BeforeReceiveStream;

        /// <summary>
        /// 收到流数据
        /// </summary>
        public event RRQMStreamStatusEventHandler<SimpleProtocolSocketClient> ReceivedStream;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(SimpleProtocolSocketClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.Received += this.OnReceive;
            socketClient.BeforeReceiveStream += this.OnBeforeReceiveStream;
            socketClient.ReceivedStream += this.OnReceivedStream;
            base.OnConnecting(socketClient, e);
        }

        private void OnReceive(SimpleProtocolSocketClient socketClient, short procotol, ByteBlock byteBlock)
        {
            this.Received?.Invoke(socketClient, procotol, byteBlock);
        }

        private void OnBeforeReceiveStream(SimpleProtocolSocketClient client, StreamOperationEventArgs e)
        {
            this.BeforeReceiveStream?.Invoke(client, e);
        }

        private void OnReceivedStream(SimpleProtocolSocketClient client, StreamStatusEventArgs e)
        {
            this.ReceivedStream?.Invoke(client, e);
        }
    }
}