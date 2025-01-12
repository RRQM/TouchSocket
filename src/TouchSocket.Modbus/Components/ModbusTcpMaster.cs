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
        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(10);
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
        public async Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreSlim.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var waitData = this.m_waitHandlePool.GetWaitDataAsync(out var sign);
            try
            {
                var modbusTcpRequest = new ModbusTcpRequest((ushort)sign, request);

                var valueByteBlock = new ValueByteBlock(modbusTcpRequest.MaxLength);
                try
                {
                    modbusTcpRequest.Build(ref valueByteBlock);
                    await this.ProtectedSendAsync(valueByteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                finally
                {
                    valueByteBlock.Dispose();
                }

                waitData.SetCancellationToken(token);
                var waitDataStatus = await waitData.WaitAsync(millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                waitDataStatus.ThrowIfNotRunning();

                var response = waitData.WaitResult;
                response.Request = request;
                TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
                return response;
            }
            finally
            {
                this.m_waitHandlePool.Destroy(waitData);
                this.m_semaphoreSlim.Release();
            }
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnecting(ConnectingEventArgs e)
        {
            this.SetAdapter(new ModbusTcpAdapterForPoll());
            return base.OnTcpConnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is ModbusTcpResponse response)
            {
                this.m_waitHandlePool.SetRun(response);
            }
            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }
    }
}