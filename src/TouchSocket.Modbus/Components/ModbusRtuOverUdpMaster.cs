//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// 基于Udp协议，且使用Rtu格式的Modbus主站
    /// </summary>
    public class ModbusRtuOverUdpMaster : UdpSessionBase, IModbusRtuOverUdpMaster
    {
        /// <summary>
        /// 基于Udp协议，且使用Rtu格式的Modbus主站
        /// </summary>
        public ModbusRtuOverUdpMaster()
        {
            this.Protocol = TouchSocketModbusUtility.ModbusRtuOverUdp;
        }

        #region 字段

        private readonly SemaphoreSlim m_semaphoreSlimForRequest = new SemaphoreSlim(1, 1);
        private readonly WaitData<ModbusRtuResponse> m_waitData = new WaitData<ModbusRtuResponse>();
        private readonly WaitDataAsync<ModbusRtuResponse> m_waitDataAsync = new WaitDataAsync<ModbusRtuResponse>();

        #endregion 字段

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.SetAdapter(new ModbusUdpRtuAdapter());
            base.LoadConfig(config);
        }

        ///// <inheritdoc/>
        //public IModbusResponse 123SendModbusRequest(ModbusRequest request, int millisecondsTimeout, CancellationToken token)
        //{
        //    this.m_semaphoreSlimForRequest.WaitTime(millisecondsTimeout, token);
        //    try
        //    {
        //        var modbusTcpRequest = new ModbusRtuRequest(request);

        //        this.ProtectedSend(modbusTcpRequest);
        //        this.m_waitData.SetCancellationToken(token);
        //        var waitDataStatus = this.m_waitData.Wait(millisecondsTimeout);
        //        waitDataStatus.ThrowIfNotRunning();

        //        var response = this.m_waitData.WaitResult;
        //        TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
        //        return response;
        //    }
        //    finally
        //    {
        //        this.m_semaphoreSlimForRequest.Release();
        //    }
        //}

        /// <inheritdoc/>
        public async Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreSlimForRequest.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);

            try
            {
                var modbusTcpRequest = new ModbusRtuRequest(request);

                await this.ProtectedSendAsync(modbusTcpRequest).ConfigureAwait(false);
                this.m_waitDataAsync.SetCancellationToken(token);
                var waitDataStatus = await this.m_waitDataAsync.WaitAsync(millisecondsTimeout).ConfigureAwait(false);
                waitDataStatus.ThrowIfNotRunning();

                var response = this.m_waitData.WaitResult;
                TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
                return response;
            }
            finally
            {
                this.m_semaphoreSlimForRequest.Release();
            }
        }

        /// <inheritdoc/>
        protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
        {
            if (e.RequestInfo is ModbusRtuResponse response)
            {
                this.SetRun(response);
            }
            await base.OnUdpReceived(e).ConfigureAwait(false);
        }

        private void SetRun(ModbusRtuResponse response)
        {
            this.m_waitData.Set(response);
            this.m_waitDataAsync.Set(response);
        }
    }
}