using System;
using System.Collections.Generic;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务器接口
    /// </summary>
    public interface INamedPipeService : IService, IIdSender, IIdRequsetInfoSender
    {
        /// <summary>
        /// 当前在线客户端数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取最大可连接数
        /// </summary>
        int MaxCount { get; }

        /// <summary>
        /// 管道监听集合
        /// </summary>
        IEnumerable<NamedPipeMonitor> Monitors { get; }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        INamedPipeSocketClientCollection SocketClients { get; }

        /// <summary>
        /// 清理当前已连接的所有客户端
        /// </summary>
        void Clear();

        /// <summary>
        /// 获取当前在线的所有Id集合
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetIds();

        /// <summary>
        /// 重置Id
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="Exception"></exception>
        void ResetId(string oldId, string newId);

        /// <summary>
        /// 根据Id判断<see cref="INamedPipeSocketClient"/>是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool SocketClientExist(string id);
    }
}