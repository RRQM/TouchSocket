//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
using RRQMCore.Concurrent;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 传输集合
    /// </summary>
    public class TransferCollection: IEnumerable<UrlFileInfo>
    {
        internal TransferCollection()
        {
            list = new List<UrlFileInfo>(); 
        }
        private List<UrlFileInfo> list;

        /// <summary>
        /// 返回一个循环访问集合的枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<UrlFileInfo> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        internal void Add(UrlFileInfo fileInfo)
        {
            this.list.Add(fileInfo);
        }

        internal bool GetFirst(out UrlFileInfo fileInfo)
        {
            lock (this)
            {
                if (this.list.Count>0)
                {
                    fileInfo = this.list[0];
                    this.list.RemoveAt(0);
                    return true;
                }
                fileInfo = null;
                return false;
            }
        }
    }
}