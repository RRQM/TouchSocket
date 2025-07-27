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

using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.SerialPorts;

/// <summary>
/// 串口客户端基类
/// </summary>
public abstract class SerialPortClientBase : SetupConfigObject, ISerialPortSession
{
    /// <summary>
    /// 串口客户端基类
    /// </summary>
    public SerialPortClientBase()
    {
        this.Protocol = SerialPortUtility.SerialPort;
    }

    #region 变量

    private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private bool m_online;
    private InternalReceiver m_receiver;
    private Task m_runTask;
    private SerialPortTransport m_transport;

    #endregion 变量

    #region 事件

    /// <summary>
    /// 断开连接。在客户端未设置连接状态时，不会触发
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnSerialClosed(ClosedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(ISerialClosedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnSerialClosing(ClosingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(ISerialClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 已经建立连接
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnSerialConnected(ConnectedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(ISerialConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 准备连接的时候，此时并未建立连接
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnSerialConnecting(ConnectingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(ISerialConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当收到适配器处理的数据时。
    /// </summary>
    /// <param name="e"></param>
    /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
    protected virtual async Task OnSerialReceived(ReceivedDataEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(ISerialReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当收到原始数据时
    /// </summary>
    /// <param name="byteBlock">包含接收数据的字节块</param>
    /// <returns>
    /// 如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。
    /// 返回<see langword="false"/>则表示数据未被处理，可能会继续向下传递。
    /// </returns>
    protected virtual ValueTask<bool> OnSerialReceiving(IByteBlockReader byteBlock)
    {
        return this.PluginManager.RaiseAsync(typeof(ISerialReceivingPlugin), this.Resolver, this, new ByteBlockEventArgs(byteBlock));
    }

    /// <summary>
    /// 在序列化发送前调用的虚拟方法。
    /// </summary>
    /// <param name="memory">待发送的字节序列。</param>
    /// <returns>一个表示操作结果的<see cref="ValueTask{T}"/>，始终返回 true。</returns>
    /// <remarks>
    /// 此方法的存在是为了提供一个扩展点，允许在序列化数据发送之前进行自定义处理。
    /// 默认实现直接返回 true，表示默认情况下允许序列化发送继续进行。
    /// </remarks>
    protected virtual ValueTask<bool> OnSerialSending(ReadOnlyMemory<byte> memory)
    {
        return this.PluginManager.RaiseAsync(typeof(ISerialSendingPlugin), this.Resolver, this, new SendingEventArgs(memory));
    }

    private async Task PrivateConnected(SerialPortTransport transport)
    {
        var receiveTask = EasyTask.SafeRun(this.ReceiveLoopAsync, transport);

        var e_connected = new ConnectedEventArgs();
        await this.OnSerialConnected(e_connected).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await receiveTask.SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var e_closed = transport.ClosedEventArgs;
        this.m_online = false;
        var adapter = this.m_dataHandlingAdapter;
        this.m_dataHandlingAdapter = default;
        adapter.SafeDispose();

        await this.OnSerialClosed(e_closed).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task PrivateOnClosing(ClosingEventArgs e)
    {
        return this.OnSerialClosing(e);
    }

    private async Task PrivateOnSerialConnecting(ConnectingEventArgs e)
    {
        await this.OnSerialConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (this.m_dataHandlingAdapter == null)
        {
            var adapter = this.Config.GetValue(SerialPortConfigExtension.SerialDataHandlingAdapterProperty)?.Invoke();
            if (adapter != null)
            {
                this.SetAdapter(adapter);
            }
        }
    }

    #endregion 事件

    #region 属性

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_transport == null ? new CancellationToken(true) : this.m_transport.ClosedToken;

    /// <inheritdoc/>
    public bool IsClient => true;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_transport?.ReceiveCounter.LastIncrement ?? default;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_transport?.SendCounter.LastIncrement ?? default;

    /// <inheritdoc/>
    public bool Online => this.m_online && this.m_transport != null && this.m_transport.IsOpen;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    /// <inheritdoc/>
    protected SingleStreamDataHandlingAdapter ProtectedDataHandlingAdapter => this.m_dataHandlingAdapter;

    #endregion 属性

    #region 断开操作

    /// <inheritdoc/>
    public virtual async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            if (!this.m_online)
            {
                return Result.Success;
            }

            await this.PrivateOnClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var transport = this.m_transport;
            if (transport != null)
            {
                await transport.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            await this.WaitClearConnect().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            _ = EasyTask.SafeRun(async () => await this.CloseAsync(TouchSocketResource.DisposeClose).ConfigureAwait(EasyTask.ContinueOnCapturedContext));
        }
        base.SafetyDispose(disposing);
    }

    #endregion 断开操作

    #region Connect

    /// <summary>
    /// 异步连接到串口设备。
    /// </summary>
    /// <param name="millisecondsTimeout">连接操作的超时时间（以毫秒为单位）。</param>
    /// <param name="token">用于取消操作的 <see cref="CancellationToken"/>。</param>
    /// <returns>表示异步操作的任务。</returns>
    protected async Task SerialPortConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        this.ThrowIfDisposed();
        this.ThrowIfConfigIsNull();
        await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            if (this.m_online)
            {
                return;
            }

            await this.WaitClearConnect().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var serialPortOption = this.Config.GetValue(SerialPortConfigExtension.SerialPortOptionProperty) ?? throw new ArgumentNullException("串口配置不能为空。");

            var serialPort = CreateSerial(serialPortOption);
            await this.PrivateOnSerialConnecting(new ConnectingEventArgs()).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            serialPort.Open();

            if (!serialPort.IsOpen)
            {
                ThrowHelper.ThrowException(TouchSocketCoreResource.UnknownError);
            }

            this.m_transport = new SerialPortTransport(serialPort, this.Config.GetValue(TouchSocketConfigExtension.TransportOptionProperty));
            this.m_online = true;
            // 启动新任务，处理连接后的操作
            this.m_runTask = EasyTask.SafeRun(this.PrivateConnected, this.m_transport);
        }
        finally
        {
            this.m_semaphoreForConnect.Release();
        }
    }

    #endregion Connect

    protected async Task ReceiveLoopAsync(ITransport transport)
    {
        var token = transport.ClosedToken;
        try
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var result = await transport.Input.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (this.DisposedValue || token.IsCancellationRequested)
                {
                    return;
                }

                if (result.IsCanceled || result.IsCompleted)
                {
                    return;
                }

                foreach (var item in result.Buffer)
                {
                    var reader = new ByteBlockReader(item);
                    try
                    {
                        if (await this.OnSerialReceiving(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                        {
                            continue;
                        }

                        if (this.m_dataHandlingAdapter == null)
                        {
                            await this.PrivateHandleReceivedData(reader, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                        else
                        {
                            await this.m_dataHandlingAdapter.ReceivedInputAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Exception(this, ex);
                    }
                }
                transport.Input.AdvanceTo(result.Buffer.End);
            }
        }
        catch (Exception ex)
        {
            // 如果发生异常，记录日志并退出接收循环
            this.Logger?.Debug(this, ex);
        }
        finally
        {
            var receiver = this.m_receiver;
            var e_closed = transport.ClosedEventArgs;
            if (receiver != null)
            {
                await receiver.Complete(e_closed.Message).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
       
    }

    /// <summary>
    /// 设置数据处理适配器。
    /// </summary>
    /// <param name="adapter">要设置的适配器实例。</param>
    /// <exception cref="ArgumentNullException">如果提供的适配器实例为<see langword="null"/>，则抛出此异常。</exception>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 检查当前实例是否已被释放，如果是，则抛出异常。
        this.ThrowIfDisposed();
        // 检查adapter参数是否为<see langword="null"/>，如果是，则抛出ArgumentNullException异常。
        if (adapter is null)
        {
            throw new ArgumentNullException(nameof(adapter));
        }

        // 如果当前实例的配置不为空，则将配置应用到适配器上。
        if (this.Config != null)
        {
            adapter.Config(this.Config);
        }

        // 设置适配器的日志记录器和加载、接收数据的回调方法。
        adapter.Logger = this.Logger;
        adapter.OnLoaded(this);
        adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
        //adapter.SendCallBack = this.ProtectedDefaultSend;
        adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;

        // 将提供的适配器实例设置为当前实例的数据处理适配器。
        this.m_dataHandlingAdapter = adapter;
    }

    private static SerialPort CreateSerial(SerialPortOption option)
    {
        var serialPort = new SerialPort(option.PortName, option.BaudRate, option.Parity, option.DataBits, option.StopBits)
        {
            Handshake = option.Handshake,
            RtsEnable = option.RtsEnable,
            DtrEnable = option.DtrEnable
        };
        return serialPort;
    }

    private async Task PrivateHandleReceivedData(IByteBlockReader byteBlock, IRequestInfo requestInfo)
    {
        var receiver = this.m_receiver;
        if (receiver != null)
        {
            await receiver.InputReceiveAsync(byteBlock, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await this.OnSerialReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task WaitClearConnect()
    {
        // 确保上次接收任务已经结束
        var runTask = this.m_runTask;
        if (runTask != null)
        {
            await runTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    #region Receiver

    /// <summary>
    /// 清除接收器对象
    /// </summary>
    /// <remarks>
    /// 将内部接收器对象置为<see langword="null"/>，以释放资源或重置状态
    /// </remarks>
    protected void ProtectedClearReceiver()
    {
        this.m_receiver = null;
    }

    /// <summary>
    /// 创建或获取一个接收器对象。
    /// </summary>
    /// <param name="receiverObject">接收器客户端对象，用于接收操作结果。</param>
    /// <returns>返回一个实现了IReceiver&lt;IReceiverResult&gt;接口的接收器对象。</returns>
    /// <remarks>
    /// 这个方法使用了空条件运算符（??=）来实现懒加载，即只有当m_receiver为<see langword="null"/>时才会创建一个新的InternalReceiver对象。
    /// 这样做可以提高性能，因为无需频繁地创建接收器实例。
    /// </remarks>
    protected IReceiver<IReceiverResult> ProtectedCreateReceiver(IReceiverClient<IReceiverResult> receiverObject)
    {
        return this.m_receiver ??= new InternalReceiver(receiverObject);
    }

    #endregion Receiver

    #region Throw

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfCannotSendRequestInfo()
    {
        if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSendRequestInfo)
        {
            throw new NotSupportedException($"当前适配器为空或者不支持对象发送。");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfClientNotConnected()
    {
        if (this.m_online)
        {
            return;
        }

        ThrowHelper.ThrowClientNotConnectedException();
    }

    #endregion Throw

    #region 发送

    /// <summary>
    /// 异步发送数据，保护方法。
    /// </summary>
    /// <param name="memory">待发送的字节数据内存。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>异步任务。</returns>
    protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();
        await this.OnSerialSending(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        var transport = this.m_transport;

        await transport.SemaphoreSlimForWriter.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            await transport.Output.WriteAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            transport.SemaphoreSlimForWriter.Release();
        }
    }

    /// <summary>
    /// 异步发送数据，通过适配器模式灵活处理数据发送。
    /// </summary>
    /// <param name="memory">待发送的只读字节内存块。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>一个异步任务，表示发送操作。</returns>
    protected Task ProtectedSendAsync(in ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        // 如果数据处理适配器未设置，则使用默认发送方式。
        if (this.m_dataHandlingAdapter == null)
        {
            return this.ProtectedDefaultSendAsync(memory, token);
        }
        else
        {
            // 否则，使用适配器的发送方法进行数据发送。
            return this.m_dataHandlingAdapter.SendInputAsync(memory, token);
        }
    }

    /// <summary>
    /// 异步发送请求信息的受保护方法。
    ///
    /// 此方法首先检查当前对象是否能够发送请求信息，如果不能，则抛出异常。
    /// 如果可以发送，它将使用数据处理适配器来异步发送输入请求。
    /// </summary>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，该任务代表异步操作的结果。</returns>
    protected Task ProtectedSendAsync(in IRequestInfo requestInfo, CancellationToken token)
    {
        // 检查是否具备发送请求的条件，如果不具备则抛出异常
        this.ThrowIfCannotSendRequestInfo();

        // 使用数据处理适配器异步发送输入请求
        return this.m_dataHandlingAdapter.SendInputAsync(requestInfo, token);
    }

    #endregion 发送
}