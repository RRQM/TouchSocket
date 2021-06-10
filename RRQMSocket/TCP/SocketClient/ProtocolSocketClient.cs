using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 协议辅助类
    /// </summary>
    public abstract class ProtocolSocketClient : SocketClient
    {
        /// <summary>
        /// 新建
        /// </summary>
        public sealed override void Create()
        {
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
        }

        /// <summary>
        /// 收到消息
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            int agreement = BitConverter.ToInt32(byteBlock.Buffer, 0);
        }

    }
}
