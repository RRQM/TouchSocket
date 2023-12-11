using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// INamedPipeService
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface INamedPipeService<TClient> : INamedPipeServiceBase where TClient : INamedPipeSocketClient
    {
        /// <summary>
        /// 用户连接完成
        /// </summary>
        ConnectedEventHandler<TClient> Connected { get; set; }

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        ConnectingEventHandler<TClient> Connecting { get; set; }

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        DisconnectEventHandler<TClient> Disconnected { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        DisconnectEventHandler<TClient> Disconnecting { get; set; }

        /// <summary>
        /// 尝试获取TClient
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="socketClient">TClient</param>
        /// <returns></returns>
        bool TryGetSocketClient(string id, out TClient socketClient);
    }

    /// <summary>
    /// INamedPipeService
    /// </summary>
    public interface INamedPipeService : INamedPipeService<NamedPipeSocketClient>
    {

    }
}
