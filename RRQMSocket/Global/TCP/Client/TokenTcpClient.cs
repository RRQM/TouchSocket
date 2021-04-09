//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP客户端
    /// </summary>
    public abstract class TokenTcpClient : TcpClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenTcpClient() : this(new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool">设置内存池实例</param>
        public TokenTcpClient(BytePool bytePool) : base(bytePool)
        {
        }

        private string connectionToken = "rrqm";

        /// <summary>
        /// 连接令箭,当为null或空时，重置为默认值“rrqm”
        /// </summary>
        public string ConnectionToken
        {
            get { return connectionToken; }
            set
            {
                if (value == null || value == string.Empty)
                {
                    value = "rrqm";
                }
                connectionToken = value;
            }
        }

        /// <summary>
        /// 获取服务器分配的令箭
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// 获取或设置验证超时时间,默认为3秒； 
        /// </summary>
        public int VerifyTimeout { get; set; } = 3;

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="setting"></param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public override void Connect(ConnectSetting setting)
        {
            IPAddress IP = IPAddress.Parse(setting.TargetIP);
            EndPoint endPoint = new IPEndPoint(IP, setting.TargetPort);
            this.Connect(endPoint);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="endPoint"></param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public override void Connect(EndPoint endPoint)
        {
            if (this.disposable)
            {
                throw new RRQMException("无法重新利用已释放对象");
            }
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(endPoint);
                this.MainSocket = socket;
                this.MainSocket.Send(Encoding.UTF8.GetBytes(this.ConnectionToken));
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }

            int waitCount = 0;
            while (waitCount < VerifyTimeout * 1000 / 20)
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
                                this.ID = Encoding.UTF8.GetString(byteBlock.Buffer, 1, r - 1);
                                Start();
                                return;
                            }
                            else if (byteBlock.Buffer[0] == 2)
                            {
                                throw new RRQMException("Token验证错误");
                            }
                            else if (byteBlock.Buffer[0] == 3)
                            {
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
                Thread.Sleep(20);
            }

            throw new RRQMTimeoutException("验证Token超时");
        }
    }
}