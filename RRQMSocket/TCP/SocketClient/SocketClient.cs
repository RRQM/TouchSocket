//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器辅助类
    /// </summary>
    public abstract class SocketClient : BaseSocket, ISocketClient, IHandleBuffer
    {
        internal bool breakOut;
        internal BytePool bytePool;
        internal string id;
        internal long lastTick;
        internal ReceiveType receiveType;
        internal ITcpServiceBase service;
        internal ServiceConfig serviceConfig;
        private ClearType clearType;
        private DataHandlingAdapter dataHandlingAdapter;
        private SocketAsyncEventArgs eventArgs;
        private string ip;
        private Socket mainSocket;
        private NetworkStream networkStream;
        private int port;

        /// <summary>
        /// 连接
        /// </summary>
        public event RRQMMessageEventHandler<SocketClient> Connected;

        /// <summary>
        /// 正在连接
        /// </summary>
        public event RRQMMessageEventHandler<SocketClient> Connecting;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMMessageEventHandler<SocketClient> Disconnected;

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool
        { get { return this.bytePool; } }

        /// <summary>
        /// 选择清理类型
        /// </summary>
        public ClearType ClearType
        {
            get { return clearType; }
            set { clearType = value; }
        }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get
            {
                return dataHandlingAdapter;
            }
        }

        /// <summary>
        /// 标记
        /// </summary>
        public object Flag { get; set; }

        /// <summary>
        /// 用于索引的ID
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP
        {
            get { return ip; }
        }

        /// <summary>
        /// 主通信器
        /// </summary>
        public Socket MainSocket
        {
            get { return mainSocket; }
            internal set
            {
                mainSocket = value;
            }
        }

        /// <summary>
        /// IP及端口
        /// </summary>
        public string Name => $"{this.ip}:{this.port}";

        /// <summary>
        /// 判断该实例是否还在线
        /// </summary>
        public bool Online
        { get { return !this.breakOut; } }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Port
        {
            get { return port; }
        }

        /// <summary>
        /// 端口号
        /// </summary>
        /// <summary>
        /// 包含此辅助类的主服务器类
        /// </summary>
        public ITcpServiceBase Service
        {
            get { return service; }
        }

        /// <summary>
        /// 服务配置
        /// </summary>
        public ServiceConfig ServiceConfig
        {
            get { return serviceConfig; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Close()
        {
            this.breakOut = true;
            if (this.mainSocket != null)
            {
                this.mainSocket.Close();
            }
        }

        /// <summary>
        /// 完全释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this.disposable)
            {
                return;
            }
            base.Dispose();
            if (this.mainSocket != null)
            {
                this.mainSocket.Dispose();
            }
            if (this.eventArgs != null)
            {
                this.eventArgs.Dispose();
                this.eventArgs = null;
            }
            if (this.networkStream != null)
            {
                this.networkStream.Dispose();
                this.networkStream = null;
            }
            this.MainSocket = null;
            this.breakOut = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public NetworkStream GetNetworkStream()
        {
            if (this.networkStream == null)
            {
                throw new RRQMException($"请在连接成功，且在{ReceiveType.NetworkStream}模式下获取流");
            }
            return networkStream;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        void IHandleBuffer.HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                this.OnPreviewHandleReceivedData(byteBlock);

                if (this.disposable)
                {
                    return;
                }
                if (this.dataHandlingAdapter == null)
                {
                    logger.Debug(LogType.Error, this, "数据处理适配器为空");
                    return;
                }
                this.dataHandlingAdapter.Received(byteBlock);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 读取IP、Port
        /// </summary>
        public void ReadIpPort()
        {
            if (MainSocket == null)
            {
                this.ip = null;
                this.port = -1;
                return;
            }

            string ipport;
            if (MainSocket.Connected && MainSocket.RemoteEndPoint != null)
            {
                ipport = MainSocket.RemoteEndPoint.ToString();
            }
            else if (MainSocket.IsBound && MainSocket.LocalEndPoint != null)
            {
                ipport = MainSocket.LocalEndPoint.ToString();
            }
            else
            {
                return;
            }

            int r = ipport.LastIndexOf(":");
            this.ip = ipport.Substring(0, r);
            this.port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
        }

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
            }
            if (this.BytePool == null)
            {
                throw new RRQMException($"数据处理适配器应当在{nameof(Connecting)}执行后赋值。");
            }
            adapter.BytePool = this.BytePool;
            adapter.Logger = this.Logger;
            adapter.ReceivedCallBack = this.HandleReceivedData;
            adapter.SendCallBack = this.Sent;
            this.dataHandlingAdapter = adapter;
        }

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            this.breakOut = true;
            if (this.MainSocket != null)
            {
                MainSocket.Shutdown(how);
            }
        }

        /// <summary>
        /// 启动消息接收
        /// </summary>
        internal void BeginReceive()
        {
            try
            {
                switch (this.receiveType)
                {
                    case ReceiveType.IOCP:
                        {
                            eventArgs = new SocketAsyncEventArgs();
                            eventArgs.Completed += this.EventArgs_Completed;
                            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                            eventArgs.UserToken = byteBlock;
                            eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                            if (!MainSocket.ReceiveAsync(eventArgs))
                            {
                                ProcessReceived(eventArgs);
                            }
                            break;
                        }
                    case ReceiveType.BIO:
                        {
                            Thread thread = new Thread(this.BIOReceive);
                            thread.IsBackground = true;
                            thread.Name = $"{this.id}客户端接收线程";
                            thread.Start();
                            break;
                        }
                    case ReceiveType.NetworkStream:
                        {
                            this.networkStream = new NetworkStream(this.mainSocket, true);
                            break;
                        }
                    default:
                        break;
                }
            }
            catch
            {
                this.breakOut = true;
            }
        }

        internal void GetTimeout(int time, long nowTick)
        {
            try
            {
                this.OnPerSecond();
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在{nameof(OnPerSecond)}中发生异常", ex);
            }

            if (this.receiveType == ReceiveType.NetworkStream)
            {
                if (!this.mainSocket.Connected)
                {
                    this.breakOut = true;
                }
            }
            else
            {
                if (nowTick - this.lastTick / 10000000 > time)
                {
                    this.breakOut = true;
                }
            }
           
        }

        internal void OnEvent(int type, MesEventArgs e)
        {
            switch (type)
            {
                case 1:
                    {
                        this.OnConnected(e);
                        break;
                    }
                case 2:
                    {
                        this.OnDisconnected(e);
                        break;
                    }
                case 3:
                    {
                        this.OnConnecting((ClientOperationEventArgs)e);
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected abstract void HandleReceivedData(ByteBlock byteBlock, object obj);

        /// <summary>
        /// 当连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(MesEventArgs e)
        {
            this.Connected?.Invoke(this, e);
        }

        /// <summary>
        /// 正在连接
        /// </summary>
        protected virtual void OnConnecting(ClientOperationEventArgs e)
        {
            this.Connecting?.Invoke(this, e);

            if (this.dataHandlingAdapter == null)
            {
                if (e.DataHandlingAdapter == null)
                {
                    this.SetDataHandlingAdapter(new NormalDataHandlingAdapter());
                }
                else
                {
                    this.SetDataHandlingAdapter(e.DataHandlingAdapter);
                }
            }
        }

        /// <summary>
        /// 当断开连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(MesEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// 每一秒执行
        /// </summary>
        protected virtual void OnPerSecond()
        {
        }

        /// <summary>
        ///  预处理收到数据，
        /// 一般用于调试检验数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>返回值标识该数据是否继续向下执行</returns>
        protected virtual bool OnPreviewHandleReceivedData(ByteBlock byteBlock)
        {
            return true;
        }

        #region 同步发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }
            this.dataHandlingAdapter.Send(buffer, offset, length, false);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void Send(IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }
            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.Send(transferBytes, false);
            }
            else
            {
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.Send(byteBlock.Buffer, 0, byteBlock.Len, false);
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
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(byte[] buffer, int offset, int length)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }
            this.dataHandlingAdapter.Send(buffer, offset, length, true);
        }

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(byte[] buffer)
        {
            this.SendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// IOCP发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(ByteBlock byteBlock)
        {
            this.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void SendAsync(IList<TransferByte> transferBytes)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), "数据处理适配器为空");
            }
            if (this.dataHandlingAdapter.CanSplicingSend)
            {
                this.dataHandlingAdapter.Send(transferBytes, true);
            }
            else
            {
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Buffer, item.Offset, item.Length);
                    }
                    this.dataHandlingAdapter.Send(byteBlock.Buffer, 0, byteBlock.Len, true);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        #endregion 异步发送

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        protected virtual void ResetID(WaitSetID waitSetID)
        {
            this.Service.ResetID(waitSetID);
        }

        /// <summary>
        /// 等待接收
        /// </summary>
        protected virtual void WaitReceive()
        {
        }

        private void BIOReceive()
        {
            while (true)
            {
                if (this.disposable)
                {
                    break;
                }
                ByteBlock byteBlock = bytePool.GetByteBlock(this.BufferLength);

                try
                {
                    int r = this.mainSocket.Receive(byteBlock.Buffer);
                    if (r == 0)
                    {
                        byteBlock.Dispose();
                        break;
                    }
                    byteBlock.SetLength(r);
                    ((IHandleBuffer)this).HandleBuffer(byteBlock);
                }
                catch
                {
                    break;
                }
            }
            this.breakOut = true;
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    ProcessReceived(e);
                }
                else
                {
                    this.breakOut = true;
                }
            }
            catch
            {
                this.breakOut = true;
            }
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (!this.disposable)
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    if (this.clearType.HasFlag(ClearType.Receive))
                    {
                        this.lastTick = DateTime.Now.Ticks;
                    }

                    ByteBlock byteBlock = (ByteBlock)e.UserToken;
                    byteBlock.SetLength(e.BytesTransferred);
                    ((IHandleBuffer)this).HandleBuffer(byteBlock);

                    try
                    {
                        WaitReceive();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Debug(LogType.Error, this, ex.Message);
                    }

                    try
                    {
                        ByteBlock newByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        e.UserToken = newByteBlock;
                        e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                        if (!MainSocket.ReceiveAsync(e))
                        {
                            ProcessReceived(e);
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    this.breakOut = true;
                }
            }
            else
            {
                this.breakOut = true;
            }
        }

        private void Sent(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("该实例已断开");
            }
            if (isAsync)
            {
                this.mainSocket.BeginSend(buffer, offset, length, SocketFlags.None, null, null);
            }
            else
            {
                while (length > 0)
                {
                    int r = MainSocket.Send(buffer, offset, length, SocketFlags.None);
                    if (r == 0 && length > 0)
                    {
                        throw new RRQMException("发送数据不完全");
                    }
                    offset += r;
                    length -= r;
                }
            }

            if (this.clearType.HasFlag(ClearType.Send))
            {
                this.lastTick = DateTime.Now.Ticks;
            }
        }
    }
}