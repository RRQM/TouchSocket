using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 简单TokenSocketClient
    /// </summary>
    public class SimpleTokenSocketClient : TokenSocketClient
    {
        /// <summary>
        /// 收到消息
        /// </summary>
        public event RRQMReceivedEventHandler<SimpleTokenSocketClient> Received;

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override sealed void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            this.Received.Invoke(this, byteBlock, obj);
        }
    }
}
