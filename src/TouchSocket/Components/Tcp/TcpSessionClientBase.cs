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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// 具有调试显示属性的抽象基类，用于TCP会话客户端。
/// </summary>
/// <remarks>
/// 此类提供了基础结构，用于支持TCP会话的客户端，包括ID、IP和端口信息。
/// 它实现了与TCP会话、客户端标识和解析器配置相关的接口。
/// </remarks>
/// <seealso cref="ResolverConfigObject"/>
/// <seealso cref="ITcpSession"/>
/// <seealso cref="ITcpListenableClient"/>
/// <seealso cref="IClient"/>
/// <seealso cref="IIdClient"/>
[DebuggerDisplay("Id={Id},IP={IP},Port={Port}")]
public abstract class TcpSessionClientBase : ResolverConfigObject, ITcpSession, ITcpListenableClient, IIdClient
{
    /// <summary>
    /// TcpSessionClientBase 类的构造函数。
    /// </summary>
    /// <remarks>
    /// 初始化 TcpSessionClientBase 类的新实例，并设置协议为 TCP。
    /// </remarks>
    protected TcpSessionClientBase()
    {
        // 设置当前会话的协议类型为 TCP，这是初始化会话时的基本配置。
        this.Protocol = Protocol.Tcp;
    }

    #region 变量

    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private string m_id;
    private string m_iP;
    private TcpListenOption m_listenOption;
    private volatile bool m_online;
    private IPluginManager m_pluginManager;
    private int m_port;
    private InternalReceiver m_receiver;
    private Task m_runTask;
    private IScopedResolver m_scopedResolver;
    private ITcpServiceBase m_service;
    private string m_serviceIp;
    private int m_servicePort;
    private TcpCore m_tcpCore;
    private TcpTransport m_transport;
    private Func<TcpSessionClientBase, bool> m_tryAddAction;
    private TryOutEventHandler<TcpSessionClientBase> m_tryGet;
    private TryOutEventHandler<TcpSessionClientBase> m_tryRemoveAction;

    #endregion 变量

    #region 属性

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_transport == null ? new CancellationToken(true) : this.m_transport.ClosedToken;

    /// <inheritdoc/>
    public sealed override TouchSocketConfig Config => this.Service?.Config;

    /// <inheritdoc/>
    public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

    /// <inheritdoc/>
    public string Id => this.m_id;

    /// <inheritdoc/>
    public string IP => this.m_iP;

    /// <inheritdoc/>
    public bool IsClient => false;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_transport == null ? default : this.m_transport.ReceiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_transport == null ? default : this.m_transport.SendCounter.LastIncrement;

    /// <inheritdoc/>
    public TcpListenOption ListenOption => this.m_listenOption;

    /// <inheritdoc/>
    public virtual bool Online => this.m_online;

    /// <inheritdoc/>
    public override IPluginManager PluginManager => this.m_pluginManager;

    /// <inheritdoc/>
    public int Port => this.m_port;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    /// <inheritdoc/>
    public override IResolver Resolver => this.m_scopedResolver.Resolver;

    /// <inheritdoc/>
    public ITcpServiceBase Service => this.m_service;

    /// <inheritdoc/>
    public string ServiceIP => this.m_serviceIp;

    /// <inheritdoc/>
    public int ServicePort => this.m_servicePort;

    /// <inheritdoc/>
    public bool UseSsl => this.m_tcpCore.UseSsl;

    protected ITransport Transport => this.m_transport;

    #endregion 属性

    #region Internal

