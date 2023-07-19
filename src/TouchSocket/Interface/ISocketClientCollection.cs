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
//------------------------------------------------------------------------------
using System.Collections.Generic;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 客户端集合
    /// </summary>
    public interface ISocketClientCollection
    {
        /// <summary>
        /// 集合长度
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 通过Id查找<see cref="ISocketClient"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ISocketClient this[string id] { get; }

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        IEnumerable<ISocketClient> GetClients();

        /// <summary>
        /// 获取Id集合
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetIds();

        /// <summary>
        /// 根据Id判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool SocketClientExist(string id);

        /// <summary>
        /// 尝试获取实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        bool TryGetSocketClient(string id, out ISocketClient socketClient);

        /// <summary>
        /// 尝试获取实例
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        bool TryGetSocketClient<TClient>(string id, out TClient socketClient) where TClient : ISocketClient;
    }
}