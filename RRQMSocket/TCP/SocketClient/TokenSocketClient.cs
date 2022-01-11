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
        public int VerifyTimeout
        {
            get { return verifyTimeout; }
        }

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken
        {
            get { return verifyToken; }
        }

        /// <summary>
        /// 等待返回池
        /// </summary>
        public RRQMWaitHandlePool<IWaitResult> WaitHandlePool { get => this.waitHandlePool; }

        /// <summary>
        /// 处理接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override sealed void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            if (isHandshaked)
            {
                this.HandleTokenReceivedData(byteBlock, obj);
            }
            else
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
                    base.Send(data,0,data.Length);
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
        }

        /// <summary>
        /// 处理Token数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected abstract void HandleTokenReceivedData(ByteBlock byteBlock, object obj);

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
                 if (!isHandshaked)
                 {
                     this.BreakOut("验证超时");
                 }
             });
        }
    }
}