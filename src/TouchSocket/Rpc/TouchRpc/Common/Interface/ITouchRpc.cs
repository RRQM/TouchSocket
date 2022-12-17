namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// ITouchRpc
    /// </summary>
    public interface ITouchRpc : IRpcActor
    {
        /// <summary>
        /// Rpc执行角色
        /// </summary>
        RpcActor RpcActor { get; }
    }
}
