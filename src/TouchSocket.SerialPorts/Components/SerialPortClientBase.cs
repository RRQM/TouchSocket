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
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
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

    private readonly Lock m_lockForAbort = new Lock();
    private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private bool m_online;
    private InternalReceiver m_receiver;
    private SerialCore m_serialCore;
    private Task m_taskReceive;

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
    protected virtual ValueTask<bool> OnSerialReceiving(ByteBlock byteBlock)
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

    private async Task PrivateOnClosing(ClosingEventArgs e)
    {
        await this.OnSerialClosing(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateOnSerialClosed(object obj)
    {
        try
        {
            var e = (ClosedEventArgs)obj;
            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.Complete(e.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            await this.OnSerialClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
    }

    private async Task PrivateOnSerialConnected(object o)
    {
        try
        {
            await this.OnSerialConnected((ConnectedEventArgs)o).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
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
    public bool IsClient => true;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_serialCore.ReceiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_serialCore.SendCounter.LastIncrement;

    /// <inheritdoc/>
    public bool Online => this.m_online && this.m_serialCore != null && this.m_serialCore.SerialPort.IsOpen;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    /// <inheritdoc/>
    protected SingleStreamDataHandlingAdapter ProtectedDataHandlingAdapter => this.m_dataHandlingAdapter;

    /// <inheritdoc/>
    protected SerialPort ProtectedMainSerialPort => this.m_serialCore?.SerialPort;

    #endregion 属性

    #region 断开操作

    /// <inheritdoc/>
    public virtual async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            if (this.m_online)
            {
                await this.PrivateOnClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                this.Abort(true, msg);
            }

            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Abort(true, $"{nameof(Dispose)}主动断开");
        }
        base.Dispose(disposing);
    }

    #endregion 断开操作

    #region Connect

    /// <inheritdoc/>
    public async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
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
            var serialPortOption = this.Config.GetValue(SerialPortConfigExtension.SerialPortOptionProperty) ?? throw new ArgumentNullException("串口配置不能为空。");

            this.m_serialCore.SafeDispose();

            var serialCore = CreateSerial(serialPortOption);
            await this.PrivateOnSerialConnecting(new ConnectingEventArgs()).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            serialCore.SerialPort.Open();

            this.m_online = true;

            this.m_serialCore = serialCore;

            this.m_taskReceive = EasyTask.Run(this.BeginReceive);
            this.m_taskReceive.FireAndForget();

            _ = EasyTask.Run(this.PrivateOnSerialConnected, new ConnectedEventArgs());
        }
        finally
        {
            this.m_semaphoreForConnect.Release();
        }
    }

    #endregion Connect

    /// <summary>
    /// 中止连接方法
    /// </summary>
    /// <param name="manual">是否为手动断开连接</param>
    /// <param name="msg">断开连接的原因</param>
    protected void Abort(bool manual, string msg)
    {
        // 锁定连接操作，确保线程安全
        lock (this.m_lockForAbort)
        {
            // 检查是否当前处于在线状态
            if (this.m_online)
            {
                // 将状态设置为离线
                this.m_online = false;
                // 安全释放串口核心资源
                this.m_serialCore.SafeDispose();

                // 安全释放数据处理适配器资源
                this.m_dataHandlingAdapter.SafeDispose();
                this.m_dataHandlingAdapter = default;

                // 启动一个新的任务，用于处理串口关闭事件
                _ = EasyTask.Run(this.PrivateOnSerialClosed, new ClosedEventArgs(manual, msg));
            }
        }
    }

    /// <summary>
    /// 设置数据处理适配器。
    /// </summary>
    /// <param name="adapter">要设置的适配器实例。</param>
    /// <exception cref="ArgumentNullException">如果提供的适配器实例为null，则抛出此异常。</exception>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 检查当前实例是否已被释放，如果是，则抛出异常。
        this.ThrowIfDisposed();
        // 检查adapter参数是否为null，如果是，则抛出ArgumentNullException异常。
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

    private static SerialCore CreateSerial(SerialPortOption option)
    {
        var serialPort = new SerialCore(option.PortName, option.BaudRate, option.Parity, option.DataBits, option.StopBits, option.StearmAsync);
        serialPort.SerialPort.Handshake = option.Handshake;
        serialPort.SerialPort.RtsEnable = option.RtsEnable;
        serialPort.SerialPort.DtrEnable = option.DtrEnable;
        return serialPort;
    }

    private async Task BeginReceive()
    {
        while (true)
        {
            try
            {
                using (var byteBlock = new ByteBlock(1024 * 64))
                {
                    var result = await this.m_serialCore.ReceiveAsync(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                    if (result.BytesTransferred > 0)
                    {
                        byteBlock.SetLength(result.BytesTransferred);
                        await this.HandleReceivingData(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Abort(false, ex.Message);
                break;
            }
        }
    }

    private async Task HandleReceivingData(ByteBlock byteBlock)
    {
        try
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (await this.OnSerialReceiving(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                return;
            }

            if (this.m_dataHandlingAdapter == null)
            {
                await this.PrivateHandleReceivedData(byteBlock, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                await this.m_dataHandlingAdapter.ReceivedInputAsync(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        catch (Exception ex)
        {
            this.Logger?.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
        }
    }

    #region Receiver

    /// <summary>
    /// 清除接收器对象
    /// </summary>
    /// <remarks>
    /// 将内部接收器对象置为null，以释放资源或重置状态
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
    /// 这个方法使用了空条件运算符（??=）来实现懒加载，即只有当m_receiver为null时才会创建一个新的InternalReceiver对象。
    /// 这样做可以提高性能，因为无需频繁地创建接收器实例。
    /// </remarks>
    protected IReceiver<IReceiverResult> ProtectedCreateReceiver(IReceiverClient<IReceiverResult> receiverObject)
    {
        return this.m_receiver ??= new InternalReceiver(receiverObject);
    }

    #endregion Receiver

    private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        var receiver = this.m_receiver;
        if (receiver != null)
        {
            await receiver.InputReceiveAsync(byteBlock, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await this.OnSerialReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

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
    /// <returns>异步任务。</returns>
    protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
    {
        // 检查实例是否已被处置，确保资源正确管理。
        this.ThrowIfDisposed();
        // 检查客户端是否已连接，防止在未连接状态下尝试发送数据。
        this.ThrowIfClientNotConnected();
        // 触发序列发送事件之前置处理，为实际数据发送做准备。
        await this.OnSerialSending(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        // 通过序列核心发送数据，实际的数据发送操作。
        await this.m_serialCore.SendAsync(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 发送

    #region 异步发送

    /// <summary>
    /// 异步发送数据，通过适配器模式灵活处理数据发送。
    /// </summary>
    /// <param name="memory">待发送的只读字节内存块。</param>
    /// <returns>一个异步任务，表示发送操作。</returns>
    protected Task ProtectedSendAsync(in ReadOnlyMemory<byte> memory)
    {
        // 如果数据处理适配器未设置，则使用默认发送方式。
        if (this.m_dataHandlingAdapter == null)
        {
            return this.ProtectedDefaultSendAsync(memory);
        }
        else
        {
            // 否则，使用适配器的发送方法进行数据发送。
            return this.m_dataHandlingAdapter.SendInputAsync(memory);
        }
    }

    /// <summary>
    /// 异步发送请求信息的受保护方法。
    ///
    /// 此方法首先检查当前对象是否能够发送请求信息，如果不能，则抛出异常。
    /// 如果可以发送，它将使用数据处理适配器来异步发送输入请求。
    /// </summary>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <returns>返回一个任务，该任务代表异步操作的结果。</returns>
    protected Task ProtectedSendAsync(in IRequestInfo requestInfo)
    {
        // 检查是否具备发送请求的条件，如果不具备则抛出异常
        this.ThrowIfCannotSendRequestInfo();

        // 使用数据处理适配器异步发送输入请求
        return this.m_dataHandlingAdapter.SendInputAsync(requestInfo);
    }

    /// <summary>
    /// 异步发送数据。
    /// 如果数据处理适配器不存在或无法拼接发送，则将所有传输字节合并到一个连续的内存块中发送。
    /// 如果数据处理适配器存在且支持拼接发送，则直接发送传输字节列表。
    /// </summary>
    /// <param name="transferBytes">要发送的字节数据列表，每个项代表一个字节片段。</param>
    /// <returns>发送任务。</returns>
    protected async Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
    {
        // 检查数据处理适配器是否存在且支持拼接发送
        if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
        {
            // 如果不支持拼接发送，则计算所有字节片段的总长度
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }
            // 使用计算出的总长度创建一个连续的内存块
            using (var byteBlock = new ByteBlock(length))
            {
                // 将每个字节片段写入连续的内存块
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
                }
                // 根据数据处理适配器的存在与否，选择不同的发送方式
                if (this.m_dataHandlingAdapter == null)
                {
                    // 如果没有数据处理适配器，则使用默认方式发送
                    await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                else
                {
                    // 如果有数据处理适配器，则通过适配器发送
                    await this.m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        else
        {
            // 如果数据处理适配器支持拼接发送，则直接发送字节列表
            await this.m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    #endregion 异步发送
}