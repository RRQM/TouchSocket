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
using RRQMCore.Pool;
using System;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器辅助类
    /// </summary>
    public abstract class SocketClient : BaseSocket, ISocketClient, IHandleBuffer, IPoolObject
    {
        internal bool breakOut;
        internal ClearType clearType;
        internal string id;
        internal long lastTick;
        internal BufferQueueGroup queueGroup;
        internal bool separateThreadReceive;
        private DataHandlingAdapter dataHandlingAdapter;
        private SocketAsyncEventArgs eventArgs;
        private Socket mainSocket;

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get { return this.queueGroup == null ? null : this.queueGroup.bytePool; } }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get { return dataHandlingAdapter; }
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
        /// IPv4地址
        /// </summary>
        public string IP { get; protected set; }

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
        public string Name => $"{this.IP}:{this.Port}";

        /// <summary>
        /// 是否为新建对象
        /// </summary>
        public bool NewCreate { get; set; }

        /// <summary>
        /// 判断该实例是否还在线
        /// </summary>
        public bool Online { get { return !this.breakOut; } }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// 包含此辅助类的主服务器类
        /// </summary>
        public _ITcpService Service { get; internal set; }

        /// <summary>
        /// 初次创建对象，效应相当于构造函数，父类方法可覆盖
        /// </summary>
        public virtual void Create()
        {
           
        }

        /// <summary>
        /// 在断开连接时销毁对象，父类方法不可覆盖
        /// </summary>
        public virtual void Destroy()
        {
            this.MainSocket = null;
            this.dataHandlingAdapter = null;
        }

        /// <summary>
        /// 完全释放资源
        /// </summary>
        public override void Dispose()
        {
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
            this.breakOut = true;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        public void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                if (this.dataHandlingAdapter == null)
                {
                    throw new RRQMException("数据处理适配器为空");
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
                this.IP = null;
                this.Port = -1;
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
            this.IP = ipport.Substring(0, r);
            this.Port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
        }

        /// <summary>
        /// 重新获取,父类方法不可覆盖
        /// </summary>
        public virtual void Recreate()
        {
            this.breakOut = false;
            this.disposable = false;
        }

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
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
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
                this.OnBeforeReceive();
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += this.EventArgs_Completed;
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                eventArgs.UserToken = byteBlock;
                eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                if (!MainSocket.ReceiveAsync(eventArgs))
                {
                    ProcessReceived(eventArgs);
                }
                this.lastTick = DateTime.Now.Ticks;
            }
            catch
            {
                this.breakOut = true;
            }
        }

        /// <summary>
        /// 测试是否在线
        /// </summary>
        internal void GetTimeout(int time, long nowTick)
        {
            if (nowTick - this.lastTick / 10000000 > time)
            {
                this.breakOut = true;
            }

            try
            {
                this.OnPerSecond();
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在{nameof(OnPerSecond)}中发生异常", ex);
            }
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected abstract void HandleReceivedData(ByteBlock byteBlock, object obj);

        /// <summary>
        /// 在接收之前
        /// </summary>
        protected virtual void OnBeforeReceive()
        {
            if (this.dataHandlingAdapter == null)
            {
                this.SetDataHandlingAdapter(new NormalDataHandlingAdapter());
            }
        }

        /// <summary>
        /// 每一秒执行
        /// </summary>
        protected virtual void OnPerSecond()
        {
        }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="id"></param>
        protected virtual void ResetID(string id)
        {
            this.Service.ResetID(this.id, id);
        }

        /// <summary>
        /// 等待接收
        /// </summary>
        protected virtual void WaitReceive()
        {
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    ProcessReceived(e);
                }
                else if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    ProcessSend(e);
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

                    if (this.separateThreadReceive)
                    {
                        ClientBuffer clientBuffer = new ClientBuffer();
                        clientBuffer.client = this;
                        clientBuffer.byteBlock = (ByteBlock)e.UserToken;
                        clientBuffer.byteBlock.SetLength(e.BytesTransferred);
                        queueGroup.bufferAndClient.Enqueue(clientBuffer);
                        if (queueGroup.isWait)
                        {
                            queueGroup.isWait = false;
                            queueGroup.waitHandleBuffer.Set();
                        }
                    }
                    else
                    {
                        ByteBlock byteBlock = (ByteBlock)e.UserToken;
                        byteBlock.SetLength(e.BytesTransferred);
                        this.HandleBuffer(byteBlock);
                    }

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

        /// <summary>
        /// 发送完成时处理函数
        /// </summary>
        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                e.Dispose();
            }
            else
            {
                this.Logger.Debug(LogType.Error, this, "异步发送错误。");
            }
        }

        private void Sent(byte[] buffer, int offset, int length, bool isAsync)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("该实例已断开");
            }
            try
            {
                if (isAsync)
                {
                    SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                    sendEventArgs.Completed += EventArgs_Completed;
                    sendEventArgs.SetBuffer(buffer, offset, length);
                    sendEventArgs.RemoteEndPoint = this.MainSocket.RemoteEndPoint;
                    if (!this.MainSocket.SendAsync(sendEventArgs))
                    {
                        // 同步发送时处理发送完成事件
                        this.ProcessSend(sendEventArgs);
                    }
                }
                else
                {
                    int r = 0;
                    while (length > 0)
                    {
                        r = MainSocket.Send(buffer, offset, length, SocketFlags.None);
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
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
        }
    }
}