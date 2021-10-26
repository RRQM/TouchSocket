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

using System.Threading;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpc调用上下文
    /// </summary>
    public class JsonRpcServerCallContext : IServerCallContext
    {
        internal ICaller caller;
        internal JsonRpcContext context;
        internal MethodInstance methodInstance;
        internal string jsonString;
        internal JsonRpcProtocolType protocolType;
        internal MethodInvoker methodInvoker;
        internal CancellationTokenSource tokenSource;

        /// <summary>
        /// Json字符串
        /// </summary>
        public string JsonString
        {
            get { return jsonString; }
        }

        /// <summary>
        /// 协议类型
        /// </summary>
        public JsonRpcProtocolType ProtocolType
        {
            get { return protocolType; }
            set { protocolType = value; }
        }

#pragma warning disable CS1591
        public ICaller Caller => this.caller;

        public IRpcContext Context => this.context;

        public MethodInstance MethodInstance => this.methodInstance;

        public MethodInvoker MethodInvoker => this.methodInvoker;

        public CancellationTokenSource TokenSource =>this.tokenSource;
    }
}