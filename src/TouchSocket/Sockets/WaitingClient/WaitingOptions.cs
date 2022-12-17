using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 等待设置
    /// </summary>
    public enum WaitingOptions
    {
        /// <summary>
        /// 发送和接收都经过适配器
        /// </summary>
        AllAdapter,

        /// <summary>
        /// 发送经过适配器，接收不经过
        /// </summary>
        SendAdapter,

        /// <summary>
        /// 发送不经过适配器，接收经过
        /// </summary>
        WaitAdapter,

        /// <summary>
        /// 全都不经过适配器。
        /// </summary>
        NoneAll
    }
}
