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
using RRQMCore.Run;

namespace RRQMSocket
{
    /// <summary>
    /// 令箭辅助类
    /// </summary>
    public abstract class TokenSocketClient : SocketClient, ITokenClientBase
    {
        internal int verifyTimeout;
        internal string verifyToken;
        private bool isHandshaked;
        private RRQMWaitHandlePool<IWaitResult> waitHandlePool;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenSocketClient()
        {
            this.waitHandlePool = new RRQMWaitHandlePool<IWaitResult>();
        }

        /// <summary>
        /// 验证超时时间,默认为3000ms
        /// </summary>
        public int VerifyTimeout => this.verifyTimeout;

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.verifyToken;

        /// <summary>
        /// 等待返回池
        /// </summary>
        public RRQMWaitHandlePool<IWaitResult> WaitHandlePool => this.waitHandlePool;

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
                try
                {
                    WaitVerify waitVerify = WaitVerify.GetVerifyInfo(byteBlock.ToArray());
                    VerifyOption verifyOption = new VerifyOption();
                    verifyOption.Token = waitVerify.Token;
                    this.OnVerifyToken(verifyOption);
                    if (verifyOption.Accept)
                    {
                        waitVerify.ID = this.ID;
                        waitVerify.Status = 1;
                        var data = waitVerify.GetData();
                        base.Send(data, 0, data.Length);
                        this.isHandshaked = true;
                        this.online = true;
                        this.OnConnected(new MesEventArgs("Token客户端成功连接"));
                    }
                    else
                    {
                        waitVerify.Status = 2;
                        waitVerify.Message = verifyOption.ErrorMessage;
                        var data = waitVerify.GetData();
                        base.Send(data, 0, data.Length);
                        this.BreakOut(verifyOption.ErrorMessage);
                    }
                }
                catch (System.Exception ex)
                {
                    if (this.OnAbnormalVerify(byteBlock, requestInfo))
                    {
                        this.isHandshaked = true;
                        this.online = true;
                        this.OnConnected(new MesEventArgs("非常规Token客户端成功连接"));
                    }
                    else
                    {
                        this.BreakOut(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 收到非正常连接。
        /// 一般地，这是由其他类型客户端发起的连接。
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        /// <returns>返回值指示，是否接受该请求</returns>
        protected virtual bool OnAbnormalVerify(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            return false;
        }

        /// <summary>
        /// 处理Token数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected abstract void HandleTokenReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo);

        /// <summary>
        /// 当验证Token时
        /// </summary>
        /// <param name="verifyOption"></param>
        protected virtual void OnVerifyToken(VerifyOption verifyOption)
        {
            if (verifyOption.Token == this.verifyToken)
            {
                verifyOption.Accept = true;
            }
            else
            {
                verifyOption.ErrorMessage = "Token不受理";
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void PreviewConnected(MesEventArgs e)
        {
            this.BeginReceive();
            RRQMCore.Run.EasyAction.DelayRun(this.verifyTimeout, () =>
             {
                 if (!this.isHandshaked)
                 {
                     this.BreakOut("验证超时");
                 }
             });
        }
    }
}