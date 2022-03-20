//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Dependency;
using RRQMCore.Log;
using System;
using System.Net;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// 若汝棋茗内置UDP会话
    /// </summary>
    public class UdpSession : UdpSessionBase
    {
        /// <summary>
        /// 当收到数据时
        /// </summary>
        public event RRQMUDPByteBlockEventHandler Received;

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            Received?.Invoke(remoteEndPoint, byteBlock);
        }
    }

    /// <summary>
    /// TCP服务器
    /// </summary>
    public abstract class UdpSessionBase : BaseSocket, IService, IClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpSessionBase()
        {
            this.Protocol = Protocol.UDP;
            this.Container = new Container();
            this.Container.RegisterTransient<ILog, ConsoleLogger>();
            this.monitor = new NetworkMonitor(null, new Socket(SocketType.Dgram, ProtocolType.Udp));
        }

        private IPHost remoteIPHost;
        private NetworkMonitor monitor;
        private long recivedCount;
        private ServerState serverState;
        private RRQMConfig serviceConfig;
        private bool study;
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual Protocol Protocol { get; set; }

        /// <summary>
        /// 默认远程节点
        /// </summary>
        public IPHost RemoteIPHost => this.remoteIPHost;

        /// <summary>
        /// 监听器
        /// </summary>
        public NetworkMonitor Monitor => this.monitor;

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName => this.Config == null ? null : this.Config.GetValue<string>(RRQMConfigExtensions.ServerNameProperty);

        /// <summary>
        /// 获取服务器状态
        /// </summary>
        public ServerState ServerState => this.serverState;

        /// <summary>
        /// 获取配置
        /// </summary>
        public RRQMConfig Config => this.serviceConfig;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Stop();
            this.serverState = ServerState.Disposed;
            base.Dispose(disposing);
        }

        #region 向默认远程同步发送

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(byte[] buffer, int offset, int length)
        {
            if (this.remoteIPHost == null)
            {
                throw new RRQMException("默认终结点为空");
            }
            this.Send(this.remoteIPHost.EndPoint, buffer, offset, length);
        }

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 向默认终结点发送
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 向默认远程同步发送

        #region 向默认远程异步发送

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
            if (this.remoteIPHost == null)
            {
                throw new RRQMException("默认终结点为空");
            }
            this.SendAsync(this.remoteIPHost.EndPoint, buffer, offset, length);
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

        #endregion 向默认远程异步发送

        #region 向设置的远程同步发送

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void Send(EndPoint remoteEP, byte[] buffer, int offset, int length)
        {
            this.monitor.Socket.SendTo(buffer, offset, length, SocketFlags.None, remoteEP);
        }

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        public virtual void Send(EndPoint remoteEP, byte[] buffer)
        {
            this.monitor.Socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP);
        }

        /// <summary>
        /// 向设置的远程同步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="byteBlock"></param>
        public virtual void Send(EndPoint remoteEP, ByteBlock byteBlock)
        {
            this.monitor.Socket.SendTo(byteBlock.Buffer, 0, byteBlock.Len, SocketFlags.None, remoteEP);
        }

        #endregion 向设置的远程同步发送

        #region 向设置的远程异步发送

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public virtual void SendAsync(EndPoint remoteEP, byte[] buffer, int offset, int length)
        {
            this.monitor.Socket.BeginSendTo(buffer, offset, length, SocketFlags.None, remoteEP, null, null);
        }

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="buffer"></param>
        public virtual void SendAsync(EndPoint remoteEP, byte[] buffer)
        {
            this.monitor.Socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP, null, null);
        }

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="byteBlock"></param>
        public virtual void SendAsync(EndPoint remoteEP, ByteBlock byteBlock)
        {
            this.monitor.Socket.BeginSendTo(byteBlock.Buffer, 0, byteBlock.Len, SocketFlags.None, remoteEP, null, null);
        }

        #endregion 向设置的远程异步发送

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="serverConfig"></param>
        public IService Setup(RRQMConfig serverConfig)
        {
            this.serviceConfig = serverConfig;
            this.LoadConfig(this.serviceConfig);
            return this;
        }

        /// <summary>
        /// 通过端口配置
        /// </summary>
        /// <param name="port"></param>
        public IService Setup(int port)
        {
            RRQMConfig serverConfig = new RRQMConfig();
            serverConfig.SetBindIPHost(new IPHost(port));
            return this.Setup(serverConfig);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public IService Start()
        {
            if (this.serverState == ServerState.Disposed)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }

            switch (this.serverState)
            {
                case ServerState.None:
                    {
                        if (this.serviceConfig.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty) is IPHost iPHost)
                        {
                            this.BeginReceive(iPHost);
                        }

                        break;
                    }
                case ServerState.Running:
                    break;

                case ServerState.Stopped:
                    {
                        if (this.serviceConfig.GetValue<IPHost>(RRQMConfigExtensions.BindIPHostProperty) is IPHost iPHost)
                        {
                            this.BeginReceive(iPHost);
                        }
                        break;
                    }
                case ServerState.Disposed:
                    {
                        throw new RRQMException("无法再次利用已释放对象");
                    }
            }

            this.serverState = ServerState.Running;
            return this;
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public IService Stop()
        {
            if (this.monitor != null)
            {
                this.monitor.Socket.Dispose();
            }
            this.monitor = null;
            this.serverState = ServerState.Stopped;
            return this;
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock);

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected virtual void LoadConfig(RRQMConfig serverConfig)
        {
            if (serverConfig == null)
            {
                throw new RRQMException("配置文件为空");
            }
            this.logger = this.Container.Resolve<ILog>();
            this.remoteIPHost = serverConfig.GetValue<IPHost>(RRQMConfigExtensions.RemoteIPHostProperty);
            this.BufferLength = serverConfig.BufferLength;
        }

        /// <summary>
        /// 在Socket初始化对象后，Bind之前调用。
        /// 可用于设置Socket参数。
        /// 父类方法可覆盖。
        /// </summary>
        /// <param name="socket"></param>
        protected virtual void PreviewBind(Socket socket)
        {
        }

        private void BeginReceive(IPHost iPHost)
        {
            int threadCount = this.Config.GetValue<int>(RRQMConfigExtensions.ThreadCountProperty);
            Socket socket = new Socket(iPHost.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.PreviewBind(socket);
            socket.Bind(iPHost.EndPoint);

            this.monitor = new NetworkMonitor(iPHost, socket);

            switch (this.serviceConfig.ReceiveType)
            {
                case ReceiveType.Auto:
                    {
                        SocketAsyncEventArgs eventArg = new SocketAsyncEventArgs();
                        eventArg.Completed += this.IO_Completed;
                        ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                        eventArg.UserToken = byteBlock;
                        eventArg.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                        eventArg.RemoteEndPoint = iPHost.EndPoint;
                        if (!socket.ReceiveFromAsync(eventArg))
                        {
                            this.ProcessReceive(socket, eventArg);
                        }
                        break;
                    }
                default:
                    throw new RRQMException("UDP中只支持Auto模式");
            }
        }

        private void HandleBuffer(EndPoint endPoint, ByteBlock byteBlock)
        {
            try
            {
                this.HandleReceivedData(endPoint, byteBlock);
            }
            catch (Exception e)
            {
                this.Logger.Debug(LogType.Error, this, $"在处理数据时发生错误，信息：{e.Message}");
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessReceive((Socket)sender, e);
        }

        private void ProcessReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (this.serverState == ServerState.Running && e.SocketError == SocketError.Success)
            {
                ByteBlock byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);

                this.HandleBuffer(e.RemoteEndPoint, byteBlock);

                ByteBlock newByteBlock = BytePool.GetByteBlock(this.BufferLength);
                e.UserToken = newByteBlock;
                e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                try
                {
                    if (!socket.ReceiveFromAsync(e))
                    {
                        this.ProcessReceive(socket, e);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Debug(LogType.Error, this, ex.Message, ex);
                }
            }
        }
    }
}