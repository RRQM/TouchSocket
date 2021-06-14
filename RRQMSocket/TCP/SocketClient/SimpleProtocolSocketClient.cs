using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// SimpleProtocolSocketClient
    /// </summary>
    public sealed class SimpleProtocolSocketClient : ProtocolSocketClient
    {
        /// <summary>
        /// 收到消息
        /// </summary>
        internal Action<SimpleProtocolSocketClient,short?, ByteBlock> OnReceived;

        /// <summary>
        /// 处理协议数据
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected override void HandleProtocolData(short? procotol, ByteBlock byteBlock)
        {
            this.OnReceived.Invoke(this,procotol,byteBlock);
        }
    }
}
