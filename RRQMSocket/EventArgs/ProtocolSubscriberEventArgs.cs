using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议订阅事件
    /// </summary>
    public class ProtocolSubscriberEventArgs : ByteBlockEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="byteBlock"></param>
        public ProtocolSubscriberEventArgs(ByteBlock byteBlock) :base(byteBlock)
        { 
        
        }

        /// <summary>
        /// 标识该数据已被处理。
        /// </summary>
        public bool Handled { get; set; }
    }
}
