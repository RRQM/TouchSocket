//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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
using RRQMCore.Log;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP服务器
    /// </summary>
    public class TokenTcpService<TClient> : TcpService<TClient> where TClient : TcpSocketClient, new()
    {
       
        private string verifyToken = "rrqm";

        /// <summary>
        /// 连接令箭,当为null或空时，重置为默认值“rrqm”
        /// </summary>
        public string VerifyToken
        {
            get { return verifyToken; }
            set
            {
                if (value == null || value == string.Empty)
                {
                    value = "rrqm";
                }
                verifyToken = value;
            }
        }

        /// <summary>
        /// 获取或设置验证超时时间,默认为3秒；
        /// </summary>
        public int VerifyTimeout { get; set; } = 3;

        internal override void PreviewCreatSocketCliect(Socket socket, BufferQueueGroup queueGroup)
        {
            Task.Factory.StartNew((Func<Task>)(async () =>
            {
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                int waitCount = 0;
                while (waitCount < this.VerifyTimeout * 1000 / 20)
                {
                    if (socket.Available > 0)
                    {
                        try
                        {
                            int r = socket.Receive(byteBlock.Buffer);

                            VerifyOption verifyOption = new VerifyOption();
                            verifyOption.Token = Encoding.UTF8.GetString(byteBlock.Buffer, 0, r);
                            this.OnVerifyToken(verifyOption);

                            if (verifyOption.Accept)
                            {
                                if (this.SocketClients.Count > this.MaxCount)
                                {
                                    byteBlock.Write(3);
                                    this.Logger.Debug(LogType.Error, this, "连接客户端数量已达到设定最大值");
                                    socket.Send(byteBlock.Buffer, 0, 1, SocketFlags.None);
                                    socket.Shutdown(SocketShutdown.Both);
                                    socket.Dispose();
                                    return;
                                }
                                else
                                {
                                    TClient client = this.socketClientPool.GetObject();
                                    client.Flag = verifyOption.Flag;
                                    if (client.NewCreate)
                                    {
                                        client.queueGroup = queueGroup;
                                        client.Service = this;
                                        client.Logger = this.Logger;
                                    }
                                    client.MainSocket = socket;
                                    client.BufferLength = this.BufferLength;

                                    CreateOption creatOption = new CreateOption();
                                    creatOption.NewCreate = client.NewCreate;

                                    lock (this)
                                    {
                                        if (client.NewCreate)
                                        {
                                            creatOption.ID = this.SocketClients.GetDefaultID();
                                        }
                                        else
                                        {
                                            creatOption.ID = client.ID;
                                        }
                                        base.OnCreatSocketCliect(client, creatOption);
                                        client.ID = creatOption.ID;

                                        this.SocketClients.Add(client);
                                    }

                                    byteBlock.Write(1);
                                    byteBlock.Write(Encoding.UTF8.GetBytes(client.ID));
                                    socket.Send(byteBlock.Buffer, 0, (int)byteBlock.Length, SocketFlags.None);

                                    client.BeginReceive();
                                    ClientConnectedMethod(client, null);

                                    return;
                                }
                            }
                            else
                            {
                                byteBlock.Write(2);
                                if (verifyOption.ErrorMessage != null)
                                {
                                    byteBlock.Write(Encoding.UTF8.GetBytes(verifyOption.ErrorMessage));
                                }
                                socket.Send(byteBlock.Buffer, 0, (int)byteBlock.Length, SocketFlags.None);
                                socket.Shutdown(SocketShutdown.Both);
                                socket.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, $"在验证客户端连接时发生错误，信息：{ex.Message}");
                        }
                        finally
                        {
                            byteBlock.Dispose();
                        }
                    }
                    waitCount++;
                    await Task.Delay(20);
                }

                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
            }));
        }

        /// <summary>
        /// 当验证Token时
        /// </summary>
        /// <param name="verifyOption"></param>
        protected virtual void OnVerifyToken(VerifyOption verifyOption)
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