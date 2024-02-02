using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IReceiverObject
    /// </summary>
    public interface IReceiverObject
    {
        /// <summary>
        /// 获取一个同步数据接收器
        /// </summary>
        /// <returns></returns>
        IReceiver CreateReceiver();

        /// <summary>
        /// 移除同步数据接收器
        /// </summary>
        void ClearReceiver();

    }
}
