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
using RRQMCore.ByteManager;

using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 协议服务器
    /// </summary>
    public class ProtocolService<TClient> : TokenService<TClient> where TClient : ProtocolSocketClient
    {
        private readonly Dictionary<short, string> usedProtocol;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtocolService()
        {
            this.usedProtocol = new Dictionary<short, string>();
        }

        #region 事件

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        public event RRQMStreamStatusEventHandler<TClient> StreamTransfered;

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        public event RRQMStreamOperationEventHandler<TClient> StreamTransfering;

        /// <summary>
        /// 处理协议数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<TClient> Received;

        #endregion 事件

        /// <summary>
        /// 判断协议是否能被使用
        /// </summary>
        /// <param name="protocol"></param>
        public void ProtocolCanUse(short protocol)
        {
            if (protocol > 0)
            {
                if (this.usedProtocol.ContainsKey(protocol))
                {
                    throw new ProtocolException($"该协议已被类协议使用，描述为：{this.usedProtocol[protocol]}");
                }
            }
            else
            {
                throw new ProtocolException($"用户协议必须大于0");
            }
        }

        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="RRQMException"></exception>
        public override void ResetID(WaitSetID waitSetID)
        {
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
        /// 添加已被使用的协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="describe"></param>
        protected void AddUsedProtocol(short protocol, string describe)
        {
            this.usedProtocol.Add(protocol, describe);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.streamTransfered = this.PrivateStreamTransfered;
            socketClient.streamTransfering = this.PrivateStreamTransfering;
            socketClient.onReceived = this.OnReceived;
            socketClient.protocolCanUse = this.ProtocolCanUse;
            base.OnConnecting(socketClient, e);
        }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfered(TClient client, StreamStatusEventArgs e)
        {
            this.StreamTransfered?.Invoke(client, e);
        }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfering(TClient client, StreamOperationEventArgs e)
        {
            this.StreamTransfering?.Invoke(client, e);
        }

        private void OnReceived(ProtocolSocketClient client, short protocol, ByteBlock byteBlock)
        {
            this.Received?.Invoke((TClient)client, protocol, byteBlock);
        }

        private void PrivateStreamTransfered(ProtocolSocketClient client, StreamStatusEventArgs e)
        {
            this.OnStreamTransfered((TClient)client, e);
        }

        private void PrivateStreamTransfering(ProtocolSocketClient client, StreamOperationEventArgs e)
        {
            this.OnStreamTransfering((TClient)client, e);
        }
    }

    /// <summary>
    /// 简单协议服务器
    /// </summary>
    public class ProtocolService : ProtocolService<ProtocolSocketClient>
    {

    }
}