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
using RRQMCore.Extensions;
using RRQMCore.Run;
using System;
using System.Threading;

namespace RRQMSocket
{

    /// <summary>
    /// 需要Token验证的TCP客户端
    /// </summary>
    public class TokenClient : TokenClientBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public event RRQMReceivedEventHandler<TokenClient> Received;

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected sealed override void HandleTokenReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(this, byteBlock, requestInfo);
            base.HandleTokenReceivedData(byteBlock, requestInfo);
        }
    }

    /// <summary>
    /// 需要Token验证的TCP客户端基类
    /// </summary>
    public class TokenClientBase : TcpClientBase, ITokenClient
    {
        private WaitHandlePool<IWaitResult> waitHandlePool;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenClientBase()
        {
            this.waitHandlePool = new WaitHandlePool<IWaitResult>();
            this.Protocol = Protocol.RRQMToken;
        }

        /// <summary>
        /// 等待返回池
        /// </summary>
        public WaitHandlePool<IWaitResult> WaitHandlePool => this.waitHandlePool;

        private string id;

        /// <summary>
        /// 获取服务器分配的ID
        /// </summary>
        public string ID => this.id;


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsHandshaked => this.isHandshaked;

        /// <summary>
        /// 重新设置ID,但是不会同步到服务器
        /// </summary>
        /// <param name="newID"></param>
        public virtual void ResetID(string newID)
        {
            this.id = newID;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public override ITcpClient Connect()
        {
            return this.Connect("rrqm");
        }

        private bool isHandshaked;

        /// <summary>
        /// 处理接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.isHandshaked)
            {
                this.HandleTokenReceivedData(byteBlock, requestInfo);
            }
            else
            {
                WaitVerify waitVerify = byteBlock.ToString().ToJsonObject<WaitVerify>();
                this.waitHandlePool.SetRun(waitVerify);
                SpinWait.SpinUntil(() =>
                {
                    return waitVerify.Handle;
                }, 3000);
            }
            base.HandleReceivedData(byteBlock, requestInfo);
        }

        /// <summary>
        /// 处理Token数据，覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected virtual void HandleTokenReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<ITokenPlugin>("OnHandleTokenData", this, new ReceivedDataEventArgs(byteBlock, requestInfo));
            }
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public virtual ITcpClient Connect(string verifyToken, CancellationToken token = default)
        {
            if (!this.Online)
            {
                base.Connect();
            }

            WaitVerify waitVerify = new WaitVerify()
            {
                Token = verifyToken
            };
            WaitData<IWaitResult> waitData = this.waitHandlePool.GetWaitData(waitVerify);
            waitData.SetCancellationToken(token);

            byte[] data = waitVerify.ToJsonBytes();
            base.Send(data, 0, data.Length);
            try
            {
                switch (waitData.Wait(1000 * 10))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitVerify verifyResult = (WaitVerify)waitData.WaitResult;
                            if (verifyResult.Status == 1)
                            {
                                this.id = verifyResult.ID;
                                this.isHandshaked = true;
                                this.OnHandshaked(new MesEventArgs("Token客户端成功连接"));
                                verifyResult.Handle = true;
                                return this;
                            }
                            else if (verifyResult.Status == 3)
                            {
                                verifyResult.Handle = true;
                                this.Close("连接数量已达到服务器设定最大值");
                                throw new RRQMException("连接数量已达到服务器设定最大值");
                            }
                            else if (verifyResult.Status == 4)
                            {
                                verifyResult.Handle = true;
                                this.Close("服务器拒绝连接");
                                throw new RRQMException("服务器拒绝连接");
                            }
                            else
                            {
                                verifyResult.Handle = true;
                                this.Close(verifyResult.Message);
                                throw new RRQMTokenVerifyException(verifyResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        this.Close("连接超时");
                        throw new TimeoutException("连接超时");
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        this.Close(null);
                        return this;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        #region 事件

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        public event RRQMMessageEventHandler<TokenClientBase> Handshaked;

        /// <summary>
        /// 在完成握手连接时，覆盖父类方法将不会触发插件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(MesEventArgs e)
        {
            this.Handshaked?.Invoke(this, e);
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<ITokenPlugin>("OnHandshaked", this, e);
            }
        }

        #endregion 事件

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected override void LoadConfig(RRQMConfig config)
        {
            base.LoadConfig(config);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            this.isHandshaked = false;
            base.OnDisconnected(e);
        }
    }
}