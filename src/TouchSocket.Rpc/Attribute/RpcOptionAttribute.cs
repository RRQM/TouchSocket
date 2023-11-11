using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 选项类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class RpcOptionAttribute : Attribute
    {
        /// <summary>
        /// 调用配置
        /// </summary>
        public IInvokeOption Option { get; set; }

        /// <summary>
        /// 调用超时
        /// </summary>
        public int Timeout { get; set; }

        public RpcOptionAttribute(IInvokeOption option)
        {
            Option = option;
            Timeout = 5000;
        }

        public RpcOptionAttribute(IInvokeOption option, int timeout)
        {
            Option = option;
            Timeout = timeout;
        }

    }
}
