//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.Dependency;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// TCP服务器辅助接口
    /// </summary>
    public interface ITcpServiceBase : IService
    {
        /// <summary>
        /// 内置IOC容器
        /// </summary>
        IContainer Container { get; set; }

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
        /// <param name="waitSetID"></param>
        void ResetID(WaitSetID waitSetID);

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

    /// <summary>
    /// TCP系列服务器接口
    /// </summary>
    public interface ITcpService<TClient> : ITcpServiceBase, IIDSender where TClient : ISocketClient
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
    /// 服务器接口
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// 服务器状态
        /// </summary>
        ServerState ServerState { get; }

        /// <summary>
        /// 获取服务器配置
        /// </summary>
        RRQMConfig Config { get; }

        /// <summary>
        /// 名称
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="serverConfig">配置</param>
        /// <exception cref="RRQMException"></exception>
        /// <returns>设置的服务实例</returns>
        IService Setup(RRQMConfig serverConfig);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="port"></param>
        /// <exception cref="RRQMException"></exception>
        /// <returns>设置的服务实例</returns>
        IService Setup(int port);

        /// <summary>
        /// 启动
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns>设置的服务实例</returns>
        IService Start();

        /// <summary>
        /// 停止
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <returns>设置的服务实例</returns>
        IService Stop();
    }
}
