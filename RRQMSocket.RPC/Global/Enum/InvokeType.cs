using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 调用类型
    /// </summary>
    public enum InvokeType : byte
    {
        /// <summary>
        /// 全局实例
        /// </summary>
        GlobalInstance,

        /// <summary>
        /// 自定义实例
        /// </summary>
        CustomInstance,

        /// <summary>
        /// 每次调用都是新实例
        /// </summary>
        NewInstance
    }
}
