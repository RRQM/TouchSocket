using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 收到字节数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="procotol"></param>
    /// <param name="byteBlock"></param>
    public delegate void RRQMReceivedProcotolEventHandler(object sender, short? procotol, ByteBlock  byteBlock);
}
