using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Sockets;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于Tcp协议，且使用Rtu数据格式的Modbus主站接口
    /// </summary>
    public class ModbusRtuOverTcpClient : TcpClientBase, IModbusRtuOverTcpClient
    {
        /// <inheritdoc/>
        public override bool CanSetDataHandlingAdapter => false;

        /// <inheritdoc/>
        public IModbusResponse SendModbusRequest(ModbusRequest request, int timeout, CancellationToken token)
        {
            try
            {
                this.m_semaphoreSlimForRequest.Wait(timeout, token);
                var modbusTcpRequest = new ModbusRtuRequest(request);

                this.Send(modbusTcpRequest);
                this.m_waitData.SetCancellationToken(token);
                var waitDataStatus = this.m_waitData.Wait(timeout);
                waitDataStatus.ThrowIfNotRunning();

                var response = m_waitData.WaitResult;
                SRHelper.ThrowIfNotSuccess(response.GetErrorCode());
                return response;
            }
            finally
            {
                m_semaphoreSlimForRequest.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int timeout, CancellationToken token)
        {
            try
            {
                await this.m_semaphoreSlimForRequest.WaitAsync(timeout, token);

                var modbusTcpRequest = new ModbusRtuRequest(request);

                this.Send(modbusTcpRequest);
                this.m_waitDataAsync.SetCancellationToken(token);
                var waitDataStatus = await this.m_waitDataAsync.WaitAsync(timeout);
                waitDataStatus.ThrowIfNotRunning();

                var response = this.m_waitData.WaitResult;
                SRHelper.ThrowIfNotSuccess(response.GetErrorCode());
                return response;
            }
            finally
            {
                m_semaphoreSlimForRequest.Release();
            }
        }

        /// <inheritdoc/>
        protected override Task OnConnecting(ConnectingEventArgs e)
        {
            this.SetAdapter(new ModbusRtuAdapter2());
            return base.OnConnecting(e);
        }

        #region 字段

        private readonly SemaphoreSlim m_semaphoreSlimForRequest = new SemaphoreSlim(1, 1);
        private WaitData<ModbusRtuResponse> m_waitData = new WaitData<ModbusRtuResponse>();
        private WaitDataAsync<ModbusRtuResponse> m_waitDataAsync = new WaitDataAsync<ModbusRtuResponse>();

        #endregion 字段

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is ModbusRtuResponse response)
            {
                this.SetRun(response);
            }
            await base.ReceivedData(e);
        }

        private void SetRun(ModbusRtuResponse response)
        {
            this.m_waitData.Set(response);
            this.m_waitDataAsync.Set(response);
        }
    }
}
