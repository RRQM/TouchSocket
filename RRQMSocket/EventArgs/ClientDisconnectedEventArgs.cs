using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 断开连接事件参数
    /// </summary>
    public class ClientDisconnectedEventArgs:MesEventArgs
    {
        private bool manual;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="mes"></param>
        public ClientDisconnectedEventArgs(bool manual,string mes):base(mes)
        {
            this.manual = manual;
        }

        /// <summary>
        /// 是否为主动行为。
        /// </summary>
        public bool Manual
        {
            get { return manual; }
        }

    }
}
