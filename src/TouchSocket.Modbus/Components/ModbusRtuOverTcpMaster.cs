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
    /// 基于Tcp协议，且使用Rtu数据格式的Modbus主站接口
    /// </summary>
    public class ModbusRtuOverTcpMaster : TcpClientBase, IModbusRtuOverTcpMaster
    {
        /// <summary>
        /// 基于Tcp协议，且使用Rtu数据格式的Modbus主站接口
        /// </summary>
        public ModbusRtuOverTcpMaster()
        {
            this.Protocol = TouchSocketModbusUtility.ModbusRtuOverTcp;
        }


        /// <inheritdoc/>
        public async Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreSlimForRequest.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            try
            {
                var modbusRequest = new ModbusRtuRequest(request);

                var byteBlock = new ValueByteBlock(modbusRequest.MaxLength);
                try
                {
                    modbusRequest.Build(ref byteBlock);

                    await this.ProtectedSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                finally
                {
                    byteBlock.Dispose();
                }

                this.m_waitDataAsync.SetCancellationToken(token);
                var waitDataStatus = await this.m_waitDataAsync.WaitAsync(millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                waitDataStatus.ThrowIfNotRunning();

                var response = this.m_waitData.WaitResult;
                response.Request = request;
                TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);
                return response;
            }
            finally
            {
                this.m_semaphoreSlimForRequest.Release();
            }
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnecting(ConnectingEventArgs e)
        {
            this.SetAdapter(new ModbusRtuAdapter());
            return base.OnTcpConnecting(e);
        }

        #region 字段

        private readonly SemaphoreSlim m_semaphoreSlimForRequest = new SemaphoreSlim(1, 1);
        private readonly WaitData<ModbusRtuResponse> m_waitData = new WaitData<ModbusRtuResponse>();
        private readonly WaitDataAsync<ModbusRtuResponse> m_waitDataAsync = new WaitDataAsync<ModbusRtuResponse>();

        #endregion 字段

        /// <inheritdoc/>
        protected override Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is ModbusRtuResponse response)
            {
                this.SetRun(response);
            }

            return EasyTask.CompletedTask;
        }

        private void SetRun(ModbusRtuResponse response)
        {
            this.m_waitData.Set(response);
            this.m_waitDataAsync.Set(response);
        }

        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }
    }
}