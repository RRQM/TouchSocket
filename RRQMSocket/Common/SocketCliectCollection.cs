//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace RRQMSocket
{
    /// <summary>
    /// 客户端集合
    /// </summary>
    [DebuggerDisplay("Count={Count}")]
    public class SocketClientCollection<T> where T : ISocketClient
    {
        private RRQMCore.SnowflakeIDGenerator iDGenerator = new RRQMCore.SnowflakeIDGenerator(4);

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get { return this.tokenDic.Count; } }

        private ConcurrentDictionary<string, T> tokenDic = new ConcurrentDictionary<string, T>();

        internal bool TryAdd(T socketClient)
        {
            return this.tokenDic.TryAdd(socketClient.ID, socketClient);
        }

        internal string GetDefaultID()
        {
            return iDGenerator.NextID().ToString();
        }

        /// <summary>
        /// 获取ID集合
        /// </summary>
        /// <returns></returns>
        public string[] GetIDs()
        {
            return this.tokenDic.Keys.ToArray();
        }

        internal bool TryRemove(string id)
        {
            return this.tokenDic.TryRemove(id, out _);
        }

        /// <summary>
        /// 尝试获取实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        public bool TryGetSocketClient(string id, out T socketClient)
        {
            return this.tokenDic.TryGetValue(id, out socketClient);
        }

        /// <summary>
        /// 根据ID判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SocketClientExist(string id)
        {
            if (tokenDic.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取SocketClient
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T this[string id]
        {
            get
            {
                T t;
                this.TryGetSocketClient(id, out t);
                return t;
            }
        }


        internal void Clear()
        {
            foreach (var client in this.tokenDic.Values)
            {
                client.Dispose();
            }
        }
    }
}