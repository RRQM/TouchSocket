//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 表示可连接的服务器基类接口
    /// </summary>
    public interface IConnectableService : IServiceBase
    {
        /// <summary>
        /// 获取已连接的客户端数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取已配置的最大可连接数量
        /// </summary>
        int MaxCount { get; }

        /// <summary>
        /// 清理（断开）已连接的所有客户端
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// 获取已连接的所有客户端Id集合
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetIds();

        /// <summary>
        /// 重置指定客户端的Id
        /// </summary>
        /// <param name="sourceId">源Id</param>
        /// <param name="targetId">目标Id</param>
        Task ResetIdAsync(string sourceId, string targetId);

        /// <summary>
        /// 根据Id判断对应的客户端是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ClientExists(string id);

        /// <summary>
        /// 获取已连接的所有客户端。
        /// </summary>
        /// <returns></returns>
        IEnumerable<IClient> GetClients();
    }
}