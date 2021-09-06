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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RRQMCore.ByteManager
{
    /// <summary>
    /// 字节块集合字典索引。
    /// </summary>
    public class BytesDictionary
    {
        internal BytesDictionary()
        {
            this.bytesDic = new ConcurrentDictionary<long, BytesCollection>();
        }

        private ConcurrentDictionary<long, BytesCollection> bytesDic;

        internal ICollection<long> Keys { get { return this.bytesDic.Keys; } }

        internal bool ContainsKey(long key)
        {
            return bytesDic.ContainsKey(key);
        }

        internal bool TryGet(long key, out BytesCollection bytesCollection)
        {
            return bytesDic.TryGetValue(key, out bytesCollection);
        }

        internal bool TryAdd(long key, BytesCollection bytesCollection)
        {
            return this.bytesDic.TryAdd(key, bytesCollection);
        }
    }
}