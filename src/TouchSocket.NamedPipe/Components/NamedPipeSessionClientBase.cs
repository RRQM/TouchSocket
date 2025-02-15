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
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道服务器辅助客户端类
/// </summary>
[DebuggerDisplay("Id={Id},IP={IP},Port={Port}")]
public abstract class NamedPipeSessionClientBase : ResolverConfigObject, INamedPipeSession, INamedPipeListenableClient, IIdClient
{
    #region 字段

    private readonly Lock m_lockForAbort = new Lock();
    private readonly SemaphoreSlim m_semaphoreSlimForSend = new SemaphoreSlim(1, 1);
    private TouchSocketConfig m_config;
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private NamedPipeListenOption m_listenOption;
    private bool m_online;
    private NamedPipeServerStream m_pipeStream;
    private IPluginManager m_pluginManager;
    private int m_receiveBufferSize = 1024 * 10;
    private ValueCounter m_receiveCounter;
    private InternalReceiver m_receiver;
    private Task m_receiveTask;
    private IScopedResolver m_scopedResolver;

    //private IResolver m_resolver;
    private INamedPipeServiceBase m_service;

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
        this.m_receiveCounter = new ValueCounter
        {
            Period = TimeSpan.FromSeconds(1),
            OnPeriod = this.OnPeriod
        };
    }

    /// <inheritdoc/>
    public override TouchSocketConfig Config => this.m_config;

    /// <inheritdoc/>
    public string Id { get; private set; }

    /// <inheritdoc/>
    public bool IsClient => false;

    /// <inheritdoc/>
    public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTime LastSentTime { get; private set; }

    /// <inheritdoc/>
    public NamedPipeListenOption ListenOption => this.m_listenOption;

    /// <inheritdoc/>
    public virtual bool Online => this.m_online;

    /// <inheritdoc/>
    public PipeStream PipeStream => this.m_pipeStream;

    /// <inheritdoc/>
    public override IPluginManager PluginManager => this.m_pluginManager;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    /// <inheritdoc/>
    public override IResolver Resolver => this.m_scopedResolver.Resolver;

    /// <inheritdoc/>
    public INamedPipeServiceBase Service => this.m_service;

    /// <inheritdoc/>
    public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

    #region Internal

    internal Task InternalInitialized()
    {
        this.LastSentTime = DateTime.UtcNow;
        return this.OnInitialized();
    }

    internal async Task InternalNamedPipeConnected(ConnectedEventArgs e)
    {
        this.m_online = true;

        this.m_receiveTask = Task.Factory.StartNew(this.BeginReceive, TaskCreationOptions.LongRunning).Unwrap();
        this.m_receiveTask.FireAndForget();

        await this.OnNamedPipeConnected(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    internal void InternalSetAction(Func<NamedPipeSessionClientBase, bool> tryAddAction, TryOutEventHandler<NamedPipeSessionClientBase> tryRemoveAction, TryOutEventHandler<NamedPipeSessionClientBase> tryGet)
    {
        this.m_tryAddAction = tryAddAction;
        this.m_tryRemoveAction = tryRemoveAction;
        this.m_tryGet = tryGet;
    }

    internal void InternalSetConfig(TouchSocketConfig config)
    {
        this.m_config = config;
    }

    internal void InternalSetContainer(IResolver resolver)
    {
        this.m_scopedResolver = resolver.CreateScopedResolver();
        this.Logger ??= this.m_scopedResolver.Resolver.Resolve<ILog>();
    }

    internal void InternalSetId(string id)
    {
        this.Id = id;
    }

    internal void InternalSetListenOption(NamedPipeListenOption option)
    {
        this.m_listenOption = option;
    }

    internal void InternalSetNamedPipe(NamedPipeServerStream namedPipe)
    {
        this.m_pipeStream = namedPipe;
    }

    internal void InternalSetPluginManager(IPluginManager pluginManager)
    {
        this.m_pluginManager = pluginManager;
    }

    internal void InternalSetService(INamedPipeServiceBase serviceBase)
    {
        this.m_service = serviceBase;
    }

    #endregion Internal

    /// <inheritdoc/>
    public virtual async Task CloseAsync(string msg)
    {
        if (this.m_online)
        {
            await this.PrivateOnNamedPipeClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            lock (this.m_lockForAbort)
            {
                this.m_pipeStream.Close();
                this.Abort(true, msg);
            }
        }
    }

    /// <inheritdoc/>
    public virtual Task ResetIdAsync(string newId)
    {
        return this.ProtectedResetIdAsync(newId);
    }

    /// <summary>
    /// 中止当前操作，并安全地关闭相关资源。
    /// </summary>
    /// <param name="manual">指示中止操作是否是手动触发的。</param>
    /// <param name="msg">中止操作的消息说明。</param>
    protected void Abort(bool manual, string msg)
    {
        // 使用锁对象 m_lockForAbort 来防止并发访问，确保线程安全
        lock (this.m_lockForAbort)
        {
            // 尝试从管理器中移除当前操作，如果成功且当前状态为在线，则进行中止操作
            if (this.m_tryRemoveAction(this.Id, out _) && this.m_online)
            {
                // 设置在线状态为 false，表示当前操作已离线
                this.m_online = false;

                // 安全地释放管道流资源，避免资源泄露
                this.m_pipeStream.SafeDispose();

                // 安全地释放保护数据处理适配器资源，避免资源泄露
                this.m_dataHandlingAdapter.SafeDispose();

                // 启动一个新的任务来处理管道关闭后的操作，传递中止操作的参数
                _ = Task.Factory.StartNew(this.PrivateOnNamedPipeClosed, new ClosedEventArgs(manual, msg));
            }
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }
        if (disposing)
        {
            this.m_scopedResolver.SafeDispose();
            this.Abort(true, TouchSocketResource.DisposeClose);
        }
        base.Dispose(disposing);
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
    /// <param name="byteBlock"></param>
    /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
    protected virtual ValueTask<bool> OnNamedPipeReceiving(ByteBlock byteBlock)
    {
        return this.PluginManager.RaiseAsync(typeof(INamedPipeReceivingPlugin), this.Resolver, this, new ByteBlockEventArgs(byteBlock));
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
            sessionClient.Id = newId;

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
                sessionClient.Id = sourceId;

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
    /// <returns>如果找到对应的客户端，则返回true；否则返回false</returns>
    protected bool ProtectedTryGetClient(string id, out NamedPipeSessionClientBase sessionClient)
    {
        // 调用内部方法尝试获取客户端
        return this.m_tryGet(id, out sessionClient);
    }

    /// <summary>
    /// 设置适配器
    /// </summary>
    /// <param name="adapter">要设置的适配器实例</param>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 检查传入的适配器实例是否为null
        if (adapter is null)
        {
            // 如果为null，则抛出ArgumentNullException异常
            throw new ArgumentNullException(nameof(adapter));
        }

        // 检查当前实例是否已有配置
        if (this.Config != null)
        {
            // 如果有配置，则将其应用到适配器上
            adapter.Config(this.Config);
        }

        // 将当前实例的日志记录器设置到适配器上
        adapter.Logger = this.Logger;
        // 调用适配器的OnLoaded方法，通知适配器已被加载
        adapter.OnLoaded(this);
        // 设置适配器接收数据时的回调方法
        adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
        // 设置适配器发送数据时的回调方法
        //adapter.SendCallBack = this.ProtectedDefaultSend;
        adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;
        // 将适配器实例设置为当前实例的数据处理适配器
        this.m_dataHandlingAdapter = adapter;
    }

    private async Task BeginReceive()
    {
        while (true)
        {
            using (var byteBlock = new ByteBlock(this.GetReceiveBufferSize()))
            {
                try
                {
                    var r = await this.m_pipeStream.ReadAsync(byteBlock.TotalMemory, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (r == 0)
                    {
                        this.Abort(false, "远程终端主动关闭");
                        return;
                    }

                    byteBlock.SetLength(r);
                    await this.HandleReceivingData(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    this.Abort(false, ex.Message);
                    return;
                }
            }
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

    private int GetReceiveBufferSize()
    {
        var minBufferSize = this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) ?? 1024 * 10;
        var maxBufferSize = this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) ?? 1024 * 512;
        return Math.Min(Math.Max(this.m_receiveBufferSize, minBufferSize), maxBufferSize);
    }

    private async Task PrivateOnNamedPipeClosed(object obj)
    {
        try
        {
            var e = (ClosedEventArgs)obj;
            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.Complete(e.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            await this.OnNamedPipeClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
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

    private async Task HandleReceivingData(ByteBlock byteBlock)
    {
        try
        {
            if (this.DisposedValue)
            {
                return;
            }

            this.m_receiveCounter.Increment(byteBlock.Length);

            if (await this.OnNamedPipeReceiving(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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

    private void OnPeriod(long value)
    {
        this.m_receiveBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
    }

    private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        var receiver = this.m_receiver;
        if (receiver != null)
        {
            await receiver.InputReceiveAsync(byteBlock, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await this.OnNamedPipeReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    #region 直接发送

    /// <inheritdoc/>
    protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
    {
        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();
        await this.OnNamedPipeSending(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            await this.m_semaphoreSlimForSend.WaitAsync();
            await this.m_pipeStream.WriteAsync(memory, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.LastSentTime = DateTime.UtcNow;
        }
        finally
        {
            this.m_semaphoreSlimForSend.Release();
        }
    }

    #endregion 直接发送

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
    /// 异步安全发送请求信息。
    /// </summary>
    /// <param name="requestInfo">请求信息，用于发送。</param>
    /// <returns>返回一个任务，该任务表示发送操作的异步结果。</returns>
    /// <remarks>
    /// 此方法用于在执行实际的数据处理之前，确保当前状态允许发送请求信息。
    /// 它使用了数据处理适配器来异步发送输入请求。
    /// </remarks>
    protected Task ProtectedSendAsync(in IRequestInfo requestInfo)
    {
        // 检查当前状态是否允许发送请求信息，如果不允许则抛出异常。
        this.ThrowIfCannotSendRequestInfo();
        // 使用数据处理适配器异步发送请求信息。
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