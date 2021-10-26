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
using RRQMCore;
using System.Collections.Concurrent;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 事件总线
    /// </summary>
    [EnterpriseEdition]
    public class EventBus
    {
        private ConcurrentDictionary<string, EventUnit> evebtDic = new ConcurrentDictionary<string, EventUnit>();

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventUnit"></param>
        internal void AddEvent(EventUnit eventUnit)
        {
            if (!evebtDic.TryAdd(eventUnit.EventName, eventUnit))
            {
                throw new RRQMRPCException("该事件已存在");
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventName"></param>
        internal void RemoveEvent(string eventName)
        {
            if (!evebtDic.TryRemove(eventName, out _))
            {
                throw new RRQMRPCException("不存在该事件");
            }
        }

        /// <summary>
        /// 获取事件单元
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventUnit"></param>
        /// <returns></returns>
        public bool TryGetEventUnit(string eventName, out EventUnit eventUnit)
        {
            return this.evebtDic.TryGetValue(eventName, out eventUnit);
        }
    }
}