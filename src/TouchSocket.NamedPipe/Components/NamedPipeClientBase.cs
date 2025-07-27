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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道客户端客户端基类
/// </summary>
public abstract class NamedPipeClientBase : SetupConfigObject, INamedPipeSession
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

    protected virtual async Task ReceiveLoopAsync(ITransport transport)
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
                        if (await this.OnNamedPipeReceiving(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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
            return;
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

    private async Task PrivateConnected(NamedPipeTransport transport)
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
    public virtual async Task<Result> CloseAsync(string msg, CancellationToken token = default)
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
    /// 建立管道的连接。
    /// </summary>
    /// <param name="millisecondsTimeout"></param>
    /// <param name="token">可取消令箭</param>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="TimeoutException"></exception>
    protected async Task PipeConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        await this.m_semaphoreSlimForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

            await namedPipe.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

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
    protected virtual ValueTask<bool> OnNamedPipeReceiving(IByteBlockReader reader)
    {
        return this.PluginManager.RaiseAsync(typeof(INamedPipeReceivingPlugin), this.Resolver, this, new ByteBlockEventArgs(reader));
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

        ThrowHelper.ThrowArgumentNullExceptionIf(adapter, nameof(adapter));

        // 如果当前实例已有配置，则将配置应用到新适配器上
        if (this.Config != null)
        {
            adapter.Config(this.Config);
        }

        // 将当前实例的日志记录器设置到适配器上
        adapter.Logger = this.Logger;
        // 调用适配器的OnLoaded方法，通知适配器已被加载
        adapter.OnLoaded(this);
        // 设置适配器接收数据时的回调方法
        adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
        // 设置适配器发送数据时的异步回调方法
        adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;
        // 将适配器实例设置为当前数据处理适配器
        this.m_dataHandlingAdapter = adapter;
    }

    private static NamedPipeClientStream CreatePipeClient(string serverName, string pipeName)
    {
        var pipeClient = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);
        return pipeClient;
    }

    private async Task PrivateHandleReceivedData(IByteBlockReader byteBlock, IRequestInfo requestInfo)
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
    protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();
        await this.OnNamedPipeSending(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

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

    #endregion 直接发送

    #region 异步发送

    /// <summary>
    /// 异步发送数据，根据是否配置了数据处理适配器来决定数据的发送方式。
    /// </summary>
    /// <param name="memory">待发送的字节内存。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>一个异步任务，表示发送操作。</returns>
    protected Task ProtectedSendAsync(in ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        // 如果未配置数据处理适配器，则使用默认的发送方式。
        if (this.m_dataHandlingAdapter == null)
        {
            return this.ProtectedDefaultSendAsync(memory, token);
        }
        else
        {
            // 如果配置了数据处理适配器，则使用适配器指定的发送方式。
            return this.m_dataHandlingAdapter.SendInputAsync(memory, token);
        }
    }

    /// <summary>
    /// 异步安全发送请求信息。
    /// </summary>
    /// <param name="requestInfo">请求信息对象，包含要发送的数据。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，表示异步操作的结果。</returns>
    /// <remarks>
    /// 此方法用于在发送请求之前验证是否可以发送请求信息，
    /// 并通过<see cref="DataHandlingAdapter"/>适配器安全处理发送过程。
    /// </remarks>
    protected Task ProtectedSendAsync(IRequestInfo requestInfo, CancellationToken token)
    {
        // 验证当前状态是否允许发送请求信息，如果不允许则抛出异常。
        this.ThrowIfCannotSendRequestInfo();
        // 调用ProtectedDataHandlingAdapter的SendInputAsync方法异步发送请求信息。
        return this.m_dataHandlingAdapter.SendInputAsync(requestInfo, token);
    }

    #endregion 异步发送
}