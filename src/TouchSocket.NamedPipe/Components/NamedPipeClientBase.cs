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

using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using TouchSocket.Resources;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道客户端客户端基类
/// </summary>
[CodeInject.RegionInject(FileName = "TcpClientBase.cs", RegionName = "ReceiveLoopAsync", Placeholders = new[] { "OnTcpReceiving", "OnNamedPipeReceiving" })]
public abstract partial class NamedPipeClientBase : SetupConfigObject, INamedPipeSession
{
    /// <summary>
    /// 命名管道客户端客户端基类
    /// </summary>
    public NamedPipeClientBase()
    {
        this.Protocol = Protocol.NamedPipe;
    }

    #region 变量

    private readonly SemaphoreSlim m_semaphoreSlimForConnect = new SemaphoreSlim(1, 1);
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private volatile bool m_online;
    private InternalReceiver m_receiver;
    private Task m_runTask;
    private NamedPipeTransport m_transport;

    #endregion 变量

    #region 事件

    /// <summary>
    /// 断开连接。在客户端未设置连接状态时，不会触发
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnNamedPipeClosed(ClosedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(INamedPipeClosedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnNamedPipeClosing(ClosingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(INamedPipeClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 已经建立管道连接
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnNamedPipeConnected(ConnectedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 准备连接的时候
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnNamedPipeConnecting(ConnectingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateConnected(NamedPipeTransport transport)
    {
        var receiveTask = EasyTask.SafeRun(this.ReceiveLoopAsync, transport);

        var e_connected = new ConnectedEventArgs();
        await this.OnNamedPipeConnected(e_connected).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await receiveTask.SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        transport.SafeDispose();

        var e_closed = transport.ClosedEventArgs;
        this.m_online = false;
        var adapter = this.m_dataHandlingAdapter;
        this.m_dataHandlingAdapter = default;
        adapter.SafeDispose();

        await this.OnNamedPipeClosed(e_closed).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateOnNamedPipeClosing(ClosingEventArgs e)
    {
        await this.OnNamedPipeClosing(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateOnNamedPipeConnecting(ConnectingEventArgs e)
    {
        await this.OnNamedPipeConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (this.m_dataHandlingAdapter == null)
        {
            var adapter = this.Config.GetValue(NamedPipeConfigExtension.NamedPipeDataHandlingAdapterProperty)?.Invoke();
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
    public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

    /// <inheritdoc/>
    public bool IsClient => true;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_transport?.ReceiveCounter.LastIncrement ?? default;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_transport?.SendCounter.LastIncrement ?? default;

    /// <inheritdoc/>
    public virtual bool Online => this.m_online;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    #endregion 属性

    #region 断开操作

    /// <inheritdoc/>
    public virtual async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!this.m_online)
            {
                return Result.Success;
            }

            await this.PrivateOnNamedPipeClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var transport = this.m_transport;
            if (transport != null)
            {
                await transport.CloseAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// 建立管道的连接。
    /// </summary>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="TimeoutException"></exception>
    protected async Task PipeConnectAsync(CancellationToken cancellationToken)
    {
        await this.m_semaphoreSlimForConnect.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            if (this.m_online)
            {
                return;
            }
            this.ThrowIfDisposed();
            this.ThrowIfConfigIsNull();

            await this.WaitClearConnect().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var pipeName = this.Config.GetValue(NamedPipeConfigExtension.PipeNameProperty);
            ThrowHelper.ThrowArgumentNullExceptionIf(pipeName, nameof(pipeName));

            var serverName = this.Config.GetValue(NamedPipeConfigExtension.PipeServerNameProperty);

            var namedPipe = CreatePipeClient(serverName, pipeName);
            await this.PrivateOnNamedPipeConnecting(new ConnectingEventArgs()).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            await namedPipe.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            if (!namedPipe.IsConnected)
            {
                ThrowHelper.ThrowException(TouchSocketCoreResource.UnknownError);
            }

            this.m_transport = new NamedPipeTransport(namedPipe, this.Config.GetValue(TouchSocketConfigExtension.TransportOptionProperty));
            this.m_online = true;
            // 启动新任务，处理连接后的操作
            this.m_runTask = EasyTask.SafeRun(this.PrivateConnected, this.m_transport);
        }
        finally
        {
            this.m_semaphoreSlimForConnect.Release();
        }
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

    #endregion Connect

    #region Receiver

    /// <inheritdoc/>
    protected void ProtectedClearReceiver()
    {
        this.m_receiver = null;
    }

    /// <inheritdoc/>
    protected IReceiver<IReceiverResult> ProtectedCreateReceiver(IReceiverClient<IReceiverResult> receiverObject)
    {
        return this.m_receiver ??= new InternalReceiver(receiverObject);
    }

    #endregion Receiver

    /// <summary>
    /// 处理已接收到的数据。
    /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
    /// </summary>
    /// <param name="e"></param>
    /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
    protected virtual async Task OnNamedPipeReceived(ReceivedDataEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(INamedPipeReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当收到原始数据
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
    protected virtual ValueTask<bool> OnNamedPipeReceiving(IBytesReader reader)
    {
        return this.PluginManager.RaiseAsync(typeof(INamedPipeReceivingPlugin), this.Resolver, this, new BytesReaderEventArgs(reader));
    }

    /// <summary>
    /// 触发命名管道发送事件的异步方法。
    /// </summary>
    /// <param name="memory">待发送的字节内存。</param>
    /// <returns>一个等待任务，结果指示发送操作是否成功。</returns>
    protected virtual ValueTask<bool> OnNamedPipeSending(ReadOnlyMemory<byte> memory)
    {
        // 将发送任务委托给插件管理器，以便在所有相关的插件中引发命名管道发送事件
        return this.PluginManager.RaiseAsync(typeof(INamedPipeSendingPlugin), this.Resolver, this, new SendingEventArgs(memory));
    }

    /// <summary>
    /// 设置适配器
    /// </summary>
    /// <param name="adapter">要设置的适配器实例</param>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 检查当前实例是否已被释放
        this.ThrowIfDisposed();

        if (adapter is null)
        {
            this.m_dataHandlingAdapter = null;//允许Null赋值
            return;
        }

        if (this.Config != null)
        {
            adapter.Config(this.Config);
        }

        adapter.OnLoaded(this);
        adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
        this.m_dataHandlingAdapter = adapter;
    }

    private static NamedPipeClientStream CreatePipeClient(string serverName, string pipeName)
    {
        var pipeClient = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);
        return pipeClient;
    }

    private async Task PrivateHandleReceivedData(ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
    {
        var receiver = this.m_receiver;
        if (receiver != null)
        {
            await receiver.InputReceiveAsync(memory, requestInfo, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await this.OnNamedPipeReceived(new ReceivedDataEventArgs(memory, requestInfo)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// 异步发送数据，通过适配器模式灵活处理数据发送。
    /// </summary>
    /// <param name="memory">待发送的只读字节内存块。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>一个异步任务，表示发送操作。</returns>
    protected async Task ProtectedSendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();

        await this.OnNamedPipeSending(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        var transport = this.m_transport;
        var adapter = this.m_dataHandlingAdapter;
        var locker = transport.WriteLocker;

        await locker.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            // 如果数据处理适配器未设置，则使用默认发送方式。
            if (adapter == null)
            {
                await transport.Writer.WriteAsync(memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                var writer = new PipeBytesWriter(transport.Writer);
                adapter.SendInput(ref writer, in memory);
                await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        finally
        {
            locker.Release();
        }
    }

    /// <summary>
    /// 异步发送请求信息的受保护方法。
    ///
    /// 此方法首先检查当前对象是否能够发送请求信息，如果不能，则抛出异常。
    /// 如果可以发送，它将使用数据处理适配器来异步发送输入请求。
    /// </summary>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个任务，该任务代表异步操作的结果。</returns>
    protected async Task ProtectedSendAsync(IRequestInfo requestInfo, CancellationToken cancellationToken)
    {
        // 检查是否具备发送请求的条件，如果不具备则抛出异常
        this.ThrowIfCannotSendRequestInfo();

        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();

        var transport = this.m_transport;
        var adapter = this.m_dataHandlingAdapter;
        var locker = transport.WriteLocker;

        await locker.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            var writer = new PipeBytesWriter(transport.Writer);
            adapter.SendInput(ref writer, requestInfo);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            locker.Release();
        }
    }

    #endregion 发送
}