//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于串口的Modbus主站接口
    /// </summary>
    public class ModbusRtuMaster : SerialPortClientBase, IModbusRtuMaster
    {
        /// <summary>
        /// 基于串口的Modbus主站接口
        /// </summary>
        public ModbusRtuMaster()
        {
            this.Protocol = TouchSocketModbusUtility.ModbusRtu;     
        }
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
                TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
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
                TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
                return response;
            }
            finally
            {
                m_semaphoreSlimForRequest.Release();
            }
        }

        /// <inheritdoc/>
        protected override Task OnConnecting(SerialConnectingEventArgs e)
        {
            //this.SetAdapter(new ModbusRtuAdapter());
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