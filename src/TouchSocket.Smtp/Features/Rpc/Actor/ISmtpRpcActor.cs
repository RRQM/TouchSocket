using TouchSocket.Rpc;

namespace TouchSocket.Smtp.Rpc
{
    /// <summary>
    /// 能够基于SMTP协议提供Rpc功能的接口
    /// </summary>
    public interface ISmtpRpcActor : IRpcClient, IActor, ITargetRpcClient
    {
    }
}
