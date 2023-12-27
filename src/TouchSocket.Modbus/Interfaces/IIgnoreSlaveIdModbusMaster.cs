using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 可以使用忽略SlaveId的限定接口
    /// </summary>
    public interface IIgnoreSlaveIdModbusMaster:IModbusMaster
    {
    }
}
