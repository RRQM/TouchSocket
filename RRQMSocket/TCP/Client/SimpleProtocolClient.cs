using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议客户端
    /// </summary>
    public class SimpleProtocolClient : ProtocolClient
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public event Action<short, ByteBlock> Received;

        /// <summary>
        /// 处理协议数据
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="byteBlock"></param>
        protected sealed override void HandleProtocolData(short agreement, ByteBlock byteBlock)
        {
            this.Received?.Invoke(agreement, byteBlock);
        }
    }
}
