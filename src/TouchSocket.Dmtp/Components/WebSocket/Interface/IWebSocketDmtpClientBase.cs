using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 基于WebSocket协议的Dmtp终端接口
    /// </summary>
    public interface IWebSocketDmtpClientBase : IClient, IPluginObject, IDmtpActorObject,IConfigObject,IHandshakeObject
    {
        /// <summary>
        /// 客户端的Id
        /// </summary>
        string Id { get; }
    }
}