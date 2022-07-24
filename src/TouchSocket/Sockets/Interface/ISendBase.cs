using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有发送动作的基类。
    /// </summary>
    public interface ISendBase
    {
        /// <summary>
        /// 表示对象能否顺利执行发送操作。
        /// <para>由于高并发，当改值为Tru时，也不一定完全能执行。</para>
        /// </summary>
        bool CanSend { get; }
    }
}
