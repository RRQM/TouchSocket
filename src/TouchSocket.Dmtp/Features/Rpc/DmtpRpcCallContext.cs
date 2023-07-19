//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// DmtpRpcCallContext
    /// </summary>
    public class DmtpRpcCallContext : IDmtpRpcCallContext
    {
        private CancellationTokenSource m_tokenSource;

        /// <inheritdoc/>
        public object Caller { get; internal set; }

        /// <inheritdoc/>
        public MethodInstance MethodInstance { get; internal set; }

        /// <inheritdoc/>
        public SerializationType SerializationType => this.DmtpRpcPackage == null ? (SerializationType)byte.MaxValue : this.DmtpRpcPackage.SerializationType;

        /// <inheritdoc/>
        public DmtpRpcPackage DmtpRpcPackage { get; internal set; }

        /// <inheritdoc/>
        public CancellationTokenSource TokenSource
        {
            get
            {
                if (this.m_tokenSource == null)
                {
                    this.m_tokenSource = new CancellationTokenSource();
                }
                return this.m_tokenSource;
            }
        }

        /// <inheritdoc/>
        public bool TryCancel()
        {
            if (this.m_tokenSource != null)
            {
                this.m_tokenSource.Cancel();
                return true;
            }
            return false;
        }
    }
}