using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 事件总线
    /// </summary>
    public class EventBus
    {
        private ConcurrentDictionary<string, EventUnit> evebtDic = new ConcurrentDictionary<string, EventUnit>();

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventUnit"></param>
        internal void AddEvent(EventUnit eventUnit)
        {
            if (!evebtDic.TryAdd(eventUnit.EventName,eventUnit))
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
        public bool TryGetEventUnit(string eventName,out EventUnit eventUnit)
        {
            return this.evebtDic.TryGetValue(eventName,out eventUnit);
        }
    }
}
