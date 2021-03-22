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
    /// 若汝棋茗内置UDP会话
    /// </summary>
    public sealed class RRQMUdpSession : UdpSession
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMUdpSession() : base(new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20))
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool"></param>
        public RRQMUdpSession(BytePool bytePool):base(bytePool)
        {
          
        }
        /// <summary>
        /// 当收到数据时
        /// </summary>
        public event RRQMUDPByteBlockEventHandler OnReceivedData;

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            OnReceivedData?.Invoke(remoteEndPoint, byteBlock);
        }
    }
}
