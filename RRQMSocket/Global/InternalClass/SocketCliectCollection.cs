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
using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 客户端集合
    /// </summary>
    public class SocketCliectCollection<T> : List<T>where T: TcpSocketClient
    {
        internal new void Add(T socketClient)
        {
            base.Add(socketClient);
        }

        internal new void AddRange(IEnumerable<T> socketClients)
        {
            base.AddRange(socketClients);
        }

        internal new void Remove(T socketClient)
        {
            base.Remove(socketClient);
        }

        internal new void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        internal new void Clear()
        {
            base.Clear();
        }
    }
}