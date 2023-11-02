using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道客户端接口
    /// </summary>
    public interface INamedPipeClient : INamedPipeClientBase,ISetupConfigObject,IConnectObject
    {
        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        ConnectedEventHandler<INamedPipeClient> Connected { get; set; }

        /// <summary>
        /// 准备连接的时候
        /// </summary>
        ConnectingEventHandler<INamedPipeClient> Connecting { get; set; }
    }
}