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
    /// 基于Tcp协议的Modbus客户端
    /// </summary>
    public class ModbusTcpMaster : TcpClientBase, IModbusTcpMaster
    {
        private readonly WaitHandlePool<ModbusTcpResponse> m_waitHandlePool;

        /// <summary>
        /// 基于Tcp协议的Modbus客户端
        /// </summary>
        public ModbusTcpMaster()
        {
            this.Protocol = TouchSocketModbusUtility.ModbusTcp;
            this.m_waitHandlePool = new WaitHandlePool<ModbusTcpResponse>()
            {
                MaxSign = ushort.MaxValue
            };
        }

        /// <inheritdoc/>
        public override bool CanSetDataHandlingAdapter => false;

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
            TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
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
            TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
            return response;
        }

        /// <inheritdoc/>
        protected override Task OnConnecting(ConnectingEventArgs e)
        {
            this.SetAdapter(new ModbusTcpAdapterForPoll());
            return base.OnConnecting(e);
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