using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// IWebsocketSmtpClientBase
    /// </summary>
    public interface IWebsocketSmtpClientBase : IClient, IPluginObject, ISmtpActorObject
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
        /// 是否已完成<see cref="ISmtpActor.IsHandshaked"/>
        /// </summary>
        bool IsHandshaked { get; }
    }
}
