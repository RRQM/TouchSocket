using RRQMCore.Serialization;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RRQMRPC服务上下文
    /// </summary>
    public class RpcServerCallContext : IServerCallContext
    {
        internal ICaller caller;
        internal RpcContext context;
        internal MethodInstance methodInstance;
        internal MethodInvoker methodInvoker;


#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public ICaller Caller => this.caller;

        public IRpcContext Context => this.context;

        public MethodInstance MethodInstance => this.methodInstance;

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType => this.context == null ? (SerializationType)byte.MaxValue : this.context.SerializationType;

        public MethodInvoker MethodInvoker => methodInvoker;
    }
}