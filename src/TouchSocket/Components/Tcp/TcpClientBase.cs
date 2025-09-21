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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// TcpClientBase 抽象基类，封装了TCP客户端的核心功能，包括连接、断开、数据收发、适配器设置、事件触发等。
/// 该类实现了 ITcpSession 接口，并继承自 SetupConfigObject。
/// </summary>
public abstract partial class TcpClientBase : SetupConfigObject, ITcpSession
{
    /// <summary>
    /// 构造函数用于初始化TcpClientBase类的实例。
    /// </summary>
    public TcpClientBase()
    {
        // 设置协议为TCP
        this.Protocol = Protocol.Tcp;
    }

    #region 变量

    private readonly TcpCore m_tcpCore = new TcpCore();
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    private string m_iP;
    private volatile bool m_online;
    private int m_port;
    private InternalReceiver m_receiver;
    private Task m_runTask;
    private TcpTransport m_transport;

    #endregion 变量

    #region 事件

    /// <summary>
    /// 在连接断开时触发。
    /// <para>
    /// 如果重写此方法，则不会触发<see cref="ITcpClosedPlugin"/>插件。
    /// </para>
    /// </summary>
    /// <param name="e">包含断开连接相关信息的事件参数</param>
    protected virtual async Task OnTcpClosed(ClosedEventArgs e)
    {
        // 调用插件管理器，触发所有ITcpClosedPlugin类型的插件
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
    /// 建立Tcp连接时触发。
    /// <para>
    /// 覆盖父类方法，将不会触发<see cref="ITcpConnectedPlugin"/>插件。
    /// </para>
    /// </summary>
    /// <param name="e">连接事件参数</param>
    protected virtual async Task OnTcpConnected(ConnectedEventArgs e)
    {
        // 调用插件管理器，异步触发所有ITcpConnectedPlugin类型的插件
        await this.PluginManager.RaiseAsync(typeof(ITcpConnectedPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接。
    /// <para>
    /// 覆盖父类方法，将不会触发<see cref="ITcpConnectingPlugin"/>插件。
    /// </para>
    /// </summary>
    /// <param name="e">连接事件参数</param>
    protected virtual async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        // 调用插件管理器，触发ITcpConnectingPlugin类型的插件进行相应操作
        await this.PluginManager.RaiseAsync(typeof(ITcpConnectingPlugin), this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task PrivateOnConnected(TcpTransport transport)
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

        await this.OnTcpClosed(e_closed).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task PrivateOnTcpClosing(ClosingEventArgs e)
    {
        return this.OnTcpClosing(e);
    }

    private async Task PrivateOnTcpConnecting(ConnectingEventArgs e)
    {
        await this.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (this.m_dataHandlingAdapter == null)
        {
            var adapter = this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty)?.Invoke();
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
    public string IP => this.m_iP;

    /// <inheritdoc/>
    public bool IsClient => true;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_transport == null ? default : this.m_transport.ReceiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_transport == null ? default : this.m_transport.SendCounter.LastIncrement;

    /// <inheritdoc/>
    public virtual bool Online => this.m_online;

    /// <inheritdoc/>
    public int Port => this.m_port;

    /// <inheritdoc/>
    public Protocol Protocol { get; protected set; }

    /// <summary>
    /// 远程IPHost
    /// </summary>
    public IPHost RemoteIPHost => this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);

    /// <inheritdoc/>
    public bool UseSsl => this.m_transport.UseSsl;

    /// <summary>
    /// 获取当前TCP传输层对象。
    /// </summary>
    protected ITransport Transport => this.m_transport;

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
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            _ = EasyTask.SafeRun(async () => await this.CloseAsync(TouchSocketResource.DisposeClose).ConfigureAwait(EasyTask.ContinueOnCapturedContext));
            this.m_tcpCore.SafeDispose();
        }

        base.SafetyDispose(disposing);
    }

    #endregion 断开操作

    protected Task AuthenticateAsync(ClientSslOption sslOption)
    {
        return this.m_transport.AuthenticateAsync(sslOption);
    }

    /// <summary>
    /// 当收到适配器处理的数据时。
    /// </summary>
    /// <param name="e">包含接收到的数据事件的相关信息。</param>
    protected virtual async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        await this.PluginManager.RaiseITcpReceivedPluginAsync(this.Resolver, this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当收到原始数据时，触发相关插件进行处理。
    /// </summary>
    /// <param name="reader">包含收到的原始数据的字节块。</param>
    /// <returns>
    /// 如果返回<see langword="true"/>，则表示数据已被处理，且不会再向下传递。
    /// </returns>
    protected virtual ValueTask<bool> OnTcpReceiving(IBytesReader reader)
    {
        return this.PluginManager.RaiseITcpReceivingPluginAsync(this.Resolver, this, new BytesReaderEventArgs(reader));
    }

    /// <summary>
    /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
    /// </summary>
    /// <param name="memory">待发送的数据，以只读内存形式提供。</param>
    /// <returns>返回值意义：表示是否继续发送数据的指示，true为继续，false为取消发送。</returns>
    protected virtual ValueTask<bool> OnTcpSending(ReadOnlyMemory<byte> memory)
    {
        return this.PluginManager.RaiseITcpSendingPluginAsync(this.Resolver, this, new SendingEventArgs(memory));
    }

    #region ReceiveLoopAsync
    /// <summary>
    /// 数据接收主循环，负责从传输层读取数据并分发给适配器或插件。
    /// </summary>
    /// <param name="transport">传输层对象</param>
    protected virtual async Task ReceiveLoopAsync(ITransport transport)
    {
        var token = transport.ClosedToken;

        await transport.ReadLocker.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

                try
                {
                    var reader = new ClassBytesReader(result.Buffer);
                    if (!await this.OnTcpReceiving(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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
                    var position = result.Buffer.GetPosition(reader.BytesRead);
                    transport.Reader.AdvanceTo(position, result.Buffer.End);

                    if (result.IsCompleted || result.IsCanceled)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.Logger?.Exception(this, ex);

                    // 处理数据出现异常时，关闭连接并退出循环
                    await transport.CloseAsync(ex.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    break; // 关闭连接后退出循环
                }
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
            receiver?.Complete(e_closed.Message);
            transport.ReadLocker.Release();
        }
    }
    #endregion

    /// <summary>
    /// 设置适配器。
    /// </summary>
    /// <param name="adapter">要设置的适配器实例。</param>
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

    private void SetSocket(Socket socket)
    {
        if (socket == null)
        {
            this.m_iP = null;
            this.m_port = -1;
            return;
        }

        this.m_iP = socket.RemoteEndPoint.GetIP();
        this.m_port = socket.RemoteEndPoint.GetPort();
        this.m_tcpCore.Reset(socket);
    }

    private async Task TryAuthenticateAsync(IPHost iPHost)
    {
        if (!this.Config.TryGetValue(TouchSocketConfigExtension.ClientSslOptionProperty, out var sslOption))
        {
            if (!iPHost.IsSsl)
            {
                return;
            }

            sslOption = new ClientSslOption()
            {
                TargetHost = iPHost.Host
            };
        }
        await this.AuthenticateAsync(sslOption).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #region Receiver

    /// <summary>
    /// 清除内部接收器。
    /// </summary>
    protected void ProtectedClearReceiver()
    {
        this.m_receiver = null;
    }

    /// <summary>
    /// 创建内部接收器。
    /// </summary>
    /// <param name="receiverObject">接收器客户端对象</param>
    /// <returns>接收器实例</returns>
    protected IReceiver<IReceiverResult> ProtectedCreateReceiver(IReceiverClient<IReceiverResult> receiverObject)
    {
        return this.m_receiver ??= new InternalReceiver(receiverObject);
    }

    #endregion Receiver

    #region Throw

    /// <summary>
    ///  如果TCP客户端未连接，则抛出异常。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfClientNotConnected()
    {
        if (this.m_online)
        {
            return;
        }

        ThrowHelper.ThrowClientNotConnectedException();
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
}