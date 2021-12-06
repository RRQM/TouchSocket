using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 接收类型
    /// </summary>
    public enum ReceiveType : byte
    {
        /// <summary>
        /// 完成端口
        /// </summary>
        IOCP,

        /// <summary>
        /// 独立线程阻塞
        /// </summary>
        BIO,

        /// <summary>
        /// 网络流模式，接收、发送方式由IOCP切换为NetworkStream.Read/Write
        /// 同时服务器超时清理将变得不可用，且适配也将失效。
        /// </summary>
        NetworkStream
    }
}
