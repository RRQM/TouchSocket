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

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务器辅助客户端类
    /// </summary>
    [DebuggerDisplay("Id={Id},IP={IP},Port={Port}")]
    public abstract class NamedPipeSessionClientBase : ResolverConfigObject, INamedPipeSession, INamedPipeListenableClient, IIdClient
    {
        #region 字段

        private readonly SemaphoreSlim m_semaphoreSlimForSend = new SemaphoreSlim(1, 1);
        private readonly object m_syncRoot = new object();
        private TouchSocketConfig m_config;
        private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
        private bool m_online;
        private NamedPipeServerStream m_pipeStream;
        private IPluginManager m_pluginManager;
        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private InternalReceiver m_receiver;
        private Task m_receiveTask;
        private IResolver m_resolver;
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
        public virtual bool Online => this.m_online;

        /// <inheritdoc/>
        public PipeStream PipeStream => this.m_pipeStream;

        /// <inheritdoc/>
        public override IPluginManager PluginManager => this.m_pluginManager;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; }

        /// <inheritdoc/>
        public override IResolver Resolver => this.m_resolver;

        /// <inheritdoc/>
        public INamedPipeServiceBase Service => this.m_service;

        /// <inheritdoc/>
        protected SingleStreamDataHandlingAdapter ProtectedDataHandlingAdapter => this.m_dataHandlingAdapter;

        #region Internal

        internal Task InternalInitialized()
        {
            this.LastSentTime = DateTime.Now;
            return this.OnInitialized();
        }

        internal async Task InternalNamedPipeConnected(ConnectedEventArgs e)
        {
            this.m_online = true;

            this.m_receiveTask =Task.Factory.StartNew(this.BeginReceive, TaskCreationOptions.LongRunning).Unwrap();
            this.m_receiveTask.FireAndForget();

            await this.OnNamedPipeConnected(e).ConfigureFalseAwait();
        }

        internal async Task InternalNamedPipeConnecting(ConnectingEventArgs e)
        {
            await this.OnNamedPipeConnecting(e).ConfigureFalseAwait();
            if (this.m_dataHandlingAdapter == null)
            {
                var adapter = this.Config.GetValue(NamedPipeConfigExtension.NamedPipeDataHandlingAdapterProperty)?.Invoke();
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

        internal void InternalSetContainer(IResolver containerProvider)
        {
            this.m_resolver = containerProvider;
            this.Logger ??= containerProvider.Resolve<ILog>();
        }

        internal void InternalSetId(string id)
        {
            this.Id = id;
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
                await this.PrivateOnNamedPipeClosing(new ClosingEventArgs(msg)).ConfigureFalseAwait();
                this.m_pipeStream.Close();
                this.Abort(true, msg);
            }
        }

        /// <inheritdoc/>
        public virtual Task ResetIdAsync(string newId)
        {
           return this.ProtectedResetId(newId);
        }

        /// <summary>
        /// 中断连接
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="manual"></param>
        protected void Abort(bool manual, string msg)
        {
            if (this.m_tryRemoveAction(this.Id, out _))
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_pipeStream.SafeDispose();
                    this.ProtectedDataHandlingAdapter.SafeDispose();

                    Task.Factory.StartNew(this.PrivateOnNamedPipeClosed, new ClosedEventArgs(manual, msg));
                }
            }
            base.Dispose(true);
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

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        protected virtual Task OnNamedPipeReceived(ReceivedDataEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual ValueTask<bool> OnNamedPipeReceiving(ByteBlock byteBlock)
        {
            return EasyValueTask.FromResult(false);
        }

        /// <summary>
        /// 当即将发送时。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual ValueTask<bool> OnNamedPipeSending(ReadOnlyMemory<byte> memory)
        {
            return EasyValueTask.FromResult(true);
        }

        /// <summary>
        /// 直接重置内部Id。
        /// </summary>
        /// <param name="newId"></param>
        protected async Task ProtectedResetId(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (this.Id == newId)
            {
                return;
            }
            var sourceId = this.Id;
            if (this.m_tryRemoveAction(this.Id, out var socketClient))
            {
                socketClient.Id = newId;
                if (this.m_tryAddAction(socketClient))
                {
                    if (this.PluginManager.Enable)
                    {
                        var e = new IdChangedEventArgs(sourceId, newId);
                        await this.PluginManager.RaiseAsync(typeof(IIdChangedPlugin), socketClient, e).ConfigureFalseAwait();
                    }
                    return;
                }
                else
                {
                    socketClient.Id = sourceId;
                    if (this.m_tryAddAction(socketClient))
                    {
                        throw new Exception("Id重复");
                    }
                    else
                    {
                        await socketClient.CloseAsync("修改新Id时操作失败，且回退旧Id时也失败。").ConfigureFalseAwait();
                    }
                }
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.Format(sourceId));
            }
        }

        /// <summary>
        /// 尝试通过Id获得对应的客户端
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        protected bool ProtectedTryGetClient(string id, out NamedPipeSessionClientBase socketClient)
        {
            return this.m_tryGet(id, out socketClient);
        }

        /// <summary>
        /// 设置适配器
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }

            adapter.Logger = this.Logger;
            adapter.OnLoaded(this);
            adapter.ReceivedAsyncCallBack = this.PrivateHandleReceivedData;
            //adapter.SendCallBack = this.ProtectedDefaultSend;
            adapter.SendAsyncCallBack = this.ProtectedDefaultSendAsync;
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
                        var r = await this.m_pipeStream.ReadAsync(byteBlock.TotalMemory, CancellationToken.None).ConfigureAwait(false);
                        if (r == 0)
                        {
                            this.Abort(false, "远程终端主动关闭");
                            return;
                        }

                        byteBlock.SetLength(r);
                        await this.HandleReceivingData(byteBlock).ConfigureFalseAwait();
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
        protected virtual Task OnNamedPipeClosed(ClosedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeClosing(ClosingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当客户端完整建立Tcp连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeConnected(ConnectedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected virtual Task OnNamedPipeConnecting(ConnectingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        private int GetReceiveBufferSize()
        {
            var minBufferSize = this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) ?? 1024 * 10;
            var maxBufferSize = this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) ?? 1024 * 512;
            return Math.Min(Math.Max(this.m_receiveBufferSize, minBufferSize), maxBufferSize);
        }

        private async Task PrivateOnNamedPipeClosed(object obj)
        {
            var e = (ClosedEventArgs)obj;
            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.Complete(e.Message).ConfigureFalseAwait();
            }
            await this.OnNamedPipeClosed(e).ConfigureFalseAwait();
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

                if (await this.OnNamedPipeReceiving(byteBlock).ConfigureAwait(false))
                {
                    return;
                }

                if (this.m_dataHandlingAdapter == null)
                {
                    await this.PrivateHandleReceivedData(byteBlock, default).ConfigureFalseAwait();
                }
                else
                {
                    await this.m_dataHandlingAdapter.ReceivedInputAsync(byteBlock).ConfigureFalseAwait();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
            }
        }

        private void OnPeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
        }

        private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                await this.m_receiver.InputReceive(byteBlock, requestInfo).ConfigureFalseAwait();
                return;
            }
            await this.OnNamedPipeReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureFalseAwait();
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

        ///// <inheritdoc/>
        //protected void ProtectedDefaultSend(byte[] buffer, int offset, int length)
        //{
        //    this.ThrowIfDisposed();
        //    this.ThrowIfClientNotConnected();
        //    if (this.OnNamedPipeSending(buffer, offset, length).GetFalseAwaitResult())
        //    {
        //        try
        //        {
        //            this.m_semaphoreSlimForSend.Wait();
        //            this.m_pipeStream.Write(buffer, offset, length);
        //            this.LastSendTime = DateTime.Now;
        //        }
        //        finally
        //        {
        //            this.m_semaphoreSlimForSend.Release();
        //        }
        //    }
        //}

        /// <inheritdoc/>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThrowIfDisposed();
            this.ThrowIfClientNotConnected();
            if (await this.OnNamedPipeSending(memory).ConfigureAwait(false))
            {
                try
                {
                    await this.m_semaphoreSlimForSend.WaitAsync();
#if NET6_0_OR_GREATER
                    await this.m_pipeStream.WriteAsync(memory, CancellationToken.None).ConfigureAwait(false);
#else
                    var segment = memory.GetArray();
                    await this.m_pipeStream.WriteAsync(segment.Array, segment.Offset, segment.Count).ConfigureFalseAwait();
#endif
                    this.LastSentTime = DateTime.Now;
                }
                finally
                {
                    this.m_semaphoreSlimForSend.Release();
                }
            }
        }

        #endregion 直接发送

        //#region 同步发送

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="requestInfo"></param>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected void ProtectedSend(IRequestInfo requestInfo)
        //{
        //    this.ThrowIfCannotSendRequestInfo();
        //    this.m_dataHandlingAdapter.SendInput(requestInfo);
        //}

        ///// <summary>
        ///// 发送字节流
        ///// </summary>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected void ProtectedSend(byte[] buffer, int offset, int length)
        //{
        //    if (this.m_dataHandlingAdapter == null)
        //    {
        //        this.ProtectedDefaultSend(buffer, offset, length);
        //    }
        //    else
        //    {
        //        this.m_dataHandlingAdapter.SendInput(buffer, offset, length);
        //    }
        //}

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="transferBytes"></param>
        //protected void ProtectedSend(IList<ArraySegment<byte>> transferBytes)
        //{
        //    if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
        //    {
        //        var length = 0;
        //        foreach (var item in transferBytes)
        //        {
        //            length += item.Count;
        //        }
        //        using (var byteBlock = new ByteBlock(length))
        //        {
        //            foreach (var item in transferBytes)
        //            {
        //                byteBlock.Write(item.Array, item.Offset, item.Count);
        //            }

        //            if (this.m_dataHandlingAdapter == null)
        //            {
        //                this.ProtectedDefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
        //            }
        //            else
        //            {
        //                this.m_dataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        this.m_dataHandlingAdapter.SendInput(transferBytes);
        //    }
        //}

        //#endregion 同步发送

        #region 异步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected Task ProtectedSendAsync(ReadOnlyMemory<byte> memory)
        {
            if (this.m_dataHandlingAdapter == null)
            {
                return this.ProtectedDefaultSendAsync(memory);
            }
            else
            {
                return this.m_dataHandlingAdapter.SendInputAsync(memory);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected Task ProtectedSendAsync(IRequestInfo requestInfo)
        {
            this.ThrowIfCannotSendRequestInfo();
            return this.m_dataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task ProtectedSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.m_dataHandlingAdapter == null || !this.m_dataHandlingAdapter.CanSplicingSend)
            {
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }
                using (var byteBlock = new ByteBlock(length))
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
                    }
                    if (this.m_dataHandlingAdapter == null)
                    {
                        await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureFalseAwait();
                    }
                    else
                    {
                        await this.m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureFalseAwait();
                    }
                }
            }
            else
            {
                await this.m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureFalseAwait();
            }
        }

        #endregion 异步发送
    }
}