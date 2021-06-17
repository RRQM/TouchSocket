using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 事件单元
    /// </summary>
    public class EventUnit
    {
        internal EventUnit()
        {
            this.Subscribers = new ReadOnlyList<string>();
        }
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get;internal set; }

        /// <summary>
        /// 发布者
        /// </summary>
        public string Publisher { get; internal set; }

        /// <summary>
        /// 委托类型名称
        /// </summary>
        public string ParameterTypes { get; internal set; }

        /// <summary>
        /// 订阅者
        /// </summary>
        public ReadOnlyList<string> Subscribers { get; internal set; }
    }
}
