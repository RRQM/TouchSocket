using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// MsgEventArgs
    /// </summary>
    public class MsgEventArgs: PluginEventArgs
    {
        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="mes"></param>
        public MsgEventArgs(string mes)
        {
            this.Message = mes;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MsgEventArgs()
        {
        }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
