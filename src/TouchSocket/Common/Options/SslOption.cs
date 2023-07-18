//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Ssl配置
    /// </summary>
    public abstract class SslOption
    {
        /// <summary>
        /// Ssl配置
        /// </summary>
        public SslOption()
        {
            this.CertificateValidationCallback = this.OnCertificateValidationCallback;
        }

        private bool OnCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 协议版本
        /// </summary>
        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Default |
                         SslProtocols.Tls | SslProtocols.Tls11;

        /// <summary>
        /// 该值指定身份验证期间是否检查证书吊销列表
        /// </summary>
        public bool CheckCertificateRevocation { get; set; } = false;

        /// <summary>
        /// SSL验证回调。
        /// </summary>
        public RemoteCertificateValidationCallback CertificateValidationCallback { get; set; }
    }
}