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

namespace RRQMSocket
{
    /// <summary>
    /// 令箭辅助类
    /// </summary>
    public class TokenSocketClient : SocketClient, ITokenClientBase
    {
        internal Action<TokenSocketClient, MesEventArgs> internalOnHandshaked;
        internal Action<TokenSocketClient, VerifyOptionEventArgs> internalOnVerifyToken;
        internal Action<TokenSocketClient, ReceivedDataEventArgs> internalOnAbnormalVerify;
        internal Action<TokenSocketClient, ByteBlock, IRequestInfo> internalHandleTokenData;
        private bool isHandshaked;
        private WaitHandlePool<IWaitResult> waitHandlePool;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenSocketClient()
        {
            this.waitHandlePool = new WaitHandlePool<IWaitResult>();
            this.Protocol = Protocol.RRQMToken;
        }

        /// <summary>
        /// 验证超时时间,默认为3000ms
        /// </summary>
        public int VerifyTimeout
        {
            get => this.GetValue<int>(RRQMConfigExtensions.VerifyTimeoutProperty);
            set => this.SetValue(RRQMConfigExtensions.VerifyTimeoutProperty, value);
        }

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken
        {
            get => this.GetValue<string>(RRQMConfigExtensions.VerifyTokenProperty);
            set => this.SetValue(RRQMConfigExtensions.VerifyTokenProperty, value);
        }

        /// <summary>
        /// 等待返回池
        /// </summary>
        public WaitHandlePool<IWaitResult> WaitHandlePool => this.waitHandlePool;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsHandshaked => this.isHandshaked;

        /// <summary>
        /// 处理接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.isHandshaked)
            {
                this.HandleTokenData(byteBlock, requestInfo);
            }
            else
            {
                try
                {
                    WaitVerify waitVerify = byteBlock.ToString().ToJsonObject<WaitVerify>();
                    VerifyOptionEventArgs args = new VerifyOptionEventArgs(waitVerify.Token);
                    this.OnVerifyToken(args);
                    if (!args.Operation.HasFlag(Operation.Handled))
                    {
                        this.internalOnVerifyToken?.Invoke(this, args);
                    }

                    if (args.Operation.HasFlag(Operation.Permit))
                    {
                        waitVerify.ID = this.ID;
                        waitVerify.Status = 1;
                        var data = waitVerify.ToJsonBytes();
                        base.Send(data, 0, data.Length);
                        this.isHandshaked = true;
                        this.OnHandshaked(new MesEventArgs("Token客户端成功连接"));
                    }
                    else
                    {
                        waitVerify.Status = 2;
                        waitVerify.Message = args.Message;
                        var data = waitVerify.ToJsonBytes();
                        base.Send(data, 0, data.Length);
                        this.Close(args.Message);
                    }
                }
                catch (Exception ex)
                {
                    ReceivedDataEventArgs args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                    this.OnAbnormalVerify(args);
                    if (!args.Operation.HasFlag(Operation.Handled))
                    {
                        this.internalOnAbnormalVerify?.Invoke(this, args);
                    }
                    if (args.Operation.HasFlag(Operation.Permit))
                    {
                        this.isHandshaked = true;
                        this.OnHandshaked(new MesEventArgs("非常规Token客户端成功连接"));
                    }
                    else
                    {
                        this.Close(ex.Message);
                    }
                }
            }
            base.HandleReceivedData(byteBlock, requestInfo);
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(MesEventArgs e)
        {
            this.internalOnHandshaked?.Invoke(this, e);
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<ITokenPlugin>("OnHandshaked", this, e);
            }
        }

        /// <summary>
        /// 处理Token数据，覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected virtual void HandleTokenData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.internalHandleTokenData?.Invoke(this, byteBlock, requestInfo);
            if (this.usePlugin)
            {
                this.PluginsManager.Raise<ITokenPlugin>("OnHandleTokenData", this, new ReceivedDataEventArgs(byteBlock, requestInfo));
            }
        }

        /// <summary>
        /// 收到非正常连接。覆盖父类方法将不会触发事件和插件。
        ///<para> 一般地，这是由普通TCP发起的连接请求。</para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAbnormalVerify(ReceivedDataEventArgs e)
        {
            e.RemoveOperation(Operation.Permit);
            this.internalOnAbnormalVerify?.Invoke(this, e);

            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<ITokenPlugin>("OnAbnormalVerify", this, e);
            }
        }

        /// <summary>
        /// 当验证Token时，覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual void OnVerifyToken(VerifyOptionEventArgs e)
        {
            if (e.Token == this.VerifyToken)
            {
                e.AddOperation(Operation.Permit);
            }
            else
            {
                e.Message = "Token不受理";
            }

            this.internalOnVerifyToken?.Invoke(this, e);

            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<ITokenPlugin>("OnVerifyToken", this, e);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnected(RRQMEventArgs e)
        {
            EasyAction.DelayRun(this.VerifyTimeout, () =>
            {
                if (!this.isHandshaked)
                {
                    this.Close("验证超时");
                }
            });
            base.OnConnected(e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnClose()
        {
            this.isHandshaked = false;
            base.OnClose();
        }
    }
}