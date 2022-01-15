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
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Run;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP客户端
    /// </summary>
    public abstract class TokenClient : TcpClient, ITokenClient
    {
        private RRQMWaitHandlePool<IWaitResult> waitHandlePool;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenClient()
        {
            this.waitHandlePool = new RRQMWaitHandlePool<IWaitResult>();
        }

        /// <summary>
        /// 等待返回池
        /// </summary>
        public RRQMWaitHandlePool<IWaitResult> WaitHandlePool { get => this.waitHandlePool; }

        private string id;

        /// <summary>
        /// 获取服务器分配的ID
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// 重新设置ID,但是不会同步到服务器
        /// </summary>
        /// <param name="id"></param>
        public virtual void ResetID(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
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
        protected override sealed void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (isHandshaked)
            {
                this.HandleTokenReceivedData(byteBlock, requestInfo);
            }
            else
            {
                WaitVerify waitVerify = WaitVerify.GetVerifyInfo(byteBlock.ToArray());
                this.waitHandlePool.SetRun(waitVerify);
                System.Threading.SpinWait.SpinUntil(() =>
                {
                    return waitVerify.Handle;
                }, 3000);
            }
        }

        /// <summary>
        /// 处理Token数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected abstract void HandleTokenReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo);

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public virtual ITcpClient Connect(string verifyToken, CancellationToken token = default)
        {
            lock (this)
            {
                if (this.Online)
                {
                    return this;
                }
                this.ConnectService();
                this.BeginReceive();
                WaitVerify waitVerify = new WaitVerify()
                {
                    Token = verifyToken
                };
                WaitData<IWaitResult> waitData = this.waitHandlePool.GetWaitData(waitVerify);
                waitData.SetCancellationToken(token);

                byte[] data = waitVerify.GetData();
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
                                    this.PreviewConnected(new MesEventArgs("Token客户端成功连接"));
                                    verifyResult.Handle = true;
                                    return this;
                                }
                                else if (verifyResult.Status == 3)
                                {
                                    verifyResult.Handle = true;
                                    this.BreakOut("连接数量已达到服务器设定最大值");
                                    this.MainSocket.Dispose();
                                    throw new RRQMException("连接数量已达到服务器设定最大值");
                                }
                                else if (verifyResult.Status == 4)
                                {
                                    verifyResult.Handle = true;
                                    this.BreakOut("服务器拒绝连接");
                                    this.MainSocket.Dispose();
                                    throw new RRQMException("服务器拒绝连接");
                                }
                                else
                                {
                                    verifyResult.Handle = true;
                                    this.BreakOut(verifyResult.Message);
                                    this.MainSocket.Dispose();
                                    throw new RRQMTokenVerifyException(verifyResult.Message);
                                }
                            }
                        case WaitDataStatus.Overtime:
                            this.BreakOut("连接超时");
                            this.MainSocket.Dispose();
                            throw new RRQMTimeoutException("连接超时");
                        case WaitDataStatus.Canceled:
                        case WaitDataStatus.Disposed:
                        default:
                            this.MainSocket.Dispose();
                            this.BreakOut(null);
                            return this;
                    }
                }
                finally
                {
                    this.WaitHandlePool.Destroy(waitData);
                }
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
        }

        /// <summary>
        /// 准备连接前设置。
        /// </summary>
        /// <param name="e"></param>
        protected override void PreviewConnected(MesEventArgs e)
        {
            this.online = true;
            this.OnConnected(e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnBreakOut()
        {
            this.isHandshaked = false;
            base.OnBreakOut();
        }
    }
}