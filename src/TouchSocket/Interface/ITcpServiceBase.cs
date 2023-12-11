//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    
    /// <summary>
    /// Tcp服务器接口
    /// </summary>
    public interface ITcpServiceBase : IService, IIdSender, IIdRequsetInfoSender, IPluginObject
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
        /// 网络监听集合
        /// </summary>
        IEnumerable<TcpNetworkMonitor> Monitors { get; }

        /// <summary>
        /// 获取当前连接的所有客户端
        /// </summary>
        ISocketClientCollection SocketClients { get; }

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
        /// 根据Id判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool SocketClientExist(string id);
    }
}