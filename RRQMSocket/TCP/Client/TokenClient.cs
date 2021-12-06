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
using RRQMCore.Run;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP客户端
    /// </summary>
    public abstract class TokenClient : TcpClient, ITokenClient
    {
        private RRQMWaitHandlePool<IWaitResult> waitHandlePool;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenClient()
        {
            this.waitHandlePool = new RRQMWaitHandlePool<IWaitResult>();
        }


        /// <summary>
        /// 等待返回池
        /// </summary>
        public RRQMWaitHandlePool<IWaitResult> WaitHandlePool { get => this.waitHandlePool; }


        private string id;

        /// <summary>
        /// 获取服务器分配的ID
        /// </summary>
        public string ID
        {
            get { return id; }
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
            return this.Connect("rrqm");
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMTokenVerifyException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public virtual ITcpClient Connect(string verifyToken, CancellationToken token = default)
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

            WaitVerify waitVerify = new WaitVerify()
            {
                Token = verifyToken
            };
            WaitData<IWaitResult> waitData = this.waitHandlePool.GetWaitData(waitVerify);
            waitData.SetCancellationToken(token);

            try
            {
                Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                PreviewConnect(socket);
                socket.Connect(iPHost.EndPoint);
                this.MainSocket = socket;
                this.MainSocket.Send(waitVerify.GetData());
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
            

            Task.Run(() =>
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int r = this.MainSocket.Receive(buffer);
                    if (r > 0)
                    {
                        byte[] data = new byte[r];

                        Array.Copy(buffer, data, r);
                        WaitVerify verify = WaitVerify.GetVerifyInfo(data);
                        this.waitHandlePool.SetRun(verify);
                    }

                }
                catch
                {

                }
            });

            switch (waitData.Wait(1000 * 10))
            {
                case WaitDataStatus.SetRunning:
                    {
                        WaitVerify verifyResult = (WaitVerify)waitData.WaitResult;
                        if (verifyResult.Status == 1)
                        {
                            this.id = verifyResult.ID;
                            InitConnect();
                            return this;
                        }
                        else if (verifyResult.Status == 3)
                        {
                            this.MainSocket.Dispose();
                            throw new RRQMException("连接数量已达到服务器设定最大值");
                        }
                        else if (verifyResult.Status == 4)
                        {
                            this.MainSocket.Dispose();
                            throw new RRQMException("服务器拒绝连接");
                        }
                        else
                        {
                            this.MainSocket.Dispose();
                            throw new RRQMTokenVerifyException(verifyResult.Message);
                        }
                    }
                case WaitDataStatus.Overtime:
                    this.MainSocket.Dispose();
                    throw new RRQMTimeoutException("连接超时");
                case WaitDataStatus.Canceled:
                case WaitDataStatus.Disposed:
                default:
                    return this;
            }

            
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
        }
    }
}