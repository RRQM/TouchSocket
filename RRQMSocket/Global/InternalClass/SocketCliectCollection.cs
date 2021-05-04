//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Exceptions;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace RRQMSocket
{
    /// <summary>
    /// 客户端集合
    /// </summary>
    [DebuggerDisplay("Count={Count}")]
    public class SocketCliectCollection<T> : IEnumerable<T> where T : TcpSocketClient
    {
        /// <summary>
        /// 获取或设置分配ID的格式
        /// </summary>
        public string IDFormat { get; set; }

        private int number;

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get { return this.tokenDic.Count; } }

        private ConcurrentDictionary<string, T> tokenDic = new ConcurrentDictionary<string, T>();

        internal void Add(T socketClient)
        {
            if (this.tokenDic.TryAdd(socketClient.ID, socketClient))
            {
                return;
            }
            throw new RRQMException("ID重复添加");
        }

        internal string GetDefaultID()
        {
            return string.Format(IDFormat, number++);
        }

        internal ICollection<string> GetTokens()
        {
            return this.tokenDic.Keys ;
        }

        internal void Remove(string token)
        {
            this.tokenDic.TryRemove(token,out _);
        }

        internal void Clear()
        {
            this.tokenDic.Clear();
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

        /// <summary>
        /// 用于枚举
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.tokenDic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.tokenDic.Values.GetEnumerator();
        }
    }
}