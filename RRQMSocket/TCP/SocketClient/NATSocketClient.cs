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
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 端口转发辅助
    /// </summary>
    public class NATSocketClient : SocketClient
    {
        SocketAsyncEventArgs eventArgs;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSetDataHandlingAdapter => false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public NATSocketClient()
        {
            this.SetAdapter(new NormalDataHandlingAdapter());
        }

        private Socket targetSocket;

        internal void BeginRunTargetSocket(Socket socket)
        {
            this.targetSocket = socket;
            switch (this.receiveType)
            {
                case ReceiveType.IOCP:
                    {
                        eventArgs = new SocketAsyncEventArgs();
                        eventArgs.Completed += this.EventArgs_Completed;
                        ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                        eventArgs.UserToken = byteBlock;
                        eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                        if (!this.targetSocket.ReceiveAsync(eventArgs))
                        {
                            ProcessReceived(eventArgs);
                        }
                        break;
                    }
                case ReceiveType.BIO:
                    {
                        Thread thread = new Thread(this.BIOReceive);
                        thread.IsBackground = true;
                        thread.Name = $"{this.id}客户端转发线程";
                        thread.Start();
                        break;
                    }
                default:
                    break;
            }
        }

        private void BIOReceive()
        {
            while (true)
            {
                if (this.disposable)
                {
                    break;
                }
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);

                try
                {
                    int r = this.targetSocket.Receive(byteBlock.Buffer);
                    if (r == 0)
                    {
                        byteBlock.Dispose();
                        break;
                    }
                    byteBlock.SetLength(r);
                    this.HandleReceivedDataFromTarget(byteBlock);
                }
                catch
                {
                    break;
                }
            }
            this.breakOut = true;
        }
        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    ProcessReceived(e);
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
                    ByteBlock byteBlock = (ByteBlock)e.UserToken;
                    byteBlock.SetLength(e.BytesTransferred);
                    this.HandleReceivedDataFromTarget(byteBlock);

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
                        ByteBlock newByteBlock = BytePool.GetByteBlock(this.BufferLength);
                        e.UserToken = newByteBlock;
                        e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                        if (!targetSocket.ReceiveAsync(e))
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
        /// 处理从目标服务器接收的数据。
        /// </summary>
        /// <param name="byteBlock"></param>
        protected virtual void HandleReceivedDataFromTarget(ByteBlock byteBlock)
        {
            try
            {
                if (this.disposable)
                {
                    return;
                }
                this.Send(byteBlock);
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            this.targetSocket.Send(byteBlock.Buffer, 0, byteBlock.Len, SocketFlags.None);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Dispose()
        {
            this.targetSocket.Dispose();
            base.Dispose();
        }
    }
}
