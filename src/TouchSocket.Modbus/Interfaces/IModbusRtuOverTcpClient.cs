using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于Tcp协议，且使用Rtu数据格式的Modbus主站接口
    /// </summary>
    public interface IModbusRtuOverTcpClient: IModbusTcpClient
    {
    }
}
