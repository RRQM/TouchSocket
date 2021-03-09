//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 客户端集合
    /// </summary>
    [DebuggerDisplay("Count={Count}")]
    public class SocketCliectCollection<T> : IEnumerable<T> where T : TcpSocketClient
    {
        internal SocketCliectCollection(string tokenString)
        {
            this.tokenString = tokenString;
        }
        private string tokenString;
        private int num;

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get { return this.tokenDic.Count; } }

        ConcurrentDictionary<string, T> tokenDic = new ConcurrentDictionary<string, T>();

        internal void Add(T socketClient, bool reBulid)
        {
            if (reBulid)
            {
                num++;
                string key = $"{num}-{this.tokenString}";
                socketClient.Token = key;
                this.tokenDic.TryAdd(key, socketClient);
            }
            else
            {
                this.tokenDic.TryAdd(socketClient.Token, socketClient);
            }
        }

        internal ICollection<string> GetTokens()
        {
            return tokenDic.Keys;
        }

        internal void Remove(string token)
        {
            this.tokenDic.TryRemove(token, out _);
        }

        internal void Clear()
        {
            this.tokenDic.Clear();
        }

        /// <summary>
        /// 获取SocketClient
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public T GetSocketClient(string token)
        {
            T t;
            this.tokenDic.TryGetValue(token, out t);
            return t;
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