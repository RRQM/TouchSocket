//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Pool;
using System;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// 服务器辅助类
    /// </summary>
    public abstract class TcpSocketClient : BaseSocket, ISocketClient, IHandleBuffer, IPoolObject
    {
        internal bool breakOut;
        internal BufferQueueGroup queueGroup;
        private bool isBeginReceive;
        private SocketAsyncEventArgs eventArgs;

        /// <summary>
        /// 包含此辅助类的主服务器类
        /// </summary>
        public IService Service { get; internal set; }

        /// <summary>
        /// 用于索引的令箭
        /// </summary>
        public string ID { get; internal set; }

        /// <summary>
        /// 判断该实例是否还在线
        /// </summary>
        public bool Online { get { return !this.breakOut; } }

        private DataHandlingAdapter dataHandlingAdapter;

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter
        {
            get { return dataHandlingAdapter; }
            set
            {
                dataHandlingAdapter = value;
                if (dataHandlingAdapter != null)
                {
                    dataHandlingAdapter.BytePool = this.BytePool;
                    dataHandlingAdapter.Logger = this.Logger;
                    dataHandlingAdapter.ReceivedCallBack = this.HandleReceivedData;
                    dataHandlingAdapter.SendCallBack = this.Sent;
                }
            }
        }

        /// <summary>
        /// 是否为新建对象
        /// </summary>
        public bool NewCreat { get; set; }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected abstract void HandleReceivedData(ByteBlock byteBlock);

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
            this.dataHandlingAdapter.Send(buffer, offset, length);
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
            this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
        }

        private void Sent(byte[] buffer, int offset, int length)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("该实例已断开");
            }
            try
            {
                MainSocket.Send(buffer, offset, length, SocketFlags.None);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
        }

        /// <summary>
        /// 启动消息接收
        /// </summary>
        internal void BeginReceive()
        {
            if (!this.isBeginReceive)
            {
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    this.eventArgs.UserToken = byteBlock;
                    this.eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                    if (!this.MainSocket.ReceiveAsync(this.eventArgs))
                    {
                        ProcessReceive();
                    }
                }
                catch
                {
                    this.breakOut = true;
                }
                this.isBeginReceive = true;
            }
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Receive && e.BytesTransferred > 0)
                {
                    ByteBlock byteBlock = (ByteBlock)e.UserToken;
                    byteBlock.Position = e.BytesTransferred;
                    byteBlock.SetLength(this.eventArgs.BytesTransferred);

                    ClientBuffer clientBuffer = this.queueGroup.clientBufferPool.GetObject();
                    clientBuffer.client = this;
                    clientBuffer.byteBlock = byteBlock;
                    queueGroup.bufferAndClient.Enqueue(clientBuffer);
                    queueGroup.waitHandleBuffer.Set();
                    WaitReceive();
                    ByteBlock newByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                    this.eventArgs.UserToken = newByteBlock;
                    this.eventArgs.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);
                    ProcessReceive();
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

        private void ProcessReceive()
        {
            if (!this.disposable)
            {
                if (this.eventArgs.SocketError == SocketError.Success && this.eventArgs.BytesTransferred > 0)
                {
                    if (!this.MainSocket.ReceiveAsync(this.eventArgs))
                    {
                        ProcessReceive();
                    }
                }
                else
                {
                    this.breakOut = true;
                }
            }
        }

        /// <summary>
        /// 测试是否在线
        /// </summary>
        internal void SendOnline()
        {
            try
            {
                MainSocket.Send(heartPackage, SocketFlags.None);
            }
            catch
            {
                this.breakOut = true;
            }
        }

        internal virtual void WaitReceive()
        {
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.breakOut = true;
        }

        /// <summary>
        /// 重新获取
        /// </summary>
        public virtual void Recreate()
        {
            this.breakOut = false;
            this.disposable = false;
            this.isBeginReceive = false;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public virtual void Destroy()
        {
            this.MainSocket = null;
        }

        /// <summary>
        /// 初次创建
        /// </summary>
        public virtual void Create()
        {
            this.eventArgs = new SocketAsyncEventArgs();
            this.eventArgs.Completed += this.EventArgs_Completed;
        }

        void IHandleBuffer.HandleBuffer(ClientBuffer clientBuffer)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
            }
            this.dataHandlingAdapter.Received(clientBuffer.byteBlock);
        }
    }
}