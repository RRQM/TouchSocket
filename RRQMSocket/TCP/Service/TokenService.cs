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

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP服务器
    /// </summary>
    public class TokenService<TClient> : TcpService<TClient> where TClient : TokenSocketClient, new()
    {
        private string verifyToken;

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken
        {
            get { return verifyToken; }
        }

        private int verifyTimeout;

        /// <summary>
        /// 验证超时时间,默认为3000ms
        /// </summary>
        public int VerifyTimeout
        {
            get { return verifyTimeout; }
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="ServiceConfig"></param>
        protected override void LoadConfig(ServiceConfig ServiceConfig)
        {
            base.LoadConfig(ServiceConfig);
            this.verifyTimeout = (int)ServiceConfig.GetValue(TokenServiceConfig.VerifyTimeoutProperty);
            this.verifyToken = (string)ServiceConfig.GetValue(TokenServiceConfig.VerifyTokenProperty);
            if (string.IsNullOrEmpty(this.verifyToken))
            {
                this.verifyToken = "rrqm";
            }
        }

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.verifyTimeout = this.verifyTimeout;
            socketClient.verifyToken = this.verifyToken;
            base.OnConnecting(socketClient, e);
        }
    }
}