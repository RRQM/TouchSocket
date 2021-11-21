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
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP客户端
    /// </summary>
    public abstract class TokenClient : TcpClient, ITokenClient
    {
        private string verifyToken = "rrqm";

        /// <summary>
        /// 验证令箭
        /// </summary>
        public string VerifyToken
        {
            get { return verifyToken; }
        }

        private string id;

        /// <summary>
        /// 获取服务器分配的ID
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        private int verifyTimeout;

        /// <summary>
        /// 验证超时时间,默认为3000ms；
        /// </summary>
        public int VerifyTimeout
        {
            get { return verifyTimeout; }
        }

        /// <summary>
        /// 重新设置ID,但是不会同步到服务器
        /// </summary>
        /// <param name="id"></param>
        public virtual void ResetID(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public override ITcpClient Connect()
        {
            if (this.ClientConfig == null)
            {
                throw new ArgumentNullException("配置文件不能为空。");
            }
            IPHost iPHost = this.ClientConfig.RemoteIPHost;
            if (iPHost == null)
            {
                throw new ArgumentNullException("iPHost不能为空。");
            }

            if (this.disposable)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }

            if (this.Online)
            {
                return this;
            }

            try
            {
                Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                PreviewConnect(socket);
                socket.Connect(iPHost.EndPoint);
                this.MainSocket = socket;
                this.MainSocket.Send(Encoding.UTF8.GetBytes(this.verifyToken == null ? string.Empty : this.verifyToken));
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }

            int waitCount = 0;
            while (waitCount < this.verifyTimeout / 10)
            {
                if (this.MainSocket.Available > 0)
                {
                    ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                    try
                    {
                        int r = this.MainSocket.Receive(byteBlock.Buffer);
                        if (r > 0)
                        {
                            if (byteBlock.Buffer[0] == 1)
                            {
                                this.id = Encoding.UTF8.GetString(byteBlock.Buffer, 1, r - 1);
                                InitConnect();
                                return this;
                            }
                            else if (byteBlock.Buffer[0] == 2)
                            {
                                this.MainSocket.Dispose();
                                throw new RRQMTokenVerifyException(Encoding.UTF8.GetString(byteBlock.Buffer, 1, r - 1));
                            }
                            else if (byteBlock.Buffer[0] == 3)
                            {
                                this.MainSocket.Dispose();
                                throw new RRQMException("连接数量已达到服务器设定最大值");
                            }
                        }
                    }
                    finally
                    {
                        byteBlock.Dispose();
                    }
                }
                waitCount++;
                Thread.Sleep(10);
            }

            this.MainSocket.Dispose();
            throw new RRQMTimeoutException("验证Token超时");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            this.verifyToken = (string)clientConfig.GetValue(TokenClientConfig.VerifyTokenProperty);
            this.verifyTimeout = (int)clientConfig.GetValue(TokenClientConfig.VerifyTimeoutProperty);
            if (string.IsNullOrEmpty(this.verifyToken))
            {
                this.verifyToken = "rrqm";
            }
        }
    }
}