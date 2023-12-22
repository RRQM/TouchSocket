using TouchSocket.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于Tcp协议的Modbus客户端
    /// </summary>
    public class ModbusTcpClient : TcpClientBase,IModbusTcpClient
    {
        private readonly WaitHandlePool<ModbusTcpResponse> m_waitHandlePool;

        /// <summary>
        /// 初始化一个基于Tcp协议的Modbus客户端
        /// </summary>
        public ModbusTcpClient()
        {
            this.m_waitHandlePool = new WaitHandlePool<ModbusTcpResponse>()
            {
                MaxSign = ushort.MaxValue
            };
        }

        /// <inheritdoc/>
        public override bool CanSetDataHandlingAdapter => false;

        /// <inheritdoc/>
        protected override Task OnConnecting(ConnectingEventArgs e)
        {
            this.SetAdapter(new ModbusTcpAdapterForPoll());
            return base.OnConnecting(e);
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
            var waitDataStatus =await waitData.WaitAsync(timeout);
            waitDataStatus.ThrowIfNotRunning();

            var response = waitData.WaitResult;
            SRHelper.ThrowIfNotSuccess(response.ErrorCode);
            return response;
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is ModbusTcpResponse response)
            {
                this.m_waitHandlePool.SetRun(response);
            }
            await base.ReceivedData(e);
        }
    }
}