using RRQMCore.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// Token连接验证
    /// </summary>
    public class VerifyOption
    {
        /// <summary>
        /// 令箭
        /// </summary>
        public string Token { get;internal set; }

        /// <summary>
        /// 是否接受
        /// </summary>
        public bool Accept { get; set; }

        /// <summary>
        /// 不接受时，返回客户端信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 标记，会同步至TcpSocketClient
        /// </summary>
        public object Flag { get; set; }
    }
}
