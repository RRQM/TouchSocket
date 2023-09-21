using TouchSocket.Http;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 基于Dmtp协议的Http终端接口
    /// </summary>
    public interface IHttpDmtpClientBase : IHttpClientBase, IDmtpActorObject
    {
    }
}