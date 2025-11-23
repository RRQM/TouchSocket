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
    private TaskCompletionSource<ModbusRtuResponse> m_waitDataAsync;

    #endregion 字段

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        this.SetAdapter(new ModbusUdpRtuAdapter());
        base.LoadConfig(config);
    }

    /// <inheritdoc/>
    public async Task<IModbusResponse> SendModbusRequestAsync(IModbusRequest request, CancellationToken cancellationToken)
    {
        await this.m_semaphoreSlimForRequest.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            var modbusTcpRequest = new ModbusRtuRequest(request);

            await this.ProtectedSendAsync(modbusTcpRequest, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

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
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        if (e.RequestInfo is ModbusRtuResponse response)
        {
            this.m_waitDataAsync?.TrySetResult(response);
        }
        await base.OnUdpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}