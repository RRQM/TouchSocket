using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 需要连接的RPC
    /// </summary>
   public class RPCSetting: ConnectSetting
    {
        public int Capacity { get; set; }
    }
}
