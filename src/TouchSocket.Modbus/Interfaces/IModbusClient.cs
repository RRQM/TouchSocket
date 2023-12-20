using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 提供Modbus的操作接口
    /// </summary>
    public interface IModbusClient
    {
        /// <summary>
        /// 发送Modbus请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IModbusResponse SendModbusRequest(ModbusRequest request, int timeout, CancellationToken token);

        /// <summary>
        /// 异步发送Modbus请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int timeout, CancellationToken token);
    }
}
