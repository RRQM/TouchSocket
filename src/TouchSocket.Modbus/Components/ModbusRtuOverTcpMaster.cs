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

using TouchSocket.Sockets;

namespace TouchSocket.Modbus;

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
    public async Task<IModbusResponse> SendModbusRequestAsync(IModbusRequest request, CancellationToken cancellationToken)
    {
        await this.m_semaphoreSlimForRequest.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            var modbusRequest = new ModbusRtuRequest(request);

            var byteBlock = new ValueByteBlock(modbusRequest.MaxLength);
            try
            {
                modbusRequest.Build(ref byteBlock);

                await this.ProtectedSendAsync(byteBlock.Memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }

            this.m_waitDataAsync = new TaskCompletionSource<ModbusRtuResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            var response = await this.m_waitDataAsync.Task.WithCancellation(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    private TaskCompletionSource<ModbusRtuResponse> m_waitDataAsync;

    #endregion 字段

    /// <inheritdoc/>
    protected override Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is ModbusRtuResponse response)
        {
            this.m_waitDataAsync.TrySetResult(response);
        }

        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        return this.TcpConnectAsync(cancellationToken);
    }
}