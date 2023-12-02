//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务器辅助客户端类
    /// </summary>
    public class NamedPipeSocketClient : ConfigObject, INamedPipeSocketClient
    {
        #region 字段

        private NamedPipeServerStream m_pipeStream;
        private ValueCounter m_receiveCounter;
        private int m_receiveBufferSize = 1024 * 10;

        #endregion 字段

        /// <summary>
        /// 命名管道服务器辅助客户端类
        /// </summary>
        public NamedPipeSocketClient()
        {
            this.Protocol = Protocol.NamedPipe;
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnPeriod
            };
        }

        /// <inheritdoc/>
        public bool CanSend => this.Online;

        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <inheritdoc/>
        public override TouchSocketConfig Config => this.m_config;

        /// <inheritdoc/>
        public IResolver Resolver { get; private set; }

        /// <inheritdoc/>
        public string Id { get; private set; }

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime { get; private set; }

        /// <inheritdoc/>
        public bool Online { get; private set; }

        /// <inheritdoc/>
        public PipeStream PipeStream => this.m_pipeStream;

        /// <inheritdoc/>
        public IPluginManager PluginManager { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }

        /// <inheritdoc/>
        public NamedPipeServiceBase Service { get; private set; }

        #region Internal

        internal Task BeginReceive()
        {
            return this.BeginBio();
        }

        internal Task InternalConnected(ConnectedEventArgs e)
        {
            this.Online = true;
            return this.OnConnected(e);
        }

        internal Task InternalConnecting(ConnectingEventArgs e)
        {
            return this.OnConnecting(e);
        }

        internal Task InternalInitialized()
        {
            this.LastSendTime = DateTime.Now;
            return this.OnInitialized();
        }

        internal void InternalSetConfig(TouchSocketConfig config)
        {
            this.m_config = config;
        }

        internal void InternalSetContainer(IResolver containerProvider)
        {
            this.Resolver = containerProvider;
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
            this.PluginManager = pluginManager;
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

        /// <summary>
        /// 当客户端完整建立Tcp连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnected(ConnectedEventArgs e)
        {
            if (await this.PluginManager.RaiseAsync(nameof(INamedPipeConnectedPlugin.OnNamedPipeConnected), this, e))
            {
                return;
            }
            await this.Service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected virtual async Task OnConnecting(ConnectingEventArgs e)
        {
            if (await this.PluginManager.RaiseAsync(nameof(INamedPipeConnectingPlugin.OnNamedPipeConnecting), this, e))
            {
                return;
            }
            await this.Service.OnInternalConnecting(this, e);
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
        protected virtual async Task OnDisconnected(DisconnectEventArgs e)
        {
            if (this.Disconnected != null)
            {
                await this.Disconnected.Invoke(this, e);
                if (e.Handled)
                {
                    return;
                }
            }

            if (await this.PluginManager.RaiseAsync(nameof(INamedPipeDisconnectedPlugin.OnNamedPipeDisconnected), this, e))
            {
                return;
            }
            await this.Service.OnInternalDisconnected(this, e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnecting(DisconnectEventArgs e)
        {
            try
            {
                if (this.Disconnecting != null)
                {
                    await this.Disconnecting.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                if (await this.PluginManager.RaiseAsync(nameof(INamedPipeDisconnectingPlugin.OnNamedPipeDisconnecting), this, e))
                {
                    return;
                }
                await this.Service.OnInternalDisconnecting(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 当初始化完成时，执行在<see cref="OnConnecting(ConnectingEventArgs)"/>之前。
        /// </summary>
        protected virtual Task OnInitialized()
        {
            return EasyTask.CompletedTask;
        }

        private async Task BeginBio()
        {
            while (true)
            {
                var byteBlock = new ByteBlock(this.m_receiveBufferSize);
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

        private Task PrivateOnDisconnected(DisconnectEventArgs e)
        {
            this.m_receiver?.TryInputReceive(default, default);
            return this.OnDisconnected(e);
        }

        private async Task PrivateOnDisconnecting(object obj)
        {
            var e = (DisconnectEventArgs)obj;
            await this.OnDisconnecting(e);
        }

        #endregion 事件&委托

        #region Receiver

        private Receiver m_receiver;
        private TouchSocketConfig m_config;
        private object SyncRoot = new object();

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.m_receiver = null;
        }

        /// <inheritdoc/>
        public IReceiver CreateReceiver()
        {
            return this.m_receiver ??= new Receiver(this);
        }

        #endregion Receiver

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {
            lock (this.SyncRoot)
            {
                if (this.Online)
                {
                    this.m_pipeStream.Close();
                    Task.Factory.StartNew(this.PrivateOnDisconnecting, new DisconnectEventArgs(true, msg));
                    this.BreakOut(msg, true);
                }
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
                    if (this.PluginManager.Enable)
                    {
                        var e = new IdChangedEventArgs(oldId, newId);
                        this.PluginManager.Raise(nameof(IIdChangedPlugin.OnIdChanged), socketClient, e);
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
                    Task.Factory.StartNew(this.PrivateOnDisconnecting, new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));

                    this.BreakOut($"{nameof(Dispose)}主动断开", true);
                }
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// 当即将发送时。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual async Task<bool> SendingData(byte[] buffer, int offset, int length)
        {
            if (this.PluginManager.GetPluginCount(nameof(INamedPipeSendingPlugin.OnNamedPipeSending)) > 0)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                await this.PluginManager.RaiseAsync(nameof(INamedPipeSendingPlugin.OnNamedPipeSending), this, args).ConfigureAwait(false);
                return args.IsPermitOperation;
            }
            return true;
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        protected virtual async Task ReceivedData(ReceivedDataEventArgs e)
        {
            await this.Service.OnInternalReceivedData(this, e);
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(nameof(INamedPipeReceivedPlugin.OnNamedPipeReceived), this, e);
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task<bool> ReceivingData(ByteBlock byteBlock)
        {
            if (this.PluginManager.GetPluginCount(nameof(INamedPipeReceivingPlugin.OnNamedPipeReceiving)) > 0)
            {
                return this.PluginManager.RaiseAsync(nameof(INamedPipeReceivingPlugin.OnNamedPipeReceiving), this, new ByteBlockEventArgs(byteBlock));
            }
            return Task.FromResult(false);
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
            adapter.SendAsyncCallBack = this.DefaultSendAsync;
            this.DataHandlingAdapter = adapter;
        }

        /// <summary>
        /// 中断连接
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="manual"></param>
        protected void BreakOut(string msg, bool manual)
        {
            if (this.GetSocketCliectCollection().TryRemove(this.Id, out _))
            {
                if (this.Online)
                {
                    this.Online = false;
                    this.m_pipeStream.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg));
                }
            }
            base.Dispose(true);
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

                this.m_receiveCounter.Increment(byteBlock.Length);
                if (this.ReceivingData(byteBlock).ConfigureAwait(false).GetAwaiter().GetResult())
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

        private void OnPeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                if (this.m_receiver.TryInputReceive(byteBlock, requestInfo))
                {
                    return;
                }
            }
            this.ReceivedData(new ReceivedDataEventArgs(byteBlock, requestInfo)).GetFalseAwaitResult();
        }

        #region 发送

        /// <inheritdoc/>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            if (this.SendingData(buffer, offset, length).GetFalseAwaitResult())
            {
                this.m_pipeStream.Write(buffer, offset, length);
                this.LastSendTime = DateTime.Now;
            }
        }

        /// <inheritdoc/>
        public async Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            if (await this.SendingData(buffer, offset, length))
            {
                await this.m_pipeStream.WriteAsync(buffer, offset, length);
                this.LastSendTime = DateTime.Now;
            }
        }

        #region 同步发送

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual async Task SendAsync(byte[] buffer, int offset, int length)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            await this.DataHandlingAdapter.SendInputAsync(buffer, offset, length);
        }

        /// <inheritdoc/>
        public virtual async Task SendAsync(IRequestInfo requestInfo)
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
            await this.DataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <inheritdoc/>
        public virtual async Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThrowIfDisposed();
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
                    await this.DataHandlingAdapter.SendInputAsync(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
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