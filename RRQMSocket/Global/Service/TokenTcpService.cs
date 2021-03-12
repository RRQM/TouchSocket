using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Pool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 需要验证的TCP服务器
    /// </summary>
    public abstract class TokenTcpService<T> : TcpService<T> where T : TcpSocketClient, new()
    {
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

        internal override void PreviewCreatSocketCliect(Socket socket, BufferQueueGroup queueGroup)
        {
            Task.Factory.StartNew(async () =>
            {
                ByteBlock byteBlock = this.BytePool.GetByteBlock(socket.Available);
                int waitCount = 0;
                while (waitCount < 50)
                {
                    if (socket.Available > 0)
                    {
                        try
                        {
                            int r = socket.Receive(byteBlock.Buffer);
                            string receivedToken = Encoding.UTF8.GetString(byteBlock.Buffer, 0, r);
                            if (receivedToken == this.ConnectionToken)
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
                                    T client = this.SocketClientPool.GetObject();
                                    if (client.NewCreat)
                                    {
                                        client.queueGroup = queueGroup;
                                        client.Service = this;
                                        client.BytePool = this.BytePool;
                                    }
                                    client.MainSocket = socket;
                                    client.BufferLength = this.BufferLength;
                                    this.OnCreatSocketCliect(client);

                                    client.BeginInitialize();
                                    client.BeginReceive();
                                    this.SocketClients.Add(client, client.NewCreat);
                                    ClientConnectedMethod(client, null);

                                    byteBlock.Write(1);
                                    byteBlock.Write(Encoding.UTF8.GetBytes(client.IDToken));
                                    socket.Send(byteBlock.Buffer, 0, (int)byteBlock.Position, SocketFlags.None);
                                    return;
                                }

                            }
                            else
                            {
                                byteBlock.Write(2);
                                socket.Send(byteBlock.Buffer, 0, 1, SocketFlags.None);
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
            });

        }
    }
}
