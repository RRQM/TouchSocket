using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 简单Token客户端
    /// </summary>
    public class SimpleTokenClient : TokenClient
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public event Action<ByteBlock, object> Received;

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            OnReceived(byteBlock, obj);
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected virtual void OnReceived(ByteBlock byteBlock, object obj)
        {
            this.Received?.Invoke(byteBlock, obj);
        }
    }
}
