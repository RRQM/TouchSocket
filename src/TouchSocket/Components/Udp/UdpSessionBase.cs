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
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// UdpSessionBase 类是 UDP 会话的基础抽象类，继承自 ServiceBase 类，并实现了 IUdpSessionBase 接口。
/// 它提供了 UDP 会话管理的基本功能，包括创建和关闭会话等。
/// </summary>
public abstract class UdpSessionBase : ServiceBase, IUdpSessionBase
{
    #region 字段

    private ServerState m_serverState;
    private InternalUdpReceiver m_receiver;
    private UdpDataHandlingAdapter m_dataHandlingAdapter;
    private DateTimeOffset m_lastReceivedTime;
    private DateTimeOffset m_lastSendTime;
    private UdpNetworkMonitor m_monitor;
    private readonly List<Task> m_receiveTasks = new List<Task>();
    private readonly SemaphoreSlim m_semaphoreSlimForReceiver = new SemaphoreSlim(1, 1);

    #endregion 字段

    /// <summary>
    /// 构造函数，初始化UDP会话基类。
    /// </summary>
    /// <remarks>
    /// 该构造函数负责创建和初始化一个UDP会话对象。它设置了会话的协议类型为UDP，
    /// 并创建了一个UDP套接字用于会话。此外，还初始化了一个用于监控UDP网络状态的对象。
    /// </remarks>
    protected UdpSessionBase()
    {
        // 设置当前会话的协议类型为UDP
        this.Protocol = Protocol.Udp;

        // 创建一个新的UDP套接字，用于接收和发送数据报
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // 初始化一个UDP网络监控对象，用于监控当前会话的网络状态
        this.m_monitor = new UdpNetworkMonitor(null, socket);
    }

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_lastReceivedTime;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_lastSendTime;

    /// <summary>
    /// 数据处理适配器
    /// </summary>
    protected UdpDataHandlingAdapter ProtectedDataHandlingAdapter => this.m_dataHandlingAdapter;

