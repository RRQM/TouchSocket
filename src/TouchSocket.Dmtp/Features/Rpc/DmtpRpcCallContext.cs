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

using System.Threading;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// DmtpRpcCallContext
    /// </summary>
    public sealed class DmtpRpcCallContext : CallContext, IDmtpRpcCallContext
    {
        /// <summary>
        /// DmtpRpcCallContext
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="methodInstance"></param>
        /// <param name="dmtpRpcPackage"></param>
        /// <param name="resolver"></param>
        public DmtpRpcCallContext(object caller, MethodInstance methodInstance, DmtpRpcPackage dmtpRpcPackage, IResolver resolver) : base(caller, methodInstance, resolver)
        {
            this.DmtpRpcPackage = dmtpRpcPackage;
        }

        /// <inheritdoc/>
        public DmtpRpcPackage DmtpRpcPackage { get; }

        /// <inheritdoc/>
        public Metadata Metadata => this.DmtpRpcPackage.Metadata;

        /// <inheritdoc/>
        public SerializationType SerializationType => this.DmtpRpcPackage == null ? (SerializationType)byte.MaxValue : this.DmtpRpcPackage.SerializationType;
    }
}