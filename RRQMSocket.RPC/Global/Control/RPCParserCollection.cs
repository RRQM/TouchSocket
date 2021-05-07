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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPCParser集合
    /// </summary>
    [DebuggerDisplay("Count")]
    public class RPCParserCollection : IEnumerable<RPCParser>
    {
        private Dictionary<string, RPCParser> parsers = new Dictionary<string, RPCParser>();

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get { return parsers.Count; } }

        /// <summary>
        /// 获取IRPCParser
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RPCParser this[string key] { get { return this.GetRPCParser(key); } }

        /// <summary>
        /// 获取IRPCParser
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RPCParser GetRPCParser(string key)
        {
            if (this.parsers.ContainsKey(key))
            {
                return this.parsers[key];
            }
            return null;
        }

        internal void Add(string key, RPCParser parser)
        {
            if (this.parsers.Values.Contains(parser))
            {
                throw new RRQMRPCException("重复添加解析器");
            }

            this.parsers.Add(key, parser);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.parsers.Values.GetEnumerator();
        }

        /// <summary>
        /// 返回枚举对象
        /// </summary>
        /// <returns></returns>
        IEnumerator<RPCParser> IEnumerable<RPCParser>.GetEnumerator()
        {
            return this.parsers.Values.GetEnumerator();
        }
    }
}