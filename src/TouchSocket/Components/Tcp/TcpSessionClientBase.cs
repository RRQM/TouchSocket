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
using System.Net.Sockets;
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
public abstract class TcpSessionClientBase : ResolverConfigObject, ITcpSession, ITcpListenableClient, IClient, IIdClient
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

    private readonly Lock m_lockForAbort = new Lock();
    private Task m_beginReceiveTask;
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private string m_id;
    private string m_iP;
    private TcpListenOption m_listenOption;
    private Socket m_mainSocket;
    private volatile bool m_online;
    private IPluginManager m_pluginManager;
    private int m_port;
    private InternalReceiver m_receiver;

    //private IResolver m_resolver;
    private Action<TcpCore> m_returnTcpCore;

    private IScopedResolver m_scopedResolver;
    private ITcpServiceBase m_service;
    private string m_serviceIP;
    private int m_servicePort;
    private TcpCore m_tcpCore;
    private Func<TcpSessionClientBase, bool> m_tryAddAction;
    private TryOutEventHandler<TcpSessionClientBase> m_tryGet;
    private TryOutEventHandler<TcpSessionClientBase> m_tryRemoveAction;

    #endregion 变量

    #region 属性

    /// <inheritdoc/>
    public override sealed TouchSocketConfig Config => this.Service?.Config;

    /// <inheritdoc/>
    public SingleStreamDataHandlingAdapter DataHandlingAdapter => this.m_dataHandlingAdapter;

    /// <inheritdoc/>
    public string Id => this.m_id;

    /// <inheritdoc/>
    public string IP => this.m_iP;

    /// <inheritdoc/>
    public bool IsClient => false;

    /// <inheritdoc/>
    public DateTime LastReceivedTime => this.m_tcpCore.ReceiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTime LastSentTime => this.m_tcpCore.SendCounter.LastIncrement;

    /// <inheritdoc/>
    public TcpListenOption ListenOption => this.m_listenOption;

    /// <inheritdoc/>
    public Socket MainSocket => this.m_mainSocket;

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
    public string ServiceIP => this.m_serviceIP;

    /// <inheritdoc/>
    public int ServicePort => this.m_servicePort;

    /// <inheritdoc/>
    public bool UseSsl => this.m_tcpCore.UseSsl;

    #endregion 属性

    #region Internal

    internal async Task InternalConnected(ConnectedEventArgs e)
    {
        this.m_online = true;

        this.m_beginReceiveTask = Task.Run(this.BeginReceive);

        this.m_beginReceiveTask.FireAndForget();

        await this.OnTcpConnected(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    internal Task InternalInitialized()
    {
        return this.OnInitialized();
    }

    internal void InternalSetAction(Func<TcpSessionClientBase, bool> tryAddAction, TryOutEventHandler<TcpSessionClientBase> tryRemoveAction, TryOutEventHandler<TcpSessionClientBase> tryGet)
    {
        this.m_tryAddAction = tryAddAction;
        this.m_tryRemoveAction = tryRemoveAction;
        this.m_tryGet = tryGet;
    }

    internal void InternalSetId(string id)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(id, nameof(id));
        this.m_id = id;
    }

    internal void InternalSetListenOption(TcpListenOption option)
    {
        this.m_listenOption = option;
    }

    internal void InternalSetPluginManager(IPluginManager pluginManager)
    {
        this.m_pluginManager = pluginManager;
    }

    internal void InternalSetResolver(IResolver resolver)
    {
        var scopedResolver = resolver.CreateScopedResolver();
        //this.m_resolver = resolver;
        this.m_scopedResolver = scopedResolver;
        this.Logger ??= scopedResolver.Resolver.Resolve<ILog>();
    }

    internal void InternalSetReturnTcpCore(Action<TcpCore> returnTcpCore)
    {
        this.m_returnTcpCore = returnTcpCore;
    }

    internal void InternalSetService(ITcpServiceBase serviceBase)
    {
        this.m_service = serviceBase;
    }

    internal void InternalSetTcpCore(TcpCore tcpCore)
    {
        var socket = tcpCore.Socket;
        this.m_mainSocket = socket;
        this.m_iP = socket.RemoteEndPoint.GetIP();
        this.m_port = socket.RemoteEndPoint.GetPort();
        this.m_serviceIP = socket.LocalEndPoint.GetIP();
        this.m_servicePort = socket.LocalEndPoint.GetPort();

        //tcpCore.OnReceived = this.HandleReceived;
        //tcpCore.OnAbort = this.TcpCoreBreakOut;
        if (this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) is int minValue)
        {
            tcpCore.MinBufferSize = minValue;
        }

        if (this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) is int maxValue)
        {
            tcpCore.MaxBufferSize = maxValue;
        }
        this.m_tcpCore = tcpCore;
    }

    /// <summary>
    /// 中止当前操作。
    /// </summary>
    /// <param name="manual">是否为手动中止。</param>
    /// <param name="msg">中止的消息。</param>
    protected void Abort(bool manual, string msg)
    {
        lock (this.m_lockForAbort)
        {
            // 如果当前实例没有有效的ID，则无需执行中止操作

            var id = this.m_id;
            if (!this.m_online)
            {
                return;
            }

            //此处的设计是为了防止多次调用Abort方法。
            //一般情况下，由于m_tryRemoveAction是线程安全，所以移除客户端只会被调用一次，
            //但是在ResetId的某些情况下，可能会被多次调用。
            //所以这里加了一个判断，如果已经中止，则直接返回。
            //https://gitee.com/RRQM_Home/TouchSocket/issues/IBCUHW
            this.m_online = false;

            // 尝试移除与ID关联的操作
            if (this.m_tryRemoveAction(id, out var outClient))
            {
                if (!ReferenceEquals(this, outClient))
                {
                    //如果移除的客户端和当前实例不相等，则抛出异常。
                    //由于m_online的设计，一般这里不会被执行。
                    ThrowHelper.ThrowInvalidOperationException("移除的客户端和当前实例不相等，可能Id已经被修改。");
                }

                // 安全地释放MainSocket资源
                this.MainSocket.SafeDispose();
                // 安全地释放数据处理适配器资源
                this.m_dataHandlingAdapter.SafeDispose();
                // 启动一个新的任务，用于处理TCP关闭事件
                _ = Task.Factory.StartNew(this.PrivateOnTcpClosed, new ClosedEventArgs(manual, msg));
            }
        }
    }

    private async Task BeginReceive()
    {
        var byteBlock = new ByteBlock(this.m_tcpCore.ReceiveBufferSize);
        while (true)
        {
            try
            {
                var result = await this.m_tcpCore.ReadAsync(byteBlock.TotalMemory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (this.DisposedValue)
                {
                    byteBlock.Dispose();
                    return;
                }

                if (result.BytesTransferred > 0)
                {
                    byteBlock.SetLength(result.BytesTransferred);
                    try
                    {
                        if (await this.OnTcpReceiving(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                        {
                            continue;
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
                        this.Logger?.Exception(ex);
                    }
                    finally
                    {
                        if (byteBlock.Holding || byteBlock.DisposedValue)
                        {
                            byteBlock.Dispose();//释放上个内存
                            byteBlock = new ByteBlock(this.m_tcpCore.ReceiveBufferSize);
                        }
                        else
                        {
                            byteBlock.Reset();
                            if (this.m_tcpCore.ReceiveBufferSize > byteBlock.Capacity)
                            {
                                byteBlock.SetCapacity(this.m_tcpCore.ReceiveBufferSize);
                            }
                        }
                    }
                }
                else if (result.SocketError != null)
                {
                    byteBlock.Dispose();
                    this.Abort(false, result.SocketError.Message);
                    return;
                }
                else
                {
                    byteBlock.Dispose();
                    this.Abort(false, TouchSocketResource.RemoteDisconnects);
                    return;
                }
            }
            catch (Exception ex)
            {
                byteBlock.Dispose();
                this.Abort(false, ex.Message);
                return;
            }
        }
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

    private Task PrivateOnClosing(ClosingEventArgs e)
    {
        return this.OnTcpClosing(e);
    }

    private async Task PrivateOnTcpClosed(object obj)
    {
        try
        {
            var beginReceiveTask = this.m_beginReceiveTask;
            if (beginReceiveTask != null)
            {
                await beginReceiveTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            var e = (ClosedEventArgs)obj;

            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.Complete(e.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            await this.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
        finally
        {
            var tcp = this.m_tcpCore;
            this.m_tcpCore = null;
            this.m_returnTcpCore.Invoke(tcp);
            this.Dispose();
        }
    }

    #endregion 事件&委托

    /// <inheritdoc/>
    public virtual async Task CloseAsync(string msg)
    {
        if (this.m_online)
        {
            await this.PrivateOnClosing(new ClosingEventArgs(msg)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            lock (this.m_lockForAbort)
            {
                //https://gitee.com/RRQM_Home/TouchSocket/issues/IASH1A
                this.MainSocket.TryClose();
                this.Abort(true, msg);
            }
        }
    }

    /// <inheritdoc/>
    public virtual Task ResetIdAsync(string newId)
    {
        return this.ProtectedResetIdAsync(newId);
    }

    /// <inheritdoc/>
    public async Task<Result> ShutdownAsync(SocketShutdown how)
    {
        if (!this.m_online)
        {
            return Result.FromFail(TouchSocketResource.ClientNotConnected);
        }

        var tcpCore = this.m_tcpCore;
        if (tcpCore == null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(tcpCore)));
        }

        return await tcpCore.ShutdownAsync(how).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        base.Dispose(disposing);
        if (disposing)
        {
            this.m_scopedResolver.SafeDispose();
            this.Abort(true, TouchSocketResource.DisposeClose);
        }
    }

    /// <summary>
    /// 当Id更新的时候触发
    /// </summary>
    /// <param name="sourceId">原始Id</param>
    /// <param name="targetId">目标Id</param>
    /// <returns>异步任务</returns>
    protected virtual Task IdChanged(string sourceId, string targetId)
    {
        //此处无需执行任何操作，直接返回一个已完成的异步任务
        return EasyTask.CompletedTask;
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
    protected virtual ValueTask<bool> OnTcpReceiving(ByteBlock byteBlock)
    {
        // 将原始数据传递给所有相关的预处理插件，以进行初步的数据处理
        return this.PluginManager.RaiseAsync(typeof(ITcpReceivingPlugin), this.Resolver, this, new ByteBlockEventArgs(byteBlock));
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
    /// <returns>如果找到对应的客户端，则返回true；否则返回false</returns>
    protected bool ProtectedTryGetClient(string id, out TcpSessionClientBase sessionClient)
    {
        // 调用内部方法m_tryGet来尝试获取客户端
        return this.m_tryGet(id, out sessionClient);
    }

    /// <summary>
    /// 设置数据处理适配器。
    /// </summary>
    /// <param name="adapter">要设置的适配器实例。</param>
    /// <exception cref="ArgumentNullException">如果提供的适配器为null，则抛出此异常。</exception>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 检查适配器是否为null，如果是则抛出异常
        if (adapter is null)
        {
            throw new ArgumentNullException(nameof(adapter));
        }

        // 如果当前实例的配置不为空，则将配置应用到适配器
        if (this.Config != null)
        {
            adapter.Config(this.Config);
        }

        // 将当前实例的日志记录器和加载回调设置到适配器中
        adapter.Logger = this.Logger;
        adapter.OnLoaded(this);

        // 将接收到数据时的异步回调和发送数据时的异步回调设置到适配器中
        adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
        //adapter.SendCallBack = this.ProtectedDefaultSend;
        adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;

        // 将当前的数据处理适配器设置为传入的适配器实例
        this.m_dataHandlingAdapter = adapter;
    }

    private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        var receiver = this.m_receiver;
        if (receiver != null)
        {
            await receiver.InputReceiveAsync(byteBlock, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await this.OnTcpReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    #region 直接发送

    /// <summary>
    /// 异步发送数据，保护方法。
    /// 此方法用于在已建立的TCP连接上异步发送数据。
    /// 它首先检查当前实例是否已被处置，然后检查客户端是否已连接。
    /// 如果这些检查通过，它将调用OnTcpSending事件处理程序进行预发送处理，
    /// 最后通过TCP核心组件实际发送数据。
    /// </summary>
    /// <param name="memory">要发送的数据，存储在内存中。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    /// <exception cref="ObjectDisposedException">如果调用此方法的实例已被处置。</exception>
    /// <exception cref="InvalidOperationException">如果客户端未连接时抛出此异常。</exception>
    protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
    {
        // 检查实例是否已被处置
        this.ThrowIfDisposed();
        // 检查客户端是否已连接
        this.ThrowIfClientNotConnected();
        // 调用OnTcpSending事件处理程序进行预发送处理
        await this.OnTcpSending(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        // 通过TCP核心组件实际发送数据
        await this.m_tcpCore.SendAsync(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 直接发送

    #region 异步发送

    /// <summary>
    /// 异步发送只读内存数据。
    /// </summary>
    /// <param name="memory">要发送的只读内存块。</param>
    /// <returns>异步操作任务。</returns>
    protected Task ProtectedSendAsync(in ReadOnlyMemory<byte> memory)
    {
        // 如果数据处理适配器未初始化，则调用默认发送方法。
        if (this.m_dataHandlingAdapter == null)
        {
            return this.ProtectedDefaultSendAsync(memory);
        }
        else
        {
            // 否则，使用数据处理适配器发送数据。
            return this.m_dataHandlingAdapter.SendInputAsync(memory);
        }
    }

    /// <summary>
    /// 异步发送请求信息。
    /// </summary>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <returns>异步操作任务。</returns>
    protected Task ProtectedSendAsync(in IRequestInfo requestInfo)
    {
        // 在发送前验证是否能够发送请求信息。
        this.ThrowIfCannotSendRequestInfo();
        // 使用数据处理适配器发送请求信息。
        return this.m_dataHandlingAdapter.SendInputAsync(requestInfo);
    }

    /// <summary>
    /// 异步发送字节传输列表。
    /// </summary>
    /// <param name="transferBytes">包含字节数据的传输列表。</param>
    /// <returns>异步操作任务。</returns>
    protected async Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
    {
        // 检查数据处理适配器是否初始化，或者是否支持拼接发送。
        if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
        {
            // 计算所有传输字节的总长度。
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }
            // 使用总长度创建一个字节块，并将所有传输字节写入该块。
            using (var byteBlock = new ByteBlock(length))
            {
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
                }
                // 根据数据处理适配器是否初始化，调用默认发送方法或适配器的发送方法。
                if (this.m_dataHandlingAdapter == null)
                {
                    await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                else
                {
                    await this.m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        else
        {
            // 如果数据处理适配器支持拼接发送，则直接使用适配器发送传输列表。
            await this.m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    #endregion 异步发送

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