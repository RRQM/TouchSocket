using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// IWebsocketDmtpClientBase
    /// </summary>
    public interface IWebsocketDmtpClientBase : IClient, IPluginObject, IDmtpActorObject
    {
        /// <summary>
        /// 客户端的Id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 加载到当前客户端的配置
        /// </summary>
        TouchSocketConfig Config { get; }

        /// <summary>
        /// 是否已完成<see cref="IDmtpActor.IsHandshaked"/>
        /// </summary>
        bool IsHandshaked { get; }
    }
}
