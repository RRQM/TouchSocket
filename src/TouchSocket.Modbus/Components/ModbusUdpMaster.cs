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
    /// 基于Udp协议的Modbus
    /// </summary>
    public class ModbusUdpMaster : UdpSessionBase, IModbusUdpMaster
    {
        private readonly WaitHandlePool<ModbusTcpResponse> m_waitHandlePool;

        /// <summary>
        /// 基于Udp协议的Modbus
        /// </summary>
        public ModbusUdpMaster()
        {
            this.Protocol = TouchSocketModbusUtility.ModbusUdp;
            this.m_waitHandlePool = new WaitHandlePool<ModbusTcpResponse>()
            {
                MaxSign = ushort.MaxValue
            };
        }

        ///// <inheritdoc/>
        //public IModbusResponse 123SendModbusRequest(ModbusRequest request, int millisecondsTimeout, CancellationToken token)
        //{
        //    var waitData = this.m_waitHandlePool.GetWaitData(out var sign);
        //    try
        //    {
        //        var modbusTcpRequest = new ModbusTcpRequest((ushort)sign, request);

        //        this.ProtectedSend(modbusTcpRequest);
        //        waitData.SetCancellationToken(token);
        //        var waitDataStatus = waitData.Wait(millisecondsTimeout);
        //        waitDataStatus.ThrowIfNotRunning();

        //        var response = waitData.WaitResult;
        //        TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
        //        return response;
        //    }
        //    finally
        //    {
        //        this.m_waitHandlePool.Destroy(waitData);
        //    }
        //}

        /// <inheritdoc/>
        public async Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int millisecondsTimeout, CancellationToken token)
        {
            var waitData = this.m_waitHandlePool.GetWaitDataAsync(out var sign);
            try
            {
                var modbusTcpRequest = new ModbusTcpRequest((ushort)sign, request);

                await this.ProtectedSendAsync(modbusTcpRequest).ConfigureFalseAwait();
                waitData.SetCancellationToken(token);
                var waitDataStatus = await waitData.WaitAsync(millisecondsTimeout).ConfigureFalseAwait();
                waitDataStatus.ThrowIfNotRunning();

                var response = waitData.WaitResult;
                TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
                return response;
            }
            finally
            {
                this.m_waitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.SetAdapter(new ModbusUdpAdapter());
            base.LoadConfig(config);
        }

        /// <inheritdoc/>
        protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
        {
            if (e.RequestInfo is ModbusTcpResponse response)
            {
                this.m_waitHandlePool.SetRun(response);
            }
            await base.OnUdpReceived(e).ConfigureFalseAwait();
        }
    }
}