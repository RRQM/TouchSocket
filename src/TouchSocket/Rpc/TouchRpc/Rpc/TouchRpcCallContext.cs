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
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// TouchRpc上下文
    /// </summary>
    internal class TouchRpcCallContext : ITouchRpcCallContext
    {
        private CancellationTokenSource m_tokenSource;

        /// <summary>
        /// 当<see cref="TokenSource"/>不为空时，调用<see cref="CancellationTokenSource.Cancel()"/>
        /// </summary>
        /// <returns></returns>
        public bool TryCancel()
        {
            if (m_tokenSource != null)
            {
                m_tokenSource.Cancel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object Caller { get; internal set; }

        /// <summary>
        /// 能取消的调用令箭，在客户端主动取消或网络故障时生效
        /// </summary>
        public CancellationTokenSource TokenSource
        {
            get
            {
                if (m_tokenSource == null)
                {
                    m_tokenSource = new CancellationTokenSource();
                }
                return m_tokenSource;
            }
        }

        /// <summary>
        /// TouchRpcContext
        /// </summary>
        public TouchRpcPackage TouchRpcPackage { get; internal set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodInstance MethodInstance { get; internal set; }

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType => TouchRpcPackage == null ? (SerializationType)byte.MaxValue : TouchRpcPackage.SerializationType;
    }
}