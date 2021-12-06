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
using RRQMCore.Run;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP服务器
    /// </summary>
    public abstract class TokenService<TClient> : TcpService<TClient> where TClient : TokenSocketClient, new()
    {
        private string verifyToken;

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken
        {
            get { return verifyToken; }
        }

        private int verifyTimeout;

        /// <summary>
        /// 验证超时时间,默认为3000ms
        /// </summary>
        public int VerifyTimeout
        {
            get { return verifyTimeout; }
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="ServiceConfig"></param>
        protected override void LoadConfig(ServiceConfig ServiceConfig)
        {
            base.LoadConfig(ServiceConfig);
            this.verifyTimeout = (int)ServiceConfig.GetValue(TokenServiceConfig.VerifyTimeoutProperty);
            this.verifyToken = (string)ServiceConfig.GetValue(TokenServiceConfig.VerifyTokenProperty);
            if (string.IsNullOrEmpty(this.verifyToken))
            {
                this.verifyToken = "rrqm";
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        protected override void PreviewConnecting(TClient client)
        {
            Task.Run(()=> 
            {
                WaitData<WaitVerify> waitData = new WaitData<WaitVerify>();
                Task.Run(() =>
                {
                    byte[] buffer = new byte[1024];
                    int r = client.MainSocket.Receive(buffer);
                    if (r > 0)
                    {
                        byte[] data = new byte[r];

                        Array.Copy(buffer, data, r);
                        WaitVerify verify = WaitVerify.GetVerifyInfo(data);
                        waitData.Set(verify);
                    }
                });

                switch (waitData.Wait(this.verifyTimeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitVerify waitVerify = waitData.WaitResult;
                            VerifyOption verifyOption = new VerifyOption();
                            verifyOption.Token = waitVerify.Token;
                            this.OnVerifyToken(client, verifyOption);

                            if (verifyOption.Accept)
                            {
                                ClientOperationEventArgs clientArgs = new ClientOperationEventArgs();
                                clientArgs.ID = GetDefaultNewID();
                                this.OnConnecting(client, clientArgs);
                                if (clientArgs.IsPermitOperation)
                                {
                                    client.id = clientArgs.ID;

                                    waitVerify.ID = client.id;
                                    waitVerify.Status = 1;
                                    client.MainSocket.Send(waitVerify.GetData(), SocketFlags.None);
                                    if (!this.SocketClients.TryAdd(client))
                                    {
                                        throw new RRQMException("ID重复");
                                    }
                                    client.BeginReceive();
                                    this.OnConnected(client, new MesEventArgs("新客户端连接"));
                                }
                                else
                                {
                                    waitVerify.Status = 4;
                                    client.MainSocket.Send(waitVerify.GetData(), SocketFlags.None);
                                    client.MainSocket.Dispose();
                                }
                            }
                            else
                            {
                                waitVerify.Status = 2;
                                waitVerify.Message = verifyOption.ErrorMessage;
                                client.MainSocket.Send(waitVerify.GetData(), SocketFlags.None);
                                client.MainSocket.Dispose();
                            }
                        }
                        break;
                    case WaitDataStatus.Overtime:
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            client.MainSocket.Dispose();
                            break;
                        }

                }
            });
           
        }

        /// <summary>
        /// 当验证Token时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="verifyOption"></param>
        protected virtual void OnVerifyToken(TClient client,VerifyOption verifyOption)
        {
            if (verifyOption.Token == this.verifyToken)
            {
                verifyOption.Accept = true;
            }
            else
            {
                verifyOption.ErrorMessage = "Token不受理";
            }
        }
    }
}