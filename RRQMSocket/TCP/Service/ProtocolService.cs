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
    public class ProtocolService<TClient> : TokenService<TClient> where TClient : ProtocolSocketClient, new()
    {
        private bool canResetID;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtocolService()
        {
            this.usedProtocol = new Dictionary<short, string>();
        }

        private readonly Dictionary<short, string> usedProtocol;

        /// <summary>
        /// 已被使用的协议集合。
        /// </summary>
        public Dictionary<short, string> UsedProtocol
        {
            get { return this.usedProtocol; }
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
        /// 重置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="RRQMException"></exception>
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.protocolCanUse = this.ProtocolCanUse;
            base.OnConnecting(socketClient, e);
        }

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

        private void OnReceive(SimpleProtocolSocketClient socketClient, short protocol, ByteBlock byteBlock)
        {
            this.Received?.Invoke(socketClient, protocol, byteBlock);
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