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
using RRQMCore.Log;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /*
    若汝棋茗
    */

    /// <summary>
    /// 若汝棋茗客户端
    /// </summary>
    public abstract class TcpClient : BaseSocket, IClient, IHandleBuffer
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpClient()
        {
            this.BytePool = new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20);
            this.DataHandlingAdapter = new NormalDataHandlingAdapter();
            this.AllowSend = true;
        }
        /// <summary>
        /// 判断是否已连接
        /// </summary>
        public bool Online { get { return MainSocket == null ? false : MainSocket.Connected; } }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get;  private set; }

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
        /// 允许发送自由数据
        /// </summary>
        public bool AllowSend { get; protected set; }

        private BufferQueueGroup queueGroup;

        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public event RRQMShowMesEventHandler ConnectedService;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMShowMesEventHandler DisConnectedService;

        private void ConnectedServiceMethod(object sender, MesEventArgs e)
        {
            ConnectedService?.Invoke(sender, e);
        }

        private void DisConnectedServiceMethod(object sender, MesEventArgs e)
        {
            DisConnectedService?.Invoke(sender, e);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="setting"></param>
        public virtual void Connect(ConnectSetting setting)
        {
            MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPAddress IP = IPAddress.Parse(setting.TargetIP);
                EndPoint endPoint = new IPEndPoint(IP, setting.TargetPort);
                MainSocket.Connect(endPoint);
                queueGroup = new BufferQueueGroup();
                queueGroup.Thread = new Thread(HandleBuffer);//处理用户的消息
                queueGroup.waitHandleBuffer = new AutoResetEvent(false);
                queueGroup.bufferAndClient = new BufferQueue();
                queueGroup.Thread.IsBackground = true;
                queueGroup.Thread.Name = "客户端处理线程";
                queueGroup.Thread.Start();
                this.IP = setting.TargetIP;
                this.Port = setting.TargetPort;

                BeginReceive();
                ConnectedServiceMethod(this, new MesEventArgs("SuccessConnection"));
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
        }

        private void HandleBuffer()
        {
            while (true)
            {
                if (disposable)
                {
                    break;
                }
                ClientBuffer clientBuffer;
                if (queueGroup.bufferAndClient.TryDequeue(out clientBuffer))
                {
                    try
                    {
                        clientBuffer.client.HandleBuffer(clientBuffer.byteBlock);
                    }
                    catch (Exception e)
                    {
                        Logger.Debug(LogType.Error, this, $"在处理数据时发生错误，信息：{e.Message}");
                    }
                    finally
                    {
                        clientBuffer.byteBlock.Dispose();
                    }
                }
                else
                {
                    queueGroup.waitHandleBuffer.WaitOne();
                }
            }
        }

        /// <summary>
        /// 启动消息接收
        /// </summary>
        private void BeginReceive()
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            try
            {
                MainSocket.BeginReceive(byteBlock.Buffer, 0, byteBlock.Buffer.Length, SocketFlags.None, new AsyncCallback(Received), byteBlock);
            }
            catch
            {
                DisConnectedServiceMethod(this, new MesEventArgs("BreakOut"));
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar"></param>
        private void Received(IAsyncResult ar)
        {
            try
            {
                int r = MainSocket.EndReceive(ar);
                if (r == 0)
                {
                    DisConnectedServiceMethod(this, new MesEventArgs("BreakOut"));
                    return;
                }

                ClientBuffer clientBuffer = new ClientBuffer();
                clientBuffer.client = this;
                clientBuffer.byteBlock = (ByteBlock)ar.AsyncState;
                clientBuffer.byteBlock.Position = r;

                this.queueGroup.bufferAndClient.Enqueue(clientBuffer);
                this.queueGroup.waitHandleBuffer.Set();

                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                if (!this.disposable)
                {
                    MainSocket.BeginReceive(byteBlock.Buffer, 0, byteBlock.Buffer.Length, SocketFlags.None, new AsyncCallback(Received), byteBlock);
                }
            }
            catch
            {
                DisConnectedServiceMethod(this, new MesEventArgs("BreakOut"));
            }
        }

        void IHandleBuffer.HandleBuffer(ByteBlock byteBlock)
        {
            if (this.dataHandlingAdapter == null)
            {
                throw new RRQMException("数据处理适配器为空");
            }
            this.dataHandlingAdapter.Received(byteBlock);
        }

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
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, (int)byteBlock.Position);
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

        private void Sent(byte[] buffer, int offset, int length)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("该实例已断开");
            }
            if (!this.AllowSend)
            {
                throw new RRQMException("不允许发送自由数据");
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
    }
}