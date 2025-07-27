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
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus;

/// <summary>
/// 基于串口的Modbus主站接口
/// </summary>
public class ModbusRtuMaster : SerialPortClientBase, IModbusRtuMaster
{
    private ModbusRequest m_modbusRequest;

    /// <summary>
    /// 基于串口的Modbus主站接口
    /// </summary>
    public ModbusRtuMaster()
    {
        this.Protocol = TouchSocketModbusUtility.ModbusRtu;
    }

    /// <inheritdoc/>
    public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        return base.SerialPortConnectAsync(millisecondsTimeout, token);
    }

    /// <inheritdoc/>
    public async Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int millisecondsTimeout, CancellationToken token)
    {
        await this.m_semaphoreSlimForRequest.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            this.m_modbusRequest = request;
            var modbusRequest = new ModbusRtuRequest(request);
            var byteBlock = new ValueByteBlock(modbusRequest.MaxLength);
            try
            {
                modbusRequest.Build(ref byteBlock);
                await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }

            this.m_waitDataAsync.Reset();
            this.m_waitDataAsync.SetCancellationToken(token);
            var waitDataStatus = await this.m_waitDataAsync.WaitAsync(millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();

            var response = this.m_waitDataAsync.WaitResult;
            TouchSocketModbusThrowHelper.ThrowIfNotSuccess(response.ErrorCode);

            response.Request = request;
            return response;
        }
        finally
        {
            this.m_modbusRequest = default;
            this.m_semaphoreSlimForRequest.Release();
        }
    }

    /// <inheritdoc/>
    protected override async Task OnSerialConnecting(ConnectingEventArgs e)
    {
        this.SetAdapter(new ModbusRtuAdapter());
        await base.OnSerialConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #region 字段

    private readonly SemaphoreSlim m_semaphoreSlimForRequest = new SemaphoreSlim(1, 1);
    private readonly WaitDataAsync<ModbusRtuResponse> m_waitDataAsync = new WaitDataAsync<ModbusRtuResponse>();

    #endregion 字段

    /// <inheritdoc/>
    protected override async Task OnSerialReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is ModbusRtuResponse response)
        {
            var result = this.SetRun(this.m_modbusRequest, response);
            if (result)
            {
                this.m_waitDataAsync.Set(response);
            }
        }
        await base.OnSerialReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 验证Modbus请求和响应是否匹配
    /// </summary>
    /// <param name="modbusRequest">Modbus请求</param>
    /// <param name="response">Modbus响应</param>
    /// <returns>如果请求和响应匹配则返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    protected virtual bool SetRun(IModbusRequest modbusRequest, IModbusResponse response)
    {
        if (modbusRequest.SlaveId != response.SlaveId)
        {
            return false;
        }

        if (modbusRequest.FunctionCode != response.FunctionCode)
        {
            return false;
        }
        return true;
    }
}