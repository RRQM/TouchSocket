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

using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// DmtpRpcCallContext
    /// </summary>
    public sealed class DmtpRpcCallContext : CallContext, IDmtpRpcCallContext
    {
        private readonly IScopedResolver m_scopedResolver;

        /// <summary>
        /// 初始化 DmtpRpcCallContext 类的新实例。
        /// </summary>
        /// <param name="caller">调用者对象，表示触发RPC方法的实例。</param>
        /// <param name="rpcMethod">RpcMethod对象，表示将要调用的RPC方法。</param>
        /// <param name="dmtpRpcPackage">IDmtpRpcRequestPackage对象，表示RPC请求包。</param>
        /// <param name="scopedResolver">IResolver接口的实现，用于解析依赖注入。</param>
        public DmtpRpcCallContext(object caller, RpcMethod rpcMethod, IDmtpRpcRequestPackage dmtpRpcPackage, IScopedResolver scopedResolver) : base(caller, rpcMethod, scopedResolver.Resolver)
        {
            this.DmtpRpcPackage = dmtpRpcPackage;
            this.m_scopedResolver = scopedResolver;
        }

        /// <inheritdoc/>
        public IDmtpRpcRequestPackage DmtpRpcPackage { get; }

        /// <inheritdoc/>
        public Metadata Metadata => this.DmtpRpcPackage.Metadata;

        /// <inheritdoc/>
        public SerializationType SerializationType => this.DmtpRpcPackage == null ? (SerializationType)byte.MaxValue : this.DmtpRpcPackage.SerializationType;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (disposing)
            {
                this.m_scopedResolver.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}