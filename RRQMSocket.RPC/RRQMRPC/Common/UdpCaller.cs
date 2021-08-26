using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// Udp调用者
    /// </summary>
    public class UdpCaller : ICaller
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="service"></param>
        /// <param name="callerEndPoint"></param>
        public UdpCaller(UdpSession service, EndPoint callerEndPoint)
        {
            this.service = service;
            this.callerEndPoint = callerEndPoint;
        }

        private UdpSession service;

        /// <summary>
        /// Udp服务器
        /// </summary>
        public UdpSession Service
        {
            get { return service; }
        }

        private EndPoint callerEndPoint;

        /// <summary>
        /// 调用者终结点
        /// </summary>
        public EndPoint CallerEndPoint
        {
            get { return callerEndPoint; }
        }

    }
}
