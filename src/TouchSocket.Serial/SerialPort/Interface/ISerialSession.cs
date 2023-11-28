
using System;

using TouchSocket.Core;
using TouchSocket.Sockets;
namespace TouchSocket.Serial
{

    /// <summary>
    /// <inheritdoc cref="ISerialSessionBase"/>
    /// </summary>
    public interface ISerialSession : ISerialSessionBase, IClientSender, IPluginObject, ISetupConfigObject
    {
        /// <summary>
        /// 成功打开串口
        /// </summary>
        ConnectedEventHandler<ISerialSession> Connected { get; set; }

        /// <summary>
        /// 准备连接串口的时候
        /// </summary>
        SerialConnectingEventHandler<ISerialSession> Connecting { get; set; }

        /// <summary>
        /// 连接串口
        /// </summary>
        /// <exception cref="Exception"></exception>
        ISerialSession Connect();

    }
}
