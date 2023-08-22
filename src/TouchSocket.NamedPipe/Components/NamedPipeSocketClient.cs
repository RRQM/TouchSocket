using System;
using System.Collections.Generic;
using System.IO.Pipes;
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
    public class NamedPipeSocketClient : BaseSocket, INamedPipeSocketClient
    {
        private object m_delaySender;

        private NamedPipeServerStream m_pipeStream;

        /// <summary>
        /// 命名管道服务器辅助客户端类
        /// </summary>
        public NamedPipeSocketClient()
        {
            this.Protocol = Protocol.NamedPipe;
        }

        /// <inheritdoc/>
        public bool CanSend => this.Online;

        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <inheritdoc/>
        public TouchSocketConfig Config { get; private set; }

        /// <inheritdoc/>
        public IContainer Container { get; private set; }

        /// <inheritdoc/>
        public string Id { get; private set; }

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public DateTime LastReceivedTime { get; private set; }

        /// <inheritdoc/>
        public DateTime LastSendTime { get; private set; }

        /// <inheritdoc/>
        public bool Online { get; private set; }

        /// <inheritdoc/>
        public PipeStream PipeStream => this.m_pipeStream;

        /// <inheritdoc/>
        public IPluginsManager PluginsManager { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }

        /// <inheritdoc/>
        public NamedPipeServiceBase Service { get; private set; }

        #region Internal

        internal void BeginReceive()
        {
            //new Thread(BeginBio)
            //{
            //    IsBackground = true
            //}
            //    .Start();

            Task.Run(this.BeginBio);
        }

        internal void InternalConnected(ConnectedEventArgs e)
        {
            this.Online = true;

            this.OnConnected(e);

            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(INamedPipeConnectedPlugin.OnNamedPipeConnected), this, e))
            {
                return;
            }
        }

        internal void InternalConnecting(ConnectingEventArgs e)
        {
            this.OnConnecting(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(INamedPipeConnectingPlugin.OnNamedPipeConnecting), this, e))
            {
                return;
            }
        }

        internal void InternalInitialized()
        {
            this.LastReceivedTime = DateTime.Now;
            this.LastSendTime = DateTime.Now;
            this.OnInitialized();
        }

        internal void InternalSetConfig(TouchSocketConfig config)
        {
            this.Config = config;
        }

        internal void InternalSetContainer(IContainer container)
        {
            this.Container = container;
            this.Logger ??= container.Resolve<ILog>();
        }

        internal void InternalSetId(string id)
        {
            this.Id = id;
        }

        internal void InternalSetNamedPipe(NamedPipeServerStream namedPipe)
        {
            this.m_pipeStream = namedPipe;
        }

        internal void InternalSetPluginsManager(IPluginsManager pluginsManager)
        {
            this.PluginsManager = pluginsManager;
        }

        internal void InternalSetService(NamedPipeServiceBase serviceBase)
        {
            this.Service = serviceBase;
        }

        #endregion Internal

        #region 事件&委托

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<INamedPipeClientBase> Disconnected { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<INamedPipeClientBase> Disconnecting { get; set; }

        /// <inheritdoc/>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <inheritdoc/>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <summary>
        /// 当客户端完整建立TCP连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(ConnectedEventArgs e)
        {
            this.Service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected virtual void OnConnecting(ConnectingEventArgs e)
        {
            this.Service.OnInternalConnecting(this, e);
        }

        /// <summary>
        /// 在延迟发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnDelaySenderError(Exception ex)
        {
            this.Logger.Log(LogLevel.Error, this, "发送错误", ex);
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(DisconnectEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnecting(DisconnectEventArgs e)
        {
            try
            {
                this.Disconnecting?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 当初始化完成时，执行在<see cref="OnConnecting(ConnectingEventArgs)"/>之前。
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        private async Task BeginBio()
        {
            while (true)
            {
                var byteBlock = new ByteBlock(this.BufferLength);
                try
                {
                    var r = await Task<int>.Factory.FromAsync(this.m_pipeStream.BeginRead, this.m_pipeStream.EndRead, byteBlock.Buffer, 0, byteBlock.Capacity, null);
                    if (r == 0)
                    {
                        this.BreakOut("远程终端主动关闭", false);
                        return;
                    }

                    byteBlock.SetLength(r);
                    this.HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message, false);
                    return;
                }
            }
        }

        private void PrivateOnDisconnected(DisconnectEventArgs e)
        {
            this.OnDisconnected(e);
            if (e.Handled)
            {
                return;
            }

            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(INamedPipeDisconnectedPlugin.OnNamedPipeDisconnected), this, e))
            {
                return;
            }

            if (!e.Handled)
            {
                this.Service.OnInternalDisconnected(this, e);
            }
        }

        private void PrivateOnDisconnecting(DisconnectEventArgs e)
        {
            this.OnDisconnecting(e);
            if (e.Handled)
            {
                return;
            }

            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(INamedPipeDisconnectingPlugin.OnNamedPipeDisconnecting), this, e))
            {
                return;
            }
            this.Service.OnInternalDisconnecting(this, e);
        }

        #endregion 事件&委托

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {
            lock (this.SyncRoot)
            {
                if (this.Online)
                {
                    this.m_pipeStream.Close();
                    this.m_pipeStream.SafeDispose();
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, msg));
                }
                this.BreakOut(msg, true);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="newId"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void ResetId(string newId)
        {
            this.DirectResetId(newId);
        }

        /// <inheritdoc/>
        public virtual void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <summary>
        /// 直接重置内部Id。
        /// </summary>
        /// <param name="newId"></param>
        protected void DirectResetId(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (this.Id == newId)
            {
                return;
            }
            var oldId = this.Id;
            if (this.GetSocketCliectCollection().TryRemove(this.Id, out NamedPipeSocketClient socketClient))
            {
                socketClient.Id = newId;
                if (this.GetSocketCliectCollection().TryAdd(socketClient))
                {
                    if (this.PluginsManager.Enable)
                    {
                        var e = new IdChangedEventArgs(oldId, newId);
                        this.PluginsManager.Raise(nameof(IIdChangedPlugin.OnIdChanged), socketClient, e);
                    }
                    return;
                }
                else
                {
                    socketClient.Id = oldId;
                    if (this.GetSocketCliectCollection().TryAdd(socketClient))
                    {
                        throw new Exception("Id重复");
                    }
                    else
                    {
                        socketClient.Close("修改新Id时操作失败，且回退旧Id时也失败。");
                    }
                }
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(oldId));
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (this.SyncRoot)
            {
                if (this.Online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));
                }
                this.BreakOut($"{nameof(Dispose)}主动断开", true);
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            return false;
        }

        /// <summary>
        /// 当即将发送时。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual bool HandleSendingData(byte[] buffer, int offset, int length)
        {
            if (this.PluginsManager.Enable)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                this.PluginsManager.Raise(nameof(INamedPipeSendingPlugin.OnNamedPipeSending), this, args);
                return args.IsPermitOperation;
            }
            return true;
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
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
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.DefaultSend;
            this.DataHandlingAdapter = adapter;
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this.SyncRoot)
            {
                if (this.GetSocketCliectCollection().TryRemove(this.Id, out _))
                {
                    if (this.Online)
                    {
                        this.Online = false;
                        this.m_pipeStream.SafeDispose();
                        //this.m_delaySender.SafeDispose();
                        this.DataHandlingAdapter.SafeDispose();
                        this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg));
                    }
                }

                base.Dispose(true);
            }
        }

        private NamedPipeSocketClientCollection GetSocketCliectCollection()
        {
            return this.Service?.SocketClients as NamedPipeSocketClientCollection;
        }

        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }

                this.LastReceivedTime = DateTime.Now;
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(INamedPipeReceivingPlugin.OnNamedPipeReceiving), this, new ByteBlockEventArgs(byteBlock)))
                {
                    return;
                }
                if (this.DataHandlingAdapter == null)
                {
                    this.Logger.Error(this, TouchSocketResource.NullDataAdapter.GetDescription());
                    return;
                }
                this.DataHandlingAdapter.ReceivedInput(byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }

            if (this.HandleReceivedData(byteBlock, requestInfo))
            {
                return;
            }

            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(INamedPipeReceivedPlugin.OnNamedPipeReceived), this, new ReceivedDataEventArgs(byteBlock, requestInfo)))
            {
                return;
            }

            this.Service.OnInternalReceivedData(this, byteBlock, requestInfo);
        }

        #region 发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            if (!this.CanSend)
            {
                throw new NotConnectedException(TouchSocketResource.NotConnected.GetDescription());
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                this.m_pipeStream.Write(buffer, offset, length);
                //if (this.m_delaySender != null && length < this.m_delaySender.DelayLength)
                //{
                //    this.m_delaySender.Send(QueueDataBytes.CreateNew(buffer, offset, length));
                //}
                //else
                //{
                //    this.MainSocket.AbsoluteSend(buffer, offset, length);
                //}
                this.LastSendTime = DateTime.Now;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.DefaultSend(buffer, offset, length);
            });
        }

        #region 同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(IRequestInfo requestInfo)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (!this.DataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.DataHandlingAdapter.SendInput(requestInfo);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            this.DataHandlingAdapter.SendInput(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (this.DataHandlingAdapter.CanSplicingSend)
            {
                this.DataHandlingAdapter.SendInput(transferBytes);
            }
            else
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
                    this.DataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
        }

        #endregion 同步发送

        #region 异步发送

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.Send(buffer, offset, length);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return Task.Run(() =>
            {
                this.Send(requestInfo);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return Task.Run(() =>
            {
                this.Send(transferBytes);
            });
        }

        #endregion 异步发送

        #region Id发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(string id, byte[] buffer, int offset, int length)
        {
            this.Service.Send(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void Send(string id, IRequestInfo requestInfo)
        {
            this.Service.Send(id, requestInfo);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public Task SendAsync(string id, byte[] buffer, int offset, int length)
        {
            return this.Service.SendAsync(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            return this.Service.SendAsync(id, requestInfo);
        }

        #endregion Id发送

        #endregion 发送
    }
}