using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RRQMCore.ByteManager;
using RRQMCore.Log;

namespace RRQMSocket
{
    internal class AsyncSender : IDisposable
    {
        internal AsyncSender()
        {
            asyncBytes = new ConcurrentQueue<AsyncByte>();
            waitHandle = new AutoResetEvent(false);
            sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.Completed += this.SendEventArgs_Completed;
            this.sendThread = new Thread(this.BeginSend);
            this.sendThread.IsBackground = true;
            this.sendThread.Name = "AsyncSendThread";
            this.sendThread.Start();
        }

        private readonly SocketAsyncEventArgs sendEventArgs;
        private readonly Thread sendThread;
        private readonly EventWaitHandle waitHandle;
        private readonly ConcurrentQueue<AsyncByte> asyncBytes;
        private Socket socket;
        private bool sending;
        private ILog logger;
        private bool dispose;

        private void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                ProcessSend(e);
            }
            else
            {
                if (!dispose)
                {
                    this.waitHandle.Set();
                }
            }
        }
        /// <summary>
        /// 发送完成时处理函数
        /// </summary>
        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    this.logger.Debug(LogType.Error, this, "异步发送错误。");
                }
            }
            catch
            {

            }
            finally
            {
                if (!dispose)
                {
                    this.waitHandle.Set();
                }
            }
        }

        private void BeginSend()
        {
            while (!this.dispose)
            {
                try
                {
                    if (this.tryGet(out AsyncByte asyncByte))
                    {
                        this.sending = true;
                        this.sendEventArgs.SetBuffer(asyncByte.buffer, asyncByte.offset, asyncByte.length);
                        if (!this.socket.SendAsync(this.sendEventArgs))
                        {
                            // 同步发送时处理发送完成事件
                            this.ProcessSend(sendEventArgs);
                        }
                        else
                        {
                            this.waitHandle.WaitOne();
                        }

                    }
                    else
                    {
                        this.sending = false;
                        this.waitHandle.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Debug(LogType.Error, this, "异步发送错误。", ex);
                }

            }
        }

        byte[] buffer = new byte[1024 * 1024];
        internal void SetBufferLength(int len)
        {
            this.buffer = new byte[len];
        }
        private bool tryGet(out AsyncByte asyncByteDe)
        {
            int len = 0;
            int surLen = buffer.Length;
            while (true)
            {
                if (this.asyncBytes.TryPeek(out AsyncByte asyncB))
                {
                    if (surLen > asyncB.length)
                    {
                        if (this.asyncBytes.TryDequeue(out AsyncByte asyncByte))
                        {
                            Array.Copy(asyncByte.buffer, asyncByte.offset, buffer, len, asyncByte.length);
                            len += asyncByte.length;
                            surLen -= asyncByte.length;
                        }
                    }
                    else if (asyncB.length > buffer.Length)
                    {
                        if (len > 0)
                        {
                            break;
                        }
                        else
                        {
                            asyncByteDe = asyncB;
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            asyncByteDe = new AsyncByte();
            asyncByteDe.buffer = buffer;
            asyncByteDe.length = len;
            return true;
        }

        public void AsyncSend(byte[] buffer, int offset, int length)
        {
            AsyncByte asyncByte = new AsyncByte();
            asyncByte.buffer = buffer;
            asyncByte.offset = offset;
            asyncByte.length = length;
            this.asyncBytes.Enqueue(asyncByte);
            if (!this.sending)
            {
                this.waitHandle.Set();
            }
        }

        public void Load(Socket socket, EndPoint endPoint, ILog logger)
        {
            this.socket = socket;
            this.sendEventArgs.RemoteEndPoint = endPoint;
            this.logger = logger;
        }

        public void Dispose()
        {
            this.dispose = true;
            this.waitHandle.Set();
            this.waitHandle.Dispose();
            this.sendEventArgs.Dispose();
        }
    }
}
