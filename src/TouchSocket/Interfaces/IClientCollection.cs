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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 客户端集合类
    /// </summary>
    /// <typeparam name="TClient">客户端类型参数，必须实现IIdClient接口</typeparam>
    public interface IClientCollection<TClient> : IEnumerable<TClient> where TClient : IIdClient
    {
        /// <summary>
        /// 集合中客户端的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 通过客户端的唯一标识符(Id)获取客户端对象。如果找不到对应的客户端，应返回null。
        /// </summary>
        /// <param name="id">客户端的唯一标识符</param>
        /// <returns>对应的客户端对象，如果找不到则返回null</returns>
        public TClient this[string id] { get; }

        /// <summary>
        /// 获取集合中所有客户端的唯一标识符(Id)
        /// </summary>
        /// <returns>一个包含所有客户端Id的集合</returns>
        IEnumerable<string> GetIds();

        /// <summary>
        /// 判断指定Id的客户端是否存在于集合中
        /// </summary>
        /// <param name="id">要查找的客户端的唯一标识符</param>
        /// <returns>如果集合中存在该Id对应的客户端返回true，否则返回false</returns>
        bool ClientExist(string id);

        /// <summary>
        /// 尝试获取指定Id的客户端对象
        /// </summary>
        /// <param name="id">要获取的客户端的唯一标识符</param>
        /// <param name="client">输出参数，用于存储找到的客户端对象</param>
        /// <returns>如果找到对应的客户端对象返回true，否则返回false</returns>
        bool TryGetClient(string id, out TClient client);
    }
}