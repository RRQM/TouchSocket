using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于Udp协议的Modbus
    /// </summary>
    public class ModbusUdpClient : UdpSessionBase, IModbusUdpClient
    {
        private readonly WaitHandlePool<ModbusTcpResponse> m_waitHandlePool;

        /// <summary>
        /// 初始化一个基于Udp协议的Modbus
        /// </summary>
        public ModbusUdpClient()
        {
            this.m_waitHandlePool = new WaitHandlePool<ModbusTcpResponse>()
            {
                MaxSign = ushort.MaxValue
            };
        }

        /// <inheritdoc/>
        public override bool CanSetDataHandlingAdapter => false;

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.SetAdapter(new ModbusUdpAdapter());
            base.LoadConfig(config);
        }

        /// <inheritdoc/>
        public IModbusResponse SendModbusRequest(ModbusRequest request, int timeout, CancellationToken token)
        {
            var waitData = this.m_waitHandlePool.GetWaitData(out var sign);
            var modbusTcpRequest = new ModbusTcpRequest((ushort)sign, request);

            this.Send(modbusTcpRequest);
            waitData.SetCancellationToken(token);
            var waitDataStatus = waitData.Wait(timeout);
            waitDataStatus.ThrowIfNotRunning();

            var response = waitData.WaitResult;
            SRHelper.ThrowIfNotSuccess(response.ErrorCode);
            return response;
        }

        /// <inheritdoc/>
        public async Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int timeout, CancellationToken token)
        {
            var waitData = this.m_waitHandlePool.GetWaitDataAsync(out var sign);
            var modbusTcpRequest = new ModbusTcpRequest((ushort)sign, request);

            this.Send(modbusTcpRequest);
            waitData.SetCancellationToken(token);
            var waitDataStatus = await waitData.WaitAsync(timeout);
            waitDataStatus.ThrowIfNotRunning();

            var response = waitData.WaitResult;
            SRHelper.ThrowIfNotSuccess(response.ErrorCode);
            return response;
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(UdpReceivedDataEventArgs e)
        {
            if (e.RequestInfo is ModbusTcpResponse response)
            {
                this.m_waitHandlePool.SetRun(response);
            }
            await base.ReceivedData(e);
        }
    }
}
