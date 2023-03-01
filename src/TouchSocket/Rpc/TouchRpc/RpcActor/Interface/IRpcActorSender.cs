using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 带有协议的发送
    /// </summary>
    public interface IRpcActorSender
    {
        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void Send(short protocol, byte[] buffer, int offset, int length);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        Task SendAsync(short protocol, byte[] buffer, int offset, int length);
    }
}
