namespace RRQMSocket.RPC
{
    /// <summary>
    /// 服务器调用上下文
    /// </summary>
    public interface IServerCallContext
    {
        /// <summary>
        /// 函数实例
        /// </summary>
        MethodInstance MethodInstance { get; }

        /// <summary>
        /// 实际调用者
        /// </summary>
        ICaller Caller { get; }

        /// <summary>
        /// 调用信使
        /// </summary>
        MethodInvoker MethodInvoker { get; }

        /// <summary>
        /// RPC请求实际
        /// </summary>
        IRpcContext Context { get; }
    }
}
