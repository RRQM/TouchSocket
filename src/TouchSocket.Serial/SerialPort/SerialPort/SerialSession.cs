
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;
namespace TouchSocket.Serial
{


    /// <inheritdoc cref="SerialSessionBase"/>
    public class SerialSession : SerialSessionBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public ReceivedEventHandler<SerialSession> Received { get; set; }

        /// <inheritdoc/>
        protected override Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(this, e);
            }
            return base.ReceivedData(e);
        }
    }

    /// <summary>
    /// 串口管理
    /// </summary>
    public class SerialSessionBase : SetupConfigObject, ISerialSession
    {
        static readonly Protocol SerialPort = new Protocol("SerialSession");
        /// <summary>
        /// 构造函数
        /// </summary>
        public SerialSessionBase()
        {
            this.Protocol = SerialPort;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SerialProperty?.ToString();
        }
        #region 变量

        private DelaySender m_delaySender;
        private bool m_online => MainSerialPort?.IsOpen == true;
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
        private readonly InternalSerialCore m_serialCore = new InternalSerialCore();
        #endregion 变量

        #region 事件

        /// <inheritdoc/>
        public ConnectedEventHandler<ISerialSession> Connected { get; set; }

        /// <inheritdoc/>
        public SerialConnectingEventHandler<ISerialSession> Connecting { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ISerialSessionBase> Disconnected { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ISerialSessionBase> Disconnecting { get; set; }

        private async Task PrivateOnConnected(ConnectedEventArgs o)
        {
            await this.OnConnected(o);
        }

        /// <summary>
        /// 已经建立连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnected(ConnectedEventArgs e)
        {
            try
            {
                if (this.Connected != null)
                {
                    await this.Connected.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }
                await this.PluginsManager.RaiseAsync(nameof(ITcpConnectedPlugin.OnTcpConnected), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        private async Task PrivateOnConnecting(SerialConnectingEventArgs e)
        {
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty).Invoke());
            }

            await this.OnConnecting(e);
        }

        /// <summary>
        /// 准备连接的时候，此时并未建立连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnecting(SerialConnectingEventArgs e)
        {
            try
            {
                if (this.Connecting != null)
                {
                    await this.Connecting.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }
                await this.PluginsManager.RaiseAsync(nameof(ITcpConnectingPlugin.OnTcpConnecting), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
            }
        }

        private async Task PrivateOnDisconnected(object obj)
        {
            this.m_receiver?.TryInputReceive(default, default);
            await this.OnDisconnected((DisconnectEventArgs)obj);
        }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnected(DisconnectEventArgs e)
        {
            try
            {
                if (this.Disconnected != null)
                {
                    await this.Disconnected.Invoke(this, e).ConfigureAwait(false);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginsManager.RaiseAsync(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this, e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
            }
        }

        private async Task PrivateOnDisconnecting(object obj)
        {
            await this.OnDisconnecting((DisconnectEventArgs)obj);
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
                    await this.Disconnecting.Invoke(this, e).ConfigureAwait(false);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginsManager.RaiseAsync(nameof(ITcpDisconnectingPlugin.OnTcpDisconnecting), this, e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.GetSerialCore().ReceiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.GetSerialCore().SendCounter.LastIncrement;

        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <inheritdoc/>
        public SerialProperty SerialProperty { get; private set; }
        /// <inheritdoc/>
        public SerialPort MainSerialPort { get; private set; }

        /// <inheritdoc/>
        public bool Online { get => this.m_online; }

        /// <inheritdoc/>
        public bool CanSend => this.m_online;

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }



        #endregion 属性

        #region 断开操作

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {
            lock (this.GetSerialCore())
            {
                if (this.m_online)
                {
                    Task.Factory.StartNew(this.PrivateOnDisconnecting, new DisconnectEventArgs(true, msg));
                    this.MainSerialPort.TryClose();
                    this.BreakOut(true, msg);
                }
            }
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            lock (this.GetSerialCore())
            {
                if (this.m_online)
                {
                    Task.Factory.StartNew(this.PrivateOnDisconnecting, new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));
                    this.BreakOut(true, $"{nameof(Dispose)}主动断开");
                }
            }
            base.Dispose(disposing);
        }

        #endregion 断开操作

        #region Connect

        /// <summary>
        /// 打开串口
        /// </summary>
        protected void Open()
        {
            try
            {
                ThrowIfDisposed();
                this.m_semaphore.Wait();
                if (this.m_online)
                {
                    return;
                }
                if (this.DisposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.Config == null)
                {
                    throw new ArgumentNullException(nameof(this.Config), "配置文件不能为空。");
                }
                var serialProperty = this.Config.GetValue(SerialConfigExtension.SerialProperty) ?? throw new ArgumentNullException("串口配置不能为空。");
                this.MainSerialPort.SafeDispose();
                var serialPort = CreateSerial(serialProperty);
                this.PrivateOnConnecting(new SerialConnectingEventArgs(serialPort)).ConfigureAwait(false).GetAwaiter().GetResult();

                serialPort.Open();

                this.SetSerialPort(serialPort);
                this.BeginReceive();

                this.PrivateOnConnected(new ConnectedEventArgs()).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            finally
            {
                this.m_semaphore.Release();
            }
        }


        private void BeginReceive()
        {
            this.GetSerialCore().BeginIocpReceive();
        }


        /// <inheritdoc/>
        public virtual ISerialSession Connect()
        {
            this.Open();
            return this;
        }

        /// <inheritdoc/>
        public async Task<ISerialSession> ConnectAsync()
        {
            return await Task.Run(() =>
            {
                return this.Connect();
            });
        }

        #endregion Connect

        #region Receiver

        private Receiver m_receiver;

        /// <inheritdoc/>
        public IReceiver CreateReceiver()
        {
            return this.m_receiver ??= new Receiver(this);
        }

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.m_receiver = null;
        }

        #endregion

        private void SerialCoreBreakOut(SerialCore core, bool manual, string msg)
        {
            this.BreakOut(manual, msg);
        }
        /// <summary>
        /// BreakOut。
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void BreakOut(bool manual, string msg)
        {
            lock (this.GetSerialCore())
            {
                if (this.m_online)
                {
                    this.MainSerialPort.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    Task.Factory.StartNew(this.PrivateOnDisconnected, new DisconnectEventArgs(manual, msg));
                }
            }
        }

        private SerialCore GetSerialCore()
        {
            this.ThrowIfDisposed();
            return this.m_serialCore ?? throw new ObjectDisposedException(this.GetType().Name);
        }


        /// <inheritdoc/>
        public virtual void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
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

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <param name="e"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task ReceivedData(ReceivedDataEventArgs e)
        {
            return this.PluginsManager.RaiseAsync(nameof(ITcpReceivedPlugin.OnTcpReceived), this, e);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual async Task<bool> SendingData(byte[] buffer, int offset, int length)
        {
            if (this.PluginsManager.GetPluginCount(nameof(ITcpSendingPlugin.OnTcpSending)) > 0)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                await this.PluginsManager.RaiseAsync(nameof(ITcpSendingPlugin.OnTcpSending), this, args).ConfigureAwait(false);
                return args.IsPermitOperation;
            }
            return true;
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.SerialProperty = config.GetValue(SerialConfigExtension.SerialProperty);
            this.Logger ??= this.Container.Resolve<ILog>();
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
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

        private static SerialPort CreateSerial(SerialProperty serialProperty)
        {
            SerialPort serialPort = new SerialPort(serialProperty.PortName, serialProperty.BaudRate, serialProperty.Parity, serialProperty.DataBits, serialProperty.StopBits)
            {
                DtrEnable = true,
                RtsEnable = true
            };
            return serialPort;
        }

        #region 发送

        #region 同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(IRequestInfo requestInfo)
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            this.DataHandlingAdapter.SendInput(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            return this.DataHandlingAdapter.SendInputAsync(buffer, offset, length);
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
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (!this.DataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            return this.DataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (this.DataHandlingAdapter.CanSplicingSend)
            {
                return this.DataHandlingAdapter.SendInputAsync(transferBytes);
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
                    return this.DataHandlingAdapter.SendInputAsync(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
        }

        #endregion 异步发送

        /// <inheritdoc/>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            if (this.SendingData(buffer, offset, length).GetFalseAwaitResult())
            {
                if (this.m_delaySender != null)
                {
                    this.m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                    return;
                }
                this.GetSerialCore().Send(buffer, offset, length);
            }
        }

        /// <inheritdoc/>
        public async Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            if (await this.SendingData(buffer, offset, length))
            {
                await this.GetSerialCore().SendAsync(buffer, offset, length);
            }
        }

        #endregion 发送


        #region 自定义


        private void SetSerialPort(SerialPort serialPort)
        {
            if (serialPort == null)
            {
                return;
            }

            this.MainSerialPort = serialPort;
            this.SerialProperty ??= new SerialProperty();
            this.SerialProperty.Parity = serialPort.Parity;
            this.SerialProperty.PortName = serialPort.PortName;
            this.SerialProperty.StopBits = serialPort.StopBits;
            this.SerialProperty.DataBits = serialPort.DataBits;
            this.SerialProperty.BaudRate = serialPort.BaudRate;

            var delaySenderOption = this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty);
            if (delaySenderOption != null)
            {
                this.m_delaySender = new DelaySender(delaySenderOption, this.MainSerialPort.AbsoluteSend);
            }
            this.m_serialCore.Reset(serialPort);
            this.m_serialCore.OnReceived = this.HandleReceived;
            this.m_serialCore.OnBreakOut = this.SerialCoreBreakOut;
            if (this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) is int minValue)
            {
                this.m_serialCore.MinBufferSize = minValue;
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) is int maxValue)
            {
                this.m_serialCore.MaxBufferSize = maxValue;
            }
        }

        private void HandleReceived(SerialCore core, ByteBlock byteBlock)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }
                if (this.ReceivingData(byteBlock).GetFalseAwaitResult())
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
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task<bool> ReceivingData(ByteBlock byteBlock)
        {
            if (this.PluginsManager.GetPluginCount(nameof(ITcpReceivingPlugin.OnTcpReceiving)) > 0)
            {
                return this.PluginsManager.RaiseAsync(nameof(ITcpReceivingPlugin.OnTcpReceiving), this, new ByteBlockEventArgs(byteBlock));
            }
            return Task.FromResult(false);
        }

        #endregion
    }
}