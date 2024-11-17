//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;

namespace TouchSocket.Sockets
{
        /// <summary>
    /// 客户端Ssl验证
    /// </summary>
    public class ClientSslOption : SslOption
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ClientSslOption()
        {
            // 初始化一个X509证书存储，用于读取和验证根证书
            var store = new X509Store(StoreName.Root);
            // 打开证书存储，允许读写操作
            store.Open(OpenFlags.ReadWrite);
            // 将证书存储中的所有证书赋值给客户端证书属性
            this.ClientCertificates = store.Certificates;
            // 关闭证书存储
            store.Close();
        }

        /// <summary>
        /// 目标Host
        /// </summary>
        public string TargetHost { get; set; }

        /// <summary>
        /// 验证组合
        /// </summary>
        public X509CertificateCollection ClientCertificates { get; set; }
    }
}