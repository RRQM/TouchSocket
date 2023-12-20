using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于Udp协议，且使用Rtu格式的Modbus主站接口
    /// </summary>
    public interface IModbusRtuOverUdpClient:IModbusUdpClient
    {
    }
}
