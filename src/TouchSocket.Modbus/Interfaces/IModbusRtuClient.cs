using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.SerialPorts;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于串口的Modbus主站接口
    /// </summary>
    public interface IModbusRtuClient:ISerialPortClient,IModbusClient
    {
    }
}
