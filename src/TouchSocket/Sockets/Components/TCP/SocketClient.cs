//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Collections.Concurrent;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 服务器辅助类
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("ID={ID},IPAdress={IP}:{Port}")]
    public class SocketClient : BaseSocket, ISocketClient, IPluginObject
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketClient()
        {
            this.Protocol = Protocol.TCP;
        }

        #region 变量

        internal string m_id;
        internal long m_lastTick;
        internal ReceiveType m_receiveType;
        internal TcpServiceBase m_service;
        internal bool m_usePlugin;
        private DataHandlingAdapter m_adapter;
        private DelaySender m_delaySender;
        private Socket m_mainSocket;
        private int m_maxPackageSize;
        private bool m_online;
        private bool m_useDelaySender;
        private Stream m_workStream;

        #endregion 变量

        #region 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => this.m_online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        public ClearType ClearType { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TouchSocketConfig Config { get; internal set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => this.Config?.Container;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter => this.m_adapter;

        /// <summary>
        /// 用于索引的ID
        /// </summary>
        public string ID => this.m_id;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Socket MainSocket => this.m_mainSocket;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int MaxPackageSize => this.m_maxPackageSize;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Online => this.m_online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager => this.Config?.PluginsManager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Protocol Protocol { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType => this.m_receiveType;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TcpServiceBase Service => this.m_service;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UsePlugin => this.m_usePlugin;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl { get; private set; }

        #endregion 属性

        #region 事件&委托

        /// <summary>
        /// 断开连接
        /// </summary>
        public ClientDisconnectedEventHandler<ITcpClientBase> Disconnected { get; set; }

        #endregion 事件&委托

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastReceivedTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ServiceIP { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int ServicePort { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Close()
        {
            this.Close($"主动调用{nameof(Close)}");
        }

        /// <summary>
        /// 中断终端，传递中断消息
        /// </summary>
        /// <param name="msg"></param>
        public virtual void Close(string msg)
        {
            this.BreakOut(msg, true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (this.m_workStream == null)
            {
                this.m_workStream = new NetworkStream(this.m_mainSocket, true);
            }
            return this.m_workStream;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="newID"></param>
        public virtual void ResetID(string newID)
        {
            if (string.IsNullOrEmpty(newID))
            {
                throw new ArgumentException($"“{nameof(newID)}”不能为 null 或空。", nameof(newID));
            }

            if (this.m_id == newID)
            {
                return;
            }
            this.m_service.ResetID(this.m_id, newID);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            this.MainSocket.Shutdown(how);
        }

        internal void BeginReceive(ReceiveType receiveType)
        {
            if (receiveType == ReceiveType.Auto)
            {
                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += this.EventArgs_Completed;
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                eventArgs.UserToken = byteBlock;
                eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                if (!this.m_mainSocket.ReceiveAsync(eventArgs))
                {
                    this.ProcessReceived(eventArgs);
                }
            }
        }

        internal void BeginReceiveSsl(ReceiveType receiveType, ServiceSslOption sslOption)
        {
            SslStream sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_mainSocket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_mainSocket, false), false);
            sslStream.AuthenticateAsServer(sslOption.Certificate, sslOption.ClientCertificateRequired, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
            this.m_workStream = sslStream;
            this.UseSsl = true;
            if (receiveType == ReceiveType.Auto)
            {
                this.BeginSsl();
            }
        }

        internal void GetTimeout(int time, long nowTick)
        {
            if (nowTick - (this.m_lastTick / 10000000.0) > time)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    this.BreakOut($"超时无数据交互，被主动清理", false);
                }, null);
            }
        }

        internal void InternalConnected(TouchSocketEventArgs e)
        {
            this.m_online = true;

            if (this.Config.GetValue<DelaySenderOption>(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
            {
                this.m_useDelaySender = true;
                this.m_delaySender.SafeDispose();
                this.m_delaySender = new DelaySender(this.m_mainSocket, senderOption.QueueLength, this.OnDelaySenderError)
                {
                    DelayLength = senderOption.DelayLength
                };
            }

            if (this.m_usePlugin && this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnConnected), this, e))
            {
                return;
            }

            this.OnConnected(e);
        }

        internal void InternalConnecting(ClientOperationEventArgs e)
        {
            if (this.m_usePlugin && this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnConnecting), this, e))
            {
                return;
            }
            this.OnConnecting(e);
        }

        internal void InternalInitialized()
        {
            this.OnInitialized();
        }

        internal void SetSocket(Socket mainSocket)
        {
            this.m_mainSocket = mainSocket ?? throw new ArgumentNullException(nameof(mainSocket));
            this.IP = mainSocket.RemoteEndPoint.GetIP();
            this.Port = mainSocket.RemoteEndPoint.GetPort();
            this.ServiceIP = mainSocket.LocalEndPoint.GetIP();
            this.ServicePort = mainSocket.LocalEndPoint.GetPort();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            this.Close($"主动调用{nameof(Dispose)}");
            base.Dispose(disposing);
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        protected virtual void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual bool HandleSendingData(byte[] buffer, int offset, int length)
        {
            if (this.m_usePlugin)
            {
                SendingEventArgs args = new SendingEventArgs(buffer, offset, length);
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnSendingData), this, args);
                if (args.Operation.HasFlag(Operation.Permit))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 当客户端完整建立TCP连接，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(TouchSocketEventArgs e)
        {
            this.m_service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        protected virtual void OnConnecting(ClientOperationEventArgs e)
        {
            this.m_service.OnInternalConnecting(this, e);
        }

        /// <summary>
        /// 在延迟发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnDelaySenderError(Exception ex)
        {
            this.Logger.Log(LogType.Error, this, "发送错误", ex);
        }

        /// <summary>
        /// 客户端已断开连接，如果从Connecting中拒绝连接，则不会触发。如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// 当初始化完成时
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.SocketSend;
            if (this.Config != null)
            {
                this.m_maxPackageSize = Math.Max(adapter.MaxPackageSize, this.Config.GetValue<int>(TouchSocketConfigExtension.MaxPackageSizeProperty));
                adapter.MaxPackageSize = this.m_maxPackageSize;
            }

            this.m_adapter = adapter;
        }

        /// <summary>
        /// 绕过适配器直接发送。<see cref="ByteBlock.Buffer"/>作为数据时，仅可同步发送。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="isAsync">是否异步发送</param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        protected void SocketSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (!this.m_online)
            {
                throw new NotConnectedException(TouchSocketRes.NotConnected.GetDescription());
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                if (this.UseSsl)
                {
                    if (isAsync)
                    {
                        this.m_workStream.WriteAsync(buffer, offset, length);
                    }
                    else
                    {
                        this.m_workStream.Write(buffer, offset, length);
                    }
                }
                else
                {
                    if (this.m_useDelaySender && length < TouchSocketUtility.BigDataBoundary)
                    {
                        if (isAsync)
                        {
                            this.m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                        }
                        else
                        {
                            this.m_delaySender.Send(QueueDataBytes.CreateNew(buffer, offset, length));
                        }
                    }
                    else
                    {
                        if (isAsync)
                        {
                            this.m_mainSocket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
                        }
                        else
                        {
                            this.m_mainSocket.AbsoluteSend(buffer, offset, length);
                        }
                    }
                }
                if (this.ClearType.HasFlag(ClearType.Send))
                {
                    this.m_lastTick = DateTime.Now.Ticks;
                }

                this.LastSendTime=DateTime.Now;
            }
        }

        private void BeginSsl()
        {
            if (!this.m_disposedValue)
            {
                ByteBlock byteBlock = new ByteBlock(this.BufferLength);
                try
                {
                    this.m_workStream.BeginRead(byteBlock.Buffer, 0, byteBlock.Capacity, this.EndSsl, byteBlock);
                }
                catch (System.Exception ex)
                {
                    byteBlock.Dispose();
                    this.BreakOut(ex.Message, false);
                }
            }
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.SafeShutdown();
                    this.m_mainSocket.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.m_adapter.SafeDispose();
                    this.m_service?.SocketClients.TryRemove(this.m_id, out _);
                    this.PrivateOnDisconnected(new ClientDisconnectedEventArgs(manual, msg));
                    this.Disconnected = null;
                }
                base.Dispose(true);
            }
        }

        private void EndSsl(IAsyncResult result)
        {
            ByteBlock byteBlock = (ByteBlock)result.AsyncState;
            try
            {
                int r = this.m_workStream.EndRead(result);
                if (r == 0)
                {
                    this.BreakOut("远程终端主动关闭", false);
                }
                byteBlock.SetLength(r);

                this.HandleBuffer(byteBlock);
                this.BeginSsl();
            }
            catch (Exception ex)
            {
                byteBlock.Dispose();
                this.BreakOut(ex.Message, false);
            }
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                this.ProcessReceived(e);
            }
            catch (Exception ex)
            {
                e.SafeDispose();
                this.BreakOut(ex.Message, false);
            }
        }

        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                if (this.ClearType.HasFlag(ClearType.Receive))
                {
                    this.m_lastTick = DateTime.Now.Ticks;
                }
                this.LastReceivedTime = DateTime.Now;
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (this.UsePlugin && this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnReceivingData), this, new ByteBlockEventArgs(byteBlock)))
                {
                    return;
                }
                if (this.m_disposedValue)
                {
                    return;
                }
                if (this.m_adapter == null)
                {
                    this.Logger.Error(this, TouchSocketRes.NullDataAdapter.GetDescription());
                    return;
                }
                this.m_adapter.ReceivedInput(byteBlock);
            }
            catch (System.Exception ex)
            {
                this.Logger.Log(LogType.Error, this, "在处理数据时发生错误", ex);
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

            if (this.m_usePlugin)
            {
                ReceivedDataEventArgs args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnReceivedData), this, args);
                if (args.Handled)
                {
                    return;
                }
            }

            this.HandleReceivedData(byteBlock, requestInfo);

            this.m_service.OnInternalReceivedData(this, byteBlock, requestInfo);
        }

        private void PrivateOnDisconnected(ClientDisconnectedEventArgs e)
        {
            if (this.m_usePlugin && this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnDisconnected), this, e))
            {
                return;
            }
            this.OnDisconnected(e);
            if (!e.Handled)
            {
                this.m_service.OnInternalDisconnected(this, e);
            }
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (this.m_disposedValue)
            {
                e.SafeDispose();
            }
            else
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    ByteBlock byteBlock = (ByteBlock)e.UserToken;
                    byteBlock.SetLength(e.BytesTransferred);
                    this.HandleBuffer(byteBlock);
                    try
                    {
                        ByteBlock newByteBlock = BytePool.GetByteBlock(this.BufferLength);
                        e.UserToken = newByteBlock;
                        e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Capacity);

                        if (!this.m_mainSocket.ReceiveAsync(e))
                        {
                            this.ProcessReceived(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BreakOut(ex.Message, false);
                    }
                }
                else
                {
                    e.SafeDispose();
                    this.BreakOut("远程主机主动断开连接", false);
                }
            }
        }

        #region 默认发送

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
            this.SocketSend(buffer, offset, length, false);
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void DefaultSend(byte[] buffer)
        {
            this.DefaultSend(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void DefaultSend(ByteBlock byteBlock)
        {
            this.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 默认发送

        #region 异步默认发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public void DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            this.SocketSend(buffer, offset, length, true);
        }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public void DefaultSendAsync(byte[] buffer)
        {
            this.DefaultSendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public void DefaultSendAsync(ByteBlock byteBlock)
        {
            this.DefaultSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 异步默认发送

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
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            if (!this.m_adapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.m_adapter.SendInput(requestInfo, false);
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
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            this.m_adapter.SendInput(buffer, offset, length, false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(transferBytes, false);
            }
            else
            {
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    this.m_adapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len, false);
                }
                finally
                {
                    byteBlock.Dispose();
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
        public virtual void SendAsync(byte[] buffer, int offset, int length)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            this.m_adapter.SendInput(buffer, offset, length, true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void SendAsync(IRequestInfo requestInfo)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            if (!this.m_adapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.m_adapter.SendInput(requestInfo, true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            if (this.m_adapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketRes.NullDataAdapter.GetDescription());
            }
            if (this.m_adapter.CanSplicingSend)
            {
                this.m_adapter.SendInput(transferBytes, true);
            }
            else
            {
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    this.m_adapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len, true);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        #endregion 异步发送

        #region ID发送

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
            this.m_service.Send(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void Send(string id, IRequestInfo requestInfo)
        {
            this.m_service.Send(id, requestInfo);
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
        public void SendAsync(string id, byte[] buffer, int offset, int length)
        {
            this.m_service.SendAsync(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void SendAsync(string id, IRequestInfo requestInfo)
        {
            this.m_service.SendAsync(id, requestInfo);
        }

        #endregion ID发送
    }
}