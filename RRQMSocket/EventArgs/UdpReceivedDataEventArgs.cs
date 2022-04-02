using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// Udp接收消息
    /// </summary>
    public class UdpReceivedDataEventArgs : ReceivedDataEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        public UdpReceivedDataEventArgs(EndPoint endPoint,ByteBlock byteBlock, IRequestInfo requestInfo) :base(byteBlock,requestInfo)
        {
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// 接收终结点
        /// </summary>
        public EndPoint EndPoint { get; }
    }
}
