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

namespace RRQMSocket
{
    /// <summary>
    /// Token服务器
    /// </summary>
    public class TokenService<TClient> : TcpService<TClient> where TClient : TokenSocketClient
    {
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

        #region 事件

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        public event RRQMMessageEventHandler<TClient> Handshaked;

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(TClient client, MesEventArgs e)
        {
            this.Handshaked?.Invoke(client, e);
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        protected virtual void OnVerifyToken(TClient client, VerifyOptionEventArgs e)
        {

        }

        /// <summary>
        /// 收到非正常连接。
        /// 一般地，这是由其他类型客户端发起的连接。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        protected virtual void OnAbnormalVerify(TClient client, ReceivedDataEventArgs e)
        {

        }

        /// <summary>
        /// 处理Token收到的数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected virtual void HandleTokenData(TClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {

        }

        #endregion 事件

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="RRQMConfig"></param>
        protected override void LoadConfig(RRQMConfig RRQMConfig)
        {
            base.LoadConfig(RRQMConfig);
            this.SetValue(RRQMConfigExtensions.VerifyTimeoutProperty, (int)RRQMConfig.GetValue(RRQMConfigExtensions.VerifyTimeoutProperty));
            this.SetValue(RRQMConfigExtensions.VerifyTokenProperty, (string)RRQMConfig.GetValue(RRQMConfigExtensions.VerifyTokenProperty));
            if (string.IsNullOrEmpty(this.VerifyToken))
            {
                this.SetValue(RRQMConfigExtensions.VerifyTokenProperty, "rrqm");
            }
        }

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.internalOnHandshaked = this.PrivateHandshaked;
            socketClient.internalOnVerifyToken = this.PrivateVerifyToken;
            socketClient.internalOnAbnormalVerify = this.PrivateAbnormalVerify;
            socketClient.internalHandleTokenData = this.PrivateHandleTokenData;
            socketClient.SetValue(RRQMConfigExtensions.VerifyTimeoutProperty, this.VerifyTimeout);
            socketClient.SetValue(RRQMConfigExtensions.VerifyTokenProperty, this.VerifyToken);
            base.OnConnecting(socketClient, e);
        }

        private void PrivateVerifyToken(TokenSocketClient client, VerifyOptionEventArgs e)
        {
            this.OnVerifyToken((TClient)client, e);
        }

        private void PrivateAbnormalVerify(TokenSocketClient client, ReceivedDataEventArgs e)
        {
            this.OnAbnormalVerify((TClient)client, e);
        }

        private void PrivateHandshaked(TokenSocketClient client, MesEventArgs e)
        {
            this.OnHandshaked((TClient)client, e);
        }

        private void PrivateHandleTokenData(TokenSocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.HandleTokenData((TClient)client, byteBlock, requestInfo);
        }
    }

    /// <summary>
    /// 简单Token服务器
    /// </summary>
    public class TokenService : TokenService<TokenSocketClient>
    {
        /// <summary>
        /// 收到数据
        /// </summary>
        public event RRQMReceivedEventHandler<TokenSocketClient> Received;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleTokenData(TokenSocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(client, byteBlock, requestInfo);
            base.HandleTokenData(client, byteBlock, requestInfo);
        }
    }
}