    /// <inheritdoc/>
    public UdpNetworkMonitor Monitor => this.m_monitor;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    /// <inheritdoc/>
    public IPHost RemoteIPHost => this.Config?.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);

    /// <inheritdoc/>
    public override ServerState ServerState => this.m_serverState;

    /// <summary>
    /// 在Udp中，该值没有意义，且可以自由赋值。
    /// </summary>
    public bool IsClient { get; protected set; }

    /// <summary>
    /// 从组播组中退出。
    /// </summary>
    /// <param name="multicastAddr">指定的组播地址。</param>
    /// <exception cref="ObjectDisposedException">如果当前对象已被处置，则引发异常。</exception>
    /// <exception cref="ArgumentNullException">如果multicastAddr为<see langword="null"/>，则引发异常。</exception>
    public void DropMulticastGroup(IPAddress multicastAddr)
    {
        this.ThrowIfDisposed();

        ThrowHelper.ThrowArgumentNullExceptionIf(multicastAddr, nameof(multicastAddr));

        // 根据Socket的地址族类型，执行相应的退出组播组操作
        if (this.m_monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
        {
            // 对于IPv4地址，创建并设置IP多播选项
            var optionValue = new MulticastOption(multicastAddr);
            this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
        }
        else
        {
            // 对于IPv6地址，创建并设置IPv6多播选项
            var optionValue2 = new IPv6MulticastOption(multicastAddr);
            this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue2);
        }
    }

    /// <summary>
    /// 加入组播。
    /// <para>组播地址为 224.0.0.0 ~ 239.255.255.255，其中 224.0.0.0~224.255.255.255 不建议在用户程序中使用，因为它们一般都有特殊用途。</para>
    /// </summary>
    /// <param name="multicastAddr">要加入的组播地址。</param>
    /// <exception cref="ArgumentNullException">如果 <paramref name="multicastAddr"/> 为 null，则抛出此异常。</exception>
    /// <exception cref="ObjectDisposedException">如果当前对象已被处置，则抛出此异常。</exception>
    public void JoinMulticastGroup(IPAddress multicastAddr)
    {
        this.ThrowIfDisposed();

        ThrowHelper.ThrowArgumentNullExceptionIf(multicastAddr, nameof(multicastAddr));

        // 根据不同的地址族设置组播成员资格
        if (this.m_monitor.Socket.AddressFamily == AddressFamily.InterNetwork)
        {
            // 对于IPv4地址，创建并应用组播选项
            var optionValue = new MulticastOption(multicastAddr);
            this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
        }
        else
        {
            // 对于IPv6地址，创建并应用组播选项
            var optionValue2 = new IPv6MulticastOption(multicastAddr);
            this.m_monitor.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue2);
        }
    }

    /// <inheritdoc/>
    public override async Task StartAsync()
    {
        this.ThrowIfDisposed();
        try
        {
            switch (this.m_serverState)
            {
                case ServerState.None:
                    {
                        if (this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty) is IPHost iPHost)
                        {
                            this.BeginReceive(iPHost);
                        }

                        break;
                    }
                case ServerState.Running:
                    return;

                case ServerState.Stopped:
                    {
                        if (this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty) is IPHost iPHost)
                        {
                            this.BeginReceive(iPHost);
                        }
                        break;
                    }
                default:
                    {
                        ThrowHelper.ThrowInvalidEnumArgumentException(this.m_serverState);
                        return;
                    }
            }

            this.m_serverState = ServerState.Running;

            await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch (Exception ex)
        {
            this.m_serverState = ServerState.Exception;
            await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, ex) { Message = ex.Message }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            throw;
        }
    }

    /// <inheritdoc/>
    public override async Task<Result> StopAsync(CancellationToken token = default)
    {
        try
        {
            this.m_monitor?.Socket.Dispose();
            this.m_monitor = null;
            this.m_serverState = ServerState.Stopped;
            await Task.WhenAll(this.m_receiveTasks.ToArray()).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_receiveTasks.Clear();

            if (this.m_receiver != null)
            {
                await this.m_receiver.Complete(default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            await this.PluginManager.RaiseAsync(typeof(IServerStartedPlugin), this.Resolver, this, new ServiceStateEventArgs(this.m_serverState, default)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

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
        if (!this.DisposedValue)
        {
            if (disposing)
            {
                this.StopAsync().GetFalseAwaitResult();
            }
        }
        base.SafetyDispose(disposing);
    }

    /// <summary>
    /// 处理已接收到的数据。
    /// </summary>
    /// <param name="e">包含接收到的数据的事件参数</param>
    /// <remarks>
    /// 此方法使用异步模式调用所有实现的处理程序来处理接收到的UDP数据。
    /// 它不直接处理数据，而是提供一个集中的位置来触发所有相关的插件处理程序。
    /// </remarks>
    protected virtual async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        // 触发所有实现了IUdpReceivedPlugin接口的插件的处理方法，并传递接收到的数据事件参数。
        await this.PluginManager.RaiseAsync(typeof(IUdpReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在UDP数据即将发送时触发插件。
    /// 如果当前类覆盖了父类的方法，则不会触发插件。
    /// </summary>
    /// <param name="endPoint">发送目标的端点。</param>
    /// <param name="memory">待发送的字节数据。</param>
    /// <returns>返回一个ValueTask布尔对象，指示插件是否处理了发送事件。</returns>
    protected virtual ValueTask<bool> OnUdpSending(EndPoint endPoint, ReadOnlyMemory<byte> memory)
    {
        // 提升插件管理器以异步方式提升IUdpSendingPlugin接口的事件
        // 使用UdpSendingEventArgs包装待发送的数据和目标端点
        return this.PluginManager.RaiseAsync(typeof(IUdpSendingPlugin), this.Resolver, this, new UdpSendingEventArgs(memory, endPoint));
    }

    /// <summary>
    /// 在Socket初始化对象后，Bind之前调用。
    /// 可用于设置Socket参数。
    /// 父类方法可覆盖。
    /// </summary>
    /// <param name="socket">待设置的Socket对象</param>
    protected virtual void PreviewBind(Socket socket)
    {
    }

    /// <summary>
    /// 设置适配器
    /// </summary>
    /// <param name="adapter">要设置的适配器实例</param>
    protected void SetAdapter(UdpDataHandlingAdapter adapter)
    {
        // 检查当前实例是否已被释放
        this.ThrowIfDisposed();
        // 如果适配器参数为空，则抛出ArgumentNullException异常
        ThrowHelper.ThrowArgumentNullExceptionIf(adapter, nameof(adapter));

        // 如果当前实例的配置信息不为空，则将配置信息应用到适配器上
        if (this.Config != null)
        {
            adapter.Config(this.Config);
        }
        // 设置适配器的日志记录器
        adapter.Logger = this.Logger;
        // 通知适配器当前实例的状态
        adapter.OnLoaded(this);
        // 设置适配器接收到数据时的回调方法
        adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
        // 设置适配器发送数据时的异步回调方法
        adapter.SendCallBackAsync = this.ProtectedDefaultSendAsync;
        // 将提供的适配器设置为当前实例的数据处理适配器
        this.m_dataHandlingAdapter = adapter;
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);
        if (this.m_dataHandlingAdapter == null)
        {
            var adapter = this.Config.GetValue(TouchSocketConfigExtension.UdpDataHandlingAdapterProperty)?.Invoke();
            if (adapter != null)
            {
                this.SetAdapter(adapter);
            }
        }
    }

    private void BeginReceive(IPHost iPHost)
    {
        var threadCount = this.Config.GetValue(TouchSocketConfigExtension.ThreadCountProperty);
        threadCount = threadCount < 0 ? 1 : threadCount;
        var socket = new Socket(iPHost.EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
        {
            EnableBroadcast = this.Config.GetValue(TouchSocketConfigExtension.EnableBroadcastProperty)
        };
        if (this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty))
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        #region Windows下UDP连接被重置错误10054

#if NET462_OR_GREATER
        if (this.Config.GetValue(TouchSocketConfigExtension.UdpConnResetProperty))
        {
            const int SIP_UDP_CONNRESET = -1744830452;
            socket.IOControl(SIP_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        }
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (this.Config.GetValue(TouchSocketConfigExtension.UdpConnResetProperty))
            {
                const int SIP_UDP_CONNRESET = -1744830452;
                socket.IOControl(SIP_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }
        }
#endif

        #endregion Windows下UDP连接被重置错误10054

        this.PreviewBind(socket);

        socket.Bind(iPHost.EndPoint);

        this.m_monitor = new UdpNetworkMonitor(iPHost, socket);

        for (var i = 0; i < threadCount; i++)
        {
            var task = EasyTask.SafeRun(this.RunReceive);
            this.m_receiveTasks.Add(task);
        }
    }

    private async Task RunReceive()
    {
        using (var udpSocketReceiver = new UdpSocketReceiver())
        {
            while (true)
            {
                using (var byteBlock = new ByteBlock(1024 * 64))
                {
                    try
                    {
                        if (this.m_serverState != ServerState.Running)
                        {
                            return;
                        }
                        var result = await udpSocketReceiver.ReceiveAsync(this.m_monitor.Socket, this.m_monitor.IPHost.EndPoint, byteBlock.TotalMemory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                        if (result.BytesTransferred > 0)
                        {
                            byteBlock.SetLength(result.BytesTransferred);
                            await this.HandleReceivingData(byteBlock, result.RemoteEndPoint).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                        else if (result.SocketError !=  SocketError.Success)
                        {
                            this.Logger?.Debug(this, result.SocketError.ToString());
                            return;
                        }
                        else
                        {
                            this.Logger?.Debug(this, TouchSocketCoreResource.UnknownError);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Exception(this, ex);
                        return;
                    }
                }
            }
        }
    }

    private async Task HandleReceivingData(IByteBlockReader byteBlock, EndPoint remoteEndPoint)
    {
        try
        {
            if (this.DisposedValue)
            {
                return;
            }

            this.m_lastReceivedTime = DateTimeOffset.UtcNow;

            if (await this.OnUdpReceiving(new UdpReceiveingEventArgs(remoteEndPoint, byteBlock)).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                return;
            }

            if (this.m_dataHandlingAdapter == null)
            {
                await this.PrivateHandleReceivedData(remoteEndPoint, byteBlock, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                await this.m_dataHandlingAdapter.ReceivedInput(remoteEndPoint, byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        catch (Exception ex)
        {
            this.Logger?.Exception(this, ex);
        }
    }

    /// <summary>
    /// 当收到原始数据时，处理数据。
    /// 该方法提供了一个机制，通过该机制可以对原始数据进行自定义处理，而不必修改具体的传输逻辑。
    /// </summary>
    /// <param name="e">包含收到的原始数据的字节块。</param>
    /// <returns>
    /// 如果返回<see langword="true"/>，则表示数据已被当前处理者完全处理，且不会再向下传递至其他处理者或消费者。
    /// 返回<see langword="false"/>表示当前处理者未处理数据，数据可能会被传递给下一个处理者或消费者。
    /// </returns>
    protected virtual ValueTask<bool> OnUdpReceiving(UdpReceiveingEventArgs e)
    {
        return this.PluginManager.RaiseAsync(typeof(IUdpReceivingPlugin), this.Resolver, this, e);
    }

    private async Task PrivateHandleReceivedData(EndPoint remoteEndPoint, IByteBlockReader byteBlock, IRequestInfo requestInfo)
    {
        if (this.m_receiver != null)
        {
            await this.m_semaphoreSlimForReceiver.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            try
            {
                await this.m_receiver.InputReceive(remoteEndPoint, byteBlock, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
            finally
            {
                this.m_semaphoreSlimForReceiver.Release();
            }
        }
        await this.OnUdpReceived(new UdpReceivedDataEventArgs(remoteEndPoint, byteBlock, requestInfo)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #region Throw

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfRemoteIPHostNull()
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(this.RemoteIPHost, nameof(this.RemoteIPHost));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfCannotSend()
    {
        if (this.m_serverState != ServerState.Running)
        {
            ThrowHelper.ThrowException(TouchSocketResource.UdpStopped);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfCannotSendRequestInfo()
    {
        if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSendRequestInfo)
        {
            ThrowHelper.ThrowNotSupportedException(TouchSocketResource.CannotSendRequestInfo);
        }
    }

    #endregion Throw

    #region 向默认远程异步发送

    /// <summary>
    /// 异步发送数据，使用提供的内存数据。
    /// </summary>
    /// <param name="memory">要发送的字节数据的内存段。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，表示发送操作的异步执行。</returns>
    protected virtual Task ProtectedSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        this.ThrowIfRemoteIPHostNull();
        // 调用重载的ProtectedSendAsync方法，传递目的端点和内存数据。
        return this.ProtectedSendAsync(this.RemoteIPHost.EndPoint, memory,token);
    }

    /// <summary>
    /// 异步发送数据，使用提供的请求信息。
    /// </summary>
    /// <param name="requestInfo">包含要发送数据的请求信息的对象。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，表示发送操作的异步执行。</returns>
    protected virtual Task ProtectedSendAsync(IRequestInfo requestInfo, CancellationToken token)
    {
        this.ThrowIfRemoteIPHostNull();
        return this.ProtectedSendAsync(this.RemoteIPHost.EndPoint, requestInfo, token);
    }

    #endregion 向默认远程异步发送

    #region 向设置的远程异步发送

    /// <summary>
    /// 异步发送数据到指定的端点。
    /// 该方法通过适配器转发数据，如果适配器未设置，则采用默认发送方式。
    /// </summary>
    /// <param name="endPoint">要发送数据到的目标端点。</param>
    /// <param name="memory">待发送的字节数据。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，表示异步操作的结果。</returns>
    protected virtual Task ProtectedSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        // 根据m_dataHandlingAdapter是否已设置，选择不同的发送方式
        return this.m_dataHandlingAdapter == null
            ? this.ProtectedDefaultSendAsync(endPoint, memory, token) // 使用默认发送方式
            : this.m_dataHandlingAdapter.SendInputAsync(endPoint, memory,token); // 通过适配器发送数据
    }

    /// <summary>
    /// 异步发送请求信息到指定的端点。
    /// 在发送之前，会检查是否具备发送请求信息的能力，并通过适配器进行发送。
    /// </summary>
    /// <param name="endPoint">要发送数据到的目标端点。</param>
    /// <param name="requestInfo">待发送的请求信息。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，表示异步操作的结果。</returns>
    protected virtual Task ProtectedSendAsync(EndPoint endPoint, IRequestInfo requestInfo, CancellationToken token)
    {
        // 检查是否具备发送请求信息的能力
        this.ThrowIfCannotSendRequestInfo();
        // 通过适配器发送请求信息
        return this.m_dataHandlingAdapter.SendInputAsync(endPoint, requestInfo, token);
    }

    #endregion 向设置的远程异步发送

    #region DefaultSendAsync

    /// <summary>
    /// 异步发送只读字节内存到远程主机。
    /// 此方法提供了一种默认的发送方式，确保只有在可以发送且远程IP主机不为空时才尝试发送数据。
    /// </summary>
    /// <param name="memory">要发送的只读字节内存。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>一个等待任务，表示异步操作。</returns>
    protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token=default)
    {
        // 如果不能发送数据，则抛出异常。
        this.ThrowIfCannotSend();
        // 如果远程IP主机为空，则抛出异常。
        this.ThrowIfRemoteIPHostNull();
        // 异步调用实际的发送方法，并传入远程主机的端点和要发送的数据。
        await this.ProtectedDefaultSendAsync(this.RemoteIPHost.EndPoint, memory,token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// 异步发送数据到指定的端点。
    ///
    /// 此方法用于封装实际的数据发送逻辑，在发送之前会进行必要的检查和调用事件处理程序，以确保数据的正确发送。
    /// </summary>
    /// <param name="endPoint">要发送数据到的端点。</param>
    /// <param name="memory">待发送的数据，以只读内存块的形式。</param>
    /// <param name="token"></param>
    /// <remarks>
    /// <para>在执行实际的数据发送之前，方法会：</para>
    /// <list type="bullet">
    /// <item>检查当前对象是否已经被释放（通过调用ThrowIfDisposed方法），如果是，则抛出异常。</item>
    /// <item>检查当前对象是否处于可以发送数据的状态（通过调用<see cref="ThrowIfCannotSend"/>方法），如果不是，则抛出异常。</item>
    /// <item>触发<see cref="OnUdpSending"/>事件，允许自定义在实际发送数据之前的逻辑。</item>
    /// </list>
    /// <para>之后，使用<see cref="Socket.SendToAsync(ArraySegment{byte}, SocketFlags, EndPoint)"/>方法异步地将数据发送到指定的端点。</para>
    /// <para>发送完成后，更新最后一次发送时间（<see cref="m_lastSendTime"/>）为当前的UTC时间。</para>
    /// </remarks>
    protected async Task ProtectedDefaultSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        this.ThrowIfDisposed();
        this.ThrowIfCannotSend();
        await this.OnUdpSending(endPoint, memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await this.Monitor.Socket.SendToAsync(memory, SocketFlags.None, endPoint,token);
        this.m_lastSendTime = DateTimeOffset.UtcNow;
    }
#else

    /// <summary>
    /// 异步发送数据到指定的端点。
    /// </summary>
    /// <param name="endPoint">要发送数据到的端点。</param>
    /// <param name="memory">待发送的数据，以只读内存形式提供。</param>
    /// <param name="token">可取消令箭</param>
    /// <remarks>
    /// 此方法为异步发送操作提供保护措施，确保数据在发送前进行必要的检查，
    /// 并通过UDP协议进行发送。
    /// </remarks>
    protected async Task ProtectedDefaultSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // 检查是否具备发送条件，如果不具备则抛出异常。
        this.ThrowIfCannotSend();
        // 检查对象是否已被释放，如果已被释放则抛出异常。
        this.ThrowIfDisposed();

        // 触发发送前的事件，允许修改数据或执行其他操作。
        await this.OnUdpSending(endPoint, memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        // 尝试将只读内存转换为数组形式，以便发送。
        if (MemoryMarshal.TryGetArray(memory, out var segment))
        {
            // 使用Socket的SendTo方法直接发送数据。
            this.Monitor.Socket.SendTo(segment.Array, segment.Offset, segment.Count, SocketFlags.None, endPoint);
        }
        else
        {
            // 如果无法直接转换为数组，则将内存复制到数组中。
            var array = memory.ToArray();

            // 使用Socket的SendTo方法发送数据数组。
            this.Monitor.Socket.SendTo(array, 0, array.Length, SocketFlags.None, endPoint);
        }
        // 更新最后一次发送时间。
        this.m_lastSendTime = DateTimeOffset.UtcNow;
    }

#endif

    #endregion DefaultSendAsync

    #region Receiver

    /// <summary>
    /// 创建或获取一个UDP接收器对象。
    /// </summary>
    /// <param name="receiverObject">接收器客户端对象，用于处理UDP接收结果。</param>
    /// <returns>返回一个<see cref="IReceiver{TResult}"/>类型的接收器对象。</returns>
    protected IReceiver<IUdpReceiverResult> ProtectedCreateReceiver(IReceiverClient<IUdpReceiverResult> receiverObject)
    {
        // 使用null合并运算符确保m_receiver只在首次调用时被实例化，以提高性能并减少资源消耗。
        return this.m_receiver ??= new InternalUdpReceiver(receiverObject);
    }

    /// <summary>
    /// 清除当前的UDP接收器对象。
    /// </summary>
    protected void ProtectedClearReceiver()
    {
        // 将m_receiver设置为<see langword="null"/>，以确保在不再需要时，接收器对象可以被垃圾回收。
        this.m_receiver = null;
    }

    #endregion Receiver
}