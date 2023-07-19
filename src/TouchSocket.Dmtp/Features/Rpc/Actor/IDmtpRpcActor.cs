using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 能够基于Dmtp协议提供Rpc功能的接口
    /// </summary>
    public interface IDmtpRpcActor : IRpcClient, IActor, ITargetRpcClient
    {
    }
}
