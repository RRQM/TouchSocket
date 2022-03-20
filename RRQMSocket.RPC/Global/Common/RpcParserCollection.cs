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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RpcParser集合
    /// </summary>
    [DebuggerDisplay("Count")]
    public class RpcParserCollection : IEnumerable<IRpcParser>
    {
        private ConcurrentDictionary<string, IRpcParser> parsers = new ConcurrentDictionary<string, IRpcParser>();

        /// <summary>
        /// 数量
        /// </summary>
        public int Count => this.parsers.Count;

        /// <summary>
        /// 获取IRpcParser
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IRpcParser this[string key] => this.parsers[key];

        /// <summary>
        /// 获取IRpcParser
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool TryGetRpcParser(string key, out IRpcParser parser)
        {
            return this.parsers.TryGetValue(key, out parser);
        }

        internal void Add(string key, IRpcParser parser)
        {
            if (this.parsers.Values.Contains(parser))
            {
                throw new RpcException("重复添加解析器");
            }

            this.parsers.TryAdd(key, parser);
        }

        internal bool TryRemove(string key, out IRpcParser parser)
        {
            return this.parsers.TryRemove(key, out parser);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.parsers.Values.GetEnumerator();
        }

        /// <summary>
        /// 返回枚举对象
        /// </summary>
        /// <returns></returns>
        IEnumerator<IRpcParser> IEnumerable<IRpcParser>.GetEnumerator()
        {
            return this.parsers.Values.GetEnumerator();
        }
    }
}