    internal async Task InternalConnected(TcpTransport transport)
    {
        this.m_online = true;
        this.m_transport = transport;
        this.m_runTask = EasyTask.SafeRun(this.PrivateConnected, transport);
        await this.m_runTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    internal async Task InternalConnecting(ConnectingEventArgs e)
    {
        await this.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (this.m_dataHandlingAdapter == null)
        {
            var adapter = this.m_listenOption.Adapter?.Invoke();
            if (adapter != null)
            {
                this.SetAdapter(adapter);
            }
        }
    }

    internal async Task InternalInitialized(
        ITcpServiceBase serviceBase,
        IResolver resolver,
        TcpListenOption option,
        TcpCore tcpCore,
        IPluginManager pluginManager,
        Func<TcpSessionClientBase, bool> tryAddAction,
        TryOutEventHandler<TcpSessionClientBase> tryRemoveAction,
        TryOutEventHandler<TcpSessionClientBase> tryGet)
    {
        this.m_service = serviceBase;

        var scopedResolver = resolver.CreateScopedResolver();
        this.m_scopedResolver = scopedResolver;
        this.Logger ??= scopedResolver.Resolver.Resolve<ILog>();

        this.m_listenOption = option;

        var socket = tcpCore.Socket;
        this.m_iP = socket.RemoteEndPoint.GetIP();
        this.m_port = socket.RemoteEndPoint.GetPort();
        this.m_serviceIp = socket.LocalEndPoint.GetIP();
        this.m_servicePort = socket.LocalEndPoint.GetPort();
        this.m_tcpCore = tcpCore;

        this.m_pluginManager = pluginManager;

        this.m_tryAddAction = tryAddAction;
        this.m_tryRemoveAction = tryRemoveAction;
        this.m_tryGet = tryGet;
        await this.OnInitialized().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    internal void InternalSetId(string id)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(id, nameof(id));
        this.m_id = id;
    }

    private async Task PrivateConnected(TcpTransport transport)
    {
        var receiveTask = EasyTask.SafeRun(this.ReceiveLoopAsync, transport);

        var e_connected = new ConnectedEventArgs();
        await this.OnTcpConnected(e_connected).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await receiveTask.SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        transport.SafeDispose();

        var e_closed = transport.ClosedEventArgs;
        this.m_online = false;
        var adapter = this.m_dataHandlingAdapter;
        this.m_dataHandlingAdapter = default;
        adapter.SafeDispose();

        await this.OnTcpClosed(e_closed).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_tryRemoveAction.Invoke(this.m_id, out var _);
    }

    #endregion Internal

    #region 事件&委托

    /// <summary>
    /// 当初始化完成时，执行在<see cref="OnTcpConnecting(ConnectingEventArgs)"/>之前。
    /// </summary>
    protected virtual Task OnInitialized()
    {
        // 初始化完成后，无需执行任何操作，直接返回一个已完成的任务
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 客户端已断开连接。
    /// <para>
    /// 覆盖父类方法，将不会触发<see cref="ITcpClosedPlugin"/>插件。
    /// </para>
    /// </summary>
    /// <param name="e">包含断开连接相关信息的事件参数</param>
    protected virtual async Task OnTcpClosed(ClosedEventArgs e)
    {
        // 调用插件管理器，通知所有实现ITcpClosedPlugin接口的插件客户端已断开连接
        await this.PluginManager.RaiseAsync(typeof(ITcpClosedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// <para>
    /// 覆盖父类方法，将不会触发<see cref="ITcpClosingPlugin"/>插件。
    /// </para>
    /// </summary>
    /// <param name="e">包含断开连接相关信息的事件参数</param>
    protected virtual async Task OnTcpClosing(ClosingEventArgs e)
    {
        // 调用插件管理器，触发所有ITcpClosingPlugin类型的插件事件
        await this.PluginManager.RaiseAsync(typeof(ITcpClosingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当客户端完整建立Tcp连接时触发。
    /// <para>
    /// 覆盖父类方法，将不会触发<see cref="ITcpConnectedPlugin"/>插件。
    /// </para>
    /// </summary>
    /// <param name="e">包含连接信息的事件参数</param>
    protected virtual async Task OnTcpConnected(ConnectedEventArgs e)
    {
        // 调用插件管理器，异步触发所有实现ITcpConnectedPlugin接口的插件
        await this.PluginManager.RaiseAsync(typeof(ITcpConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 客户端正在连接。
    /// <para>
    /// 覆盖父类方法，将不会触发<see cref="ITcpConnectingPlugin"/>插件。
    /// </para>
    /// </summary>
    protected virtual async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        // 调用插件管理器，异步触发所有ITcpConnectingPlugin插件的对应事件
        await this.PluginManager.RaiseAsync(typeof(ITcpConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task PrivateOnTcpClosing(ClosingEventArgs e)
    {
        return this.OnTcpClosing(e);
    }

    #endregion 事件&委托

    /// <inheritdoc/>
    public virtual async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            if (!this.m_online)
            {
                return Result.Success;
            }

            await this.PrivateOnTcpClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    public virtual Task ResetIdAsync(string newId, CancellationToken token=default)
    {
        return this.ProtectedResetIdAsync(newId);
    }

    /// <summary>
    /// 当Id更新的时候触发
    /// </summary>
    /// <param name="sourceId">原始Id</param>
    /// <param name="targetId">目标Id</param>
    /// <returns>异步任务</returns>
    protected virtual async Task IdChanged(string sourceId, string targetId)
    {
        await this.PluginManager.RaiseAsync(typeof(IIdChangedPlugin), this.Resolver, this, new IdChangedEventArgs(sourceId, targetId)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当收到适配器处理的数据时。
    /// </summary>
    /// <param name="e">包含收到的数据事件的相关信息。</param>
    /// <returns>一个等待任务，表示异步操作完成时的信号。</returns>
    protected virtual async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        // 提供适配器处理的数据给所有相关的插件处理
        await this.PluginManager.RaiseAsync(typeof(ITcpReceivedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当收到原始数据
    /// </summary>
    /// <param name="byteBlock">包含收到的原始数据。</param>
    /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
    protected virtual ValueTask<bool> OnTcpReceiving(IBytesReader byteBlock)
    {
        // 将原始数据传递给所有相关的预处理插件，以进行初步的数据处理
        return this.PluginManager.RaiseAsync(typeof(ITcpReceivingPlugin), this.Resolver, this, new BytesReaderEventArgs(byteBlock));
    }

    /// <summary>
    /// 在数据即将通过TCP发送时触发，此方法用于通过插件机制拦截发送行为。
    /// 如果子类覆盖了此方法，则不会触发插件。
    /// </summary>
    /// <param name="memory">待发送的数据，以只读内存区形式提供。</param>
    /// <returns>返回值表示是否允许发送，true为允许，false为阻止。</returns>
    protected virtual ValueTask<bool> OnTcpSending(ReadOnlyMemory<byte> memory)
    {
        // 通过PluginManager委托调用ITcpSendingPlugin接口，传递当前实例和待发送的数据事件。
        // 这里使用RaiseAsync方法异步触发相关插件，以实现对发送行为的扩展或拦截。
        return this.PluginManager.RaiseAsync(typeof(ITcpSendingPlugin), this.Resolver, this, new SendingEventArgs(memory));
    }

    /// <summary>
    /// 直接重置内部Id。
    /// </summary>
    /// <param name="targetId">目标Id，用于重置内部Id。</param>
    protected async Task ProtectedResetIdAsync(string targetId)
    {
        // 检查当前对象是否已被回收，如果是，则抛出异常。
        this.ThrowIfDisposed();
        // 如果目标Id为空或仅含空格，则抛出参数为空异常。
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(targetId, nameof(targetId));

        // 保存当前对象的旧Id。
        var oldId = this.m_id;

        // 如果当前对象的Id与目标Id相同，则无需进行任何操作。
        if (oldId == targetId)
        {
            return;
        }

        // 尝试从内部存储中移除当前对象的Id，并获取关联的socket客户端。
        if (this.m_tryRemoveAction(oldId, out var sessionClient))
        {
            if (!ReferenceEquals(sessionClient, this))
            {
                ThrowHelper.ThrowInvalidOperationException("原Id对应的客户端和当前实例不相符，可能Id已经被修改。");
            }
            // 将获取到的socket客户端的Id更新为目标Id。
            sessionClient.m_id = targetId;
            // 尝试将更新了Id的socket客户端重新添加到内部存储中。
            if (this.m_tryAddAction(sessionClient))
            {
                // 如果添加成功，触发Id更改事件，并等待所有更改完成。
                await this.IdChanged(oldId, targetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                // 如果添加失败，抛出异常，提示该Id已经存在。
                ThrowHelper.ThrowException(TouchSocketResource.IdAlreadyExists.Format(targetId));
            }
        }
        else
        {
            // 如果在内部存储中未找到对应的socket客户端，抛出未找到客户端异常。
            ThrowHelper.ThrowClientNotFindException(this.m_id);
        }
    }

    /// <summary>
    /// 尝试通过Id获得对应的客户端
    /// </summary>
    /// <param name="id">客户端的唯一标识符</param>
    /// <param name="sessionClient">输出参数，用于返回找到的客户端实例</param>
    /// <returns>如果找到对应的客户端，则返回<see langword="true"/>；否则返回<see langword="false"/></returns>
    protected bool ProtectedTryGetClient(string id, out TcpSessionClientBase sessionClient)
    {
        // 调用内部方法m_tryGet来尝试获取客户端
        return this.m_tryGet(id, out sessionClient);
    }

    protected virtual async Task ReceiveLoopAsync(ITransport transport)
    {
        var token = transport.ClosedToken;
        try
        {
            while (true)
            {
                if (this.DisposedValue || token.IsCancellationRequested)
                {
                    return;
                }

                var result = await transport.Reader.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (result.Buffer.Length == 0)
                {
                    break;
                }
                var reader = new ClassBytesReader(result.Buffer);
                if (!await this.OnTcpReceiving(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    try
                    {
                        if (this.m_dataHandlingAdapter == null)
                        {
                            foreach (var item in reader.Sequence)
                            {
                                await this.PrivateHandleReceivedData(item, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                reader.Advance(item.Length);
                            }
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
                var position = result.Buffer.GetPosition(reader.BytesRead);
                transport.Reader.AdvanceTo(position, result.Buffer.End);

                if (result.IsCanceled || result.IsCompleted)
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // 如果发生异常，记录日志并退出接收循环
            this.Logger?.Debug(this, ex);
            return;
        }
        finally
        {
            var receiver = this.m_receiver;
            var e_closed = transport.ClosedEventArgs;
            if (receiver != null)
            {
                receiver.Complete(e_closed.Message);
            }
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

    /// <summary>
    /// 设置数据处理适配器。
    /// </summary>
    /// <param name="adapter">要设置的适配器实例。</param>
    /// <exception cref="ArgumentNullException">如果提供的适配器为<see langword="null"/>，则抛出此异常。</exception>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        this.ThrowIfDisposed();

        if (adapter is null)
        {
            this.m_dataHandlingAdapter = null;//允许Null赋值
            return;
        }

        // 如果当前实例的配置不为空，则将配置应用到适配器
        if (this.Config != null)
        {
            adapter.Config(this.Config);
        }

        adapter.OnLoaded(this);

        // 将接收到数据时的异步回调和发送数据时的异步回调设置到适配器中
        adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
        this.m_dataHandlingAdapter=adapter;
    }

    private async Task PrivateHandleReceivedData(ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
    {
        var receiver = this.m_receiver;
        if (receiver != null)
        {
            await receiver.InputReceiveAsync(memory, requestInfo, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await this.OnTcpReceived(new ReceivedDataEventArgs(memory, requestInfo)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    #region Throw

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfCannotSendRequestInfo()
    {
        if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSendRequestInfo)
        {
            ThrowHelper.ThrowNotSupportedException(TouchSocketResource.CannotSendRequestInfo);
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
    /// <param name="token">可取消令箭</param>
    /// <returns>一个异步任务，表示发送操作。</returns>
    protected async Task ProtectedSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();


        await this.OnTcpSending(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        var transport = this.m_transport;
        var adapter = this.m_dataHandlingAdapter;
        var locker = transport.WriteLocker;

        await locker.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            // 如果数据处理适配器未设置，则使用默认发送方式。
            if (adapter == null)
            {
                await transport.Writer.WriteAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                var writer = new PipeBytesWriter(transport.Writer);
                adapter.SendInput(ref writer, in memory);
                await writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，该任务代表异步操作的结果。</returns>
    protected async Task ProtectedSendAsync(IRequestInfo requestInfo, CancellationToken token)
    {
        // 检查是否具备发送请求的条件，如果不具备则抛出异常
        this.ThrowIfCannotSendRequestInfo();

        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();

        var transport = this.m_transport;
        var adapter = this.m_dataHandlingAdapter;
        var locker = transport.WriteLocker;

        await locker.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            var writer = new PipeBytesWriter(transport.Writer);
            adapter.SendInput(ref writer, requestInfo);
            await writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            locker.Release();
        }
    }


    #endregion 发送

    #region Receiver

    /// <summary>
    /// 清除接收器对象。
    /// </summary>
    protected void ProtectedClearReceiver()
    {
        this.m_receiver = null;
    }

    /// <summary>
    /// 创建或获取接收器对象。
    /// </summary>
    /// <param name="receiverObject">接收器客户端对象，用于创建接收器。</param>
    /// <returns>返回一个接收器对象，用于处理接收操作。</returns>
    protected IReceiver<IReceiverResult> ProtectedCreateReceiver(IReceiverClient<IReceiverResult> receiverObject)
    {
        // 使用空合并操作符确保接收器对象只被创建一次。
        return this.m_receiver ??= new InternalReceiver(receiverObject);
    }

    #endregion Receiver
}