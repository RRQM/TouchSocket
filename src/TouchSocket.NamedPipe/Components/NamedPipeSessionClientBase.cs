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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using TouchSocket.Resources;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道服务器辅助客户端类
/// </summary>
[DebuggerDisplay("Id={Id},IP={IP},Port={Port}")]
[CodeInject.RegionInject(FileName = "TcpClientBase.cs", RegionName = "ReceiveLoopAsync", Placeholders = new[] { "OnTcpReceiving", "OnNamedPipeReceiving" })]
public abstract partial class NamedPipeSessionClientBase : ResolverConfigObject, INamedPipeSession, INamedPipeListenableClient, IIdClient
{
    #region 字段

    private TouchSocketConfig m_config;
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private string m_id;
    private NamedPipeListenOption m_listenOption;
    private bool m_online;
    private IPluginManager m_pluginManager;
    private InternalReceiver m_receiver;
    private Task m_runTask;
    private IScopedResolver m_scopedResolver;
    private INamedPipeServiceBase m_service;
    private NamedPipeTransport m_transport;
    private Func<NamedPipeSessionClientBase, bool> m_tryAddAction;
    private TryOutEventHandler<NamedPipeSessionClientBase> m_tryGet;
    private TryOutEventHandler<NamedPipeSessionClientBase> m_tryRemoveAction;

    #endregion 字段

    /// <summary>
    /// 命名管道服务器辅助客户端类
    /// </summary>
    public NamedPipeSessionClientBase()
    {
        this.Protocol = Protocol.NamedPipe;
    }

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_transport == null ? new CancellationToken(true) : this.m_transport.ClosedToken;

    /// <inheritdoc/>
    public override TouchSocketConfig Config => this.m_config;

    /// <inheritdoc/>
    public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

    /// <inheritdoc/>
    public string Id => this.m_id;

    /// <inheritdoc/>
    public bool IsClient => false;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_transport?.ReceiveCounter.LastIncrement ?? default;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_transport?.SendCounter.LastIncrement ?? default;

    /// <inheritdoc/>
    public NamedPipeListenOption ListenOption => this.m_listenOption;

    /// <inheritdoc/>
    public virtual bool Online => this.m_online;

    /// <inheritdoc/>
    public override IPluginManager PluginManager => this.m_pluginManager;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    /// <inheritdoc/>
    public override IResolver Resolver => this.m_scopedResolver.Resolver;

    /// <inheritdoc/>
    public INamedPipeServiceBase Service => this.m_service;

    #region Internal

    internal async Task InternalInitialized(TouchSocketConfig config, NamedPipeListenOption option, IResolver resolver, IPluginManager pluginManager, INamedPipeServiceBase serviceBase, Func<NamedPipeSessionClientBase, bool> tryAddAction, TryOutEventHandler<NamedPipeSessionClientBase> tryRemoveAction, TryOutEventHandler<NamedPipeSessionClientBase> tryGet)
    {
        this.m_config = config;
        this.m_pluginManager = pluginManager;

        this.m_listenOption = option;

        this.m_scopedResolver = resolver.CreateScopedResolver();
        this.Logger ??= this.m_scopedResolver.Resolver.Resolve<ILog>();

        this.m_service = serviceBase;

        this.m_tryAddAction = tryAddAction;
        this.m_tryRemoveAction = tryRemoveAction;
        this.m_tryGet = tryGet;
        await this.OnInitialized().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    internal async Task InternalNamedPipeConnected(NamedPipeTransport transport)
    {
        this.m_online = true;
        this.m_transport = transport;
        this.m_runTask = EasyTask.SafeRun(this.PrivateConnected, transport);
        await this.m_runTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        transport.SafeDispose();
    }

    internal async Task InternalNamedPipeConnecting(ConnectingEventArgs e)
    {
        await this.OnNamedPipeConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (this.m_dataHandlingAdapter == null)
        {
            var adapter = this.m_listenOption.Adapter?.Invoke();
            if (adapter != null)
            {
                this.SetAdapter(adapter);
            }
        }
    }

    internal void InternalSetId(string id)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(id, nameof(id));
        this.m_id = id;
    }

    private async Task PrivateConnected(ITransport transport)
    {
        var receiveTask = EasyTask.SafeRun(this.ReceiveLoopAsync, transport);
        var e_connected = new ConnectedEventArgs();
        await this.OnNamedPipeConnected(e_connected).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await receiveTask.SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var e_closed = transport.ClosedEventArgs;
        this.m_online = false;
        var adapter = this.m_dataHandlingAdapter;
        this.m_dataHandlingAdapter = default;
        adapter.SafeDispose();

        await this.OnNamedPipeClosed(e_closed).SafeWaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_tryRemoveAction.Invoke(this.m_id, out var _);
    }

    #endregion Internal

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
    public virtual Task ResetIdAsync(string newId, CancellationToken cancellationToken = default)
    {
        return this.ProtectedResetIdAsync(newId);
    }

    /// <summary>
    /// 处理已接收到的数据。
    /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
    /// </summary>
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
        // 将发送事件委托给插件管理器处理，异步调用相关插件的发送逻辑
        return this.PluginManager.RaiseAsync(typeof(INamedPipeSendingPlugin), this.Resolver, this, new SendingEventArgs(memory));
    }

    /// <summary>
    /// 直接重置内部Id。
    /// </summary>
    /// <param name="newId">新的Id值。</param>
    protected async Task ProtectedResetIdAsync(string newId)
    {
        // 检查新Id是否为空或null
        if (string.IsNullOrEmpty(newId))
        {
            throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
        }

        // 如果新旧Id相同，则无需进行更改
        if (this.Id == newId)
        {
            return;
        }

        // 保存当前Id
        var sourceId = this.Id;

        // 尝试从内部集合中移除当前Id
        if (this.m_tryRemoveAction(this.Id, out var sessionClient))
        {
            // 更新Socket客户端的Id
            sessionClient.m_id = newId;

            // 尝试将更新后的Socket客户端添加回集合
            if (this.m_tryAddAction(sessionClient))
            {
                // 如果插件管理器已启用，通知相关插件Id已更改
                if (this.PluginManager.Enable)
                {
                    var e = new IdChangedEventArgs(sourceId, newId);
                    await this.PluginManager.RaiseAsync(typeof(IIdChangedPlugin), this.Resolver, sessionClient, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                return;
            }
            else
            {
                // 如果添加失败，恢复原来的Id
                sessionClient.m_id = sourceId;

                // 再次尝试添加，如果失败，则抛出异常
                if (this.m_tryAddAction(sessionClient))
                {
                    throw new Exception("Id重复");
                }
                else
                {
                    // 如果恢复旧Id也失败，则关闭Socket客户端
                    await sessionClient.CloseAsync("修改新Id时操作失败，且回退旧Id时也失败。").ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        else
        {
            // 如果无法找到对应的Socket客户端，则抛出异常
            throw new ClientNotFindException(TouchSocketResource.ClientNotFind.Format(sourceId));
        }
    }

    /// <summary>
    /// 尝试通过Id获得对应的客户端
    /// </summary>
    /// <param name="id">客户端的唯一标识符</param>
    /// <param name="sessionClient">输出参数，用于返回找到的客户端实例</param>
    /// <returns>如果找到对应的客户端，则返回<see langword="true"/>；否则返回<see langword="false"/></returns>
    protected bool ProtectedTryGetClient(string id, out NamedPipeSessionClientBase sessionClient)
    {
        // 调用内部方法尝试获取客户端
        return this.m_tryGet(id, out sessionClient);
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
    /// 设置适配器
    /// </summary>
    /// <param name="adapter">要设置的适配器实例</param>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        this.ThrowIfDisposed();

        if (adapter is null)
        {
            this.m_dataHandlingAdapter = null;//允许Null赋值
            return;
        }

        // 检查当前实例是否已有配置
        if (this.Config != null)
        {
            // 如果有配置，则将其应用到适配器上
            adapter.Config(this.Config);
        }

        adapter.OnLoaded(this);

        adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
        this.m_dataHandlingAdapter = adapter;
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

    private async Task WaitClearConnect()
    {
        // 确保上次接收任务已经结束
        var runTask = this.m_runTask;
        if (runTask != null)
        {
            await runTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    #region 事件&委托

    /// <summary>
    /// 当初始化完成时，执行在<see cref="OnNamedPipeConnecting(ConnectingEventArgs)"/>之前。
    /// </summary>
    protected virtual Task OnInitialized()
    {
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 客户端已断开连接。
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
    /// 当客户端完整建立连接。
    /// </summary>
    /// <param name="e"></param>
    protected virtual async Task OnNamedPipeConnected(ConnectedEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 客户端正在连接。
    /// </summary>
    protected virtual async Task OnNamedPipeConnecting(ConnectingEventArgs e)
    {
        await this.PluginManager.RaiseAsync(typeof(INamedPipeConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task PrivateOnNamedPipeClosing(ClosingEventArgs e)
    {
        return this.OnNamedPipeClosing(e);
    }

    #endregion 事件&委托

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