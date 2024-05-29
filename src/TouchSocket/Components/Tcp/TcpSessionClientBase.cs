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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// SessionClient
    /// </summary>
    [DebuggerDisplay("Id={Id},IP={IP},Port={Port}")]
    public abstract class TcpSessionClientBase : ResolverConfigObject, ITcpSession, ITcpListenableClient, IClient, IIdClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpSessionClientBase()
        {
            this.Protocol = Protocol.Tcp;
        }

        #region 变量

        private readonly object m_lock = new object();
        private Task m_beginReceiveTask;
        private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
        private string m_id;
        private string m_iP;
        private Socket m_mainSocket;
        private bool m_online;
        private IPluginManager m_pluginManager;
        private int m_port;
        private InternalReceiver m_receiver;
        private IResolver m_resolver;
        private Action<TcpCore> m_returnTcpCore;
        private ITcpServiceBase m_service;
        private string m_serviceIP;
        private int m_servicePort;
        private TcpCore m_tcpCore;
        private Func<TcpSessionClientBase, bool> m_tryAddAction;
        private TryOutEventHandler<TcpSessionClientBase> m_tryGet;
        private TryOutEventHandler<TcpSessionClientBase> m_tryRemoveAction;
        private TcpListenOption m_listenOption;

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
        public override IResolver Resolver => this.m_resolver;

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

            await this.OnTcpConnected(e).ConfigureFalseAwait();
        }

        internal async Task InternalConnecting(ConnectingEventArgs e)
        {
            await this.OnTcpConnecting(e).ConfigureFalseAwait();
            if (this.m_dataHandlingAdapter == null)
            {
                var adapter = this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty)?.Invoke();
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
            this.m_resolver = resolver;
            this.Logger ??= resolver.Resolve<ILog>();
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
        /// 中断连接
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void Abort(bool manual, string msg)
        {
            if (this.m_id == null)
            {
                return;
            }
            if (this.m_tryRemoveAction(this.m_id, out _))
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.MainSocket.SafeDispose();
                    this.m_dataHandlingAdapter.SafeDispose();
                    Task.Factory.StartNew(this.PrivateOnTcpClosed, new ClosedEventArgs(manual, msg));
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
                    var result = await this.m_tcpCore.ReadAsync(byteBlock.TotalMemory).ConfigureAwait(false);

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
                            if (await this.OnTcpReceiving(byteBlock).ConfigureAwait(false))
                            {
                                continue;
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
                            this.Logger.Exception(ex);
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
                    else if (result.HasError)
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
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnTcpClosed(ClosedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnTcpClosing(ClosingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当客户端完整建立Tcp连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnTcpConnected(ConnectedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected virtual Task OnTcpConnecting(ConnectingEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        private Task PrivateOnClosing(ClosingEventArgs e)
        {
            return this.OnTcpClosing(e);
        }

        private async Task PrivateOnTcpClosed(object obj)
        {
            await this.m_beginReceiveTask.ConfigureFalseAwait();

            var e = (ClosedEventArgs)obj;

            var receiver = this.m_receiver;
            if (receiver != null)
            {
                await receiver.Complete(e.Message).ConfigureFalseAwait();
            }

            try
            {
                await this.OnTcpClosed(e).ConfigureFalseAwait();
            }
            catch (Exception)
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
                await this.PrivateOnClosing(new ClosingEventArgs(msg)).ConfigureFalseAwait();
                this.MainSocket.TryClose();
                this.Abort(true, msg);
            }
        }

        /// <inheritdoc/>
        public virtual Task ResetIdAsync(string targetId)
        {
            return this.ProtectedResetId(targetId);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Abort(true, TouchSocketResource.DisposeClose);
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 当Id更新的时候触发
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        protected virtual Task IdChanged(string sourceId, string targetId)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected abstract Task OnTcpReceived(ReceivedDataEventArgs e);

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual ValueTask<bool> OnTcpReceiving(ByteBlock byteBlock)
        {
            return EasyValueTask.FromResult(false);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="memory"></param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual ValueTask<bool> OnTcpSending(ReadOnlyMemory<byte> memory)
        {
            return EasyValueTask.FromResult(true);
        }

        /// <summary>
        /// 直接重置内部Id。
        /// </summary>
        /// <param name="targetId"></param>
        protected async Task ProtectedResetId(string targetId)
        {
            this.ThrowIfDisposed();
            ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(targetId, nameof(targetId));
            if (this.m_id == targetId)
            {
                return;
            }
            var sourceId = this.m_id;
            if (this.m_tryRemoveAction(sourceId, out var socketClient))
            {
                socketClient.m_id = targetId;
                if (this.m_tryAddAction(socketClient))
                {
                    await this.IdChanged(sourceId, targetId).ConfigureFalseAwait();
                }
                else
                {
                    ThrowHelper.ThrowException(TouchSocketResource.IdAlreadyExists.Format(targetId));
                }
            }
            else
            {
                ThrowHelper.ThrowClientNotFindException(this.m_id);
            }
        }

        /// <summary>
        /// 尝试通过Id获得对应的客户端
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        protected bool ProtectedTryGetClient(string id, out TcpSessionClientBase socketClient)
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

        private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                await this.m_receiver.InputReceive(byteBlock, requestInfo).ConfigureFalseAwait();
                return;
            }
            await this.OnTcpReceived(new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureFalseAwait();
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

        ///// <inheritdoc/>
        //protected void ProtectedDefaultSend(byte[] buffer, int offset, int length)
        //{
        //    this.ThrowIfDisposed();
        //    this.ThrowIfClientNotConnected();
        //    if (this.OnTcpSending(buffer, offset, length).GetFalseAwaitResult())
        //    {
        //        this.m_tcpCore.Send(buffer, offset, length);
        //    }
        //}

        /// <inheritdoc/>
        protected async Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
        {
            this.ThrowIfDisposed();
            this.ThrowIfClientNotConnected();
            if (await this.OnTcpSending(memory).ConfigureAwait(false))
            {
                await this.m_tcpCore.SendAsync(memory).ConfigureFalseAwait();
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
        //                this.ProtectedDefaultSend(byteBlock.Buffer, 0, byteBlock.Length);
        //            }
        //            else
        //            {
        //                this.m_dataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Length);
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
            return this.m_dataHandlingAdapter == null
                ? this.ProtectedDefaultSendAsync(memory)
                : this.m_dataHandlingAdapter.SendInputAsync(memory);
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
                        byteBlock.Write(item.Array, item.Offset, item.Count);
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
    }
}