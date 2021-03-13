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

namespace RRQMSocket
{
    /// <summary>
    /// 客户端集合
    /// </summary>
    [DebuggerDisplay("Count={Count}")]
    public class SocketCliectCollection<T> : IEnumerable<T> where T : TcpSocketClient
    {
        internal SocketCliectCollection()
        {
            //this.tokenString = tokenString;
        }

        /// <summary>
        /// 获取或设置分配IDToken的格式
        /// </summary>
        public string IDTokenFormat { get; set; }

        private int number;

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get { return this.tokenDic.Count; } }

        private ConcurrentDictionary<string, T> tokenDic = new ConcurrentDictionary<string, T>();

        internal bool Add(T socketClient, bool reBulid)
        {
            if (reBulid)
            {
                number++;
                socketClient.IDToken = string.Format(IDTokenFormat, number);
                return this.tokenDic.TryAdd(socketClient.IDToken, socketClient);
            }
            else
            {
                return this.tokenDic.TryAdd(socketClient.IDToken, socketClient);
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

        private T GetSocketClient(string iDToken)
        {
            T t;
            this.tokenDic.TryGetValue(iDToken, out t);
            return t;
        }

        /// <summary>
        /// 获取SocketClient
        /// </summary>
        /// <param name="iDToken"></param>
        /// <returns></returns>
        public T this[string iDToken] { get { return this.GetSocketClient(iDToken); } }

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