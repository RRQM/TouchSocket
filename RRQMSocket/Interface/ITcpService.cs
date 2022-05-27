using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP系列服务器接口
    /// </summary>
    public interface ITcpService<TClient> : ITcpService, IIDSender where TClient : ISocketClient
    {
        /// <summary>
        /// 用户连接完成
        /// </summary>
        event RRQMEventHandler<TClient> Connected;

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        event RRQMClientOperationEventHandler<TClient> Connecting;

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        event RRQMTcpClientDisconnectedEventHandler<TClient> Disconnected;

        /// <summary>
        /// 获取最大可连接数
        /// </summary>
        int MaxCount { get; }

        /// <summary>
        /// 获取清理无数据交互的SocketClient，默认60。如果不想清除，可使用-1。
        /// </summary>
        int ClearInterval { get; }

        /// <summary>
        /// 根据ID判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool SocketClientExist(string id);

        /// <summary>
        /// 尝试获取TClient
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="socketClient">TClient</param>
        /// <returns></returns>
        bool TryGetSocketClient(string id, out TClient socketClient);
    }

    /// <summary>
    /// TCP服务器接口
    /// </summary>
    public interface ITcpService : IService
    {
        /// <summary>
        /// 内置IOC容器
        /// </summary>
        IContainer Container { get; }

        /// <summary>
        /// 使用Ssl加密
        /// </summary>
        bool UseSsl { get; }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        SocketClientCollection SocketClients { get; }

        /// <summary>
        /// 网络监听集合
        /// </summary>
        NetworkMonitor[] Monitors { get; }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        void ResetID(string oldID, string newID);

        /// <summary>
        /// 获取当前在线的所有ID集合
        /// </summary>
        /// <returns></returns>
        string[] GetIDs();

        /// <summary>
        /// 清理当前已连接的所有客户端
        /// </summary>
        void Clear();
    }
}
