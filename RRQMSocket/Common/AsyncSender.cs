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

        private SocketAsyncEventArgs sendEventArgs;
        private Thread sendThread;
        private EventWaitHandle waitHandle;
        private bool dispose;
        private ConcurrentQueue<AsyncByte> asyncBytes;
        private Socket socket;
        private bool sending;
        private ILog logger;

        private void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                ProcessSend(e);
            }
            else
            {
                this.waitHandle.Set();
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
                this.waitHandle.Set();
            }
        }

        private void BeginSend()
        {
            while (!this.dispose)
            {
                if (!this.sending && this.asyncBytes.TryDequeue(out AsyncByte asyncByte))
                {
                    this.sending = true;
                    this.sendEventArgs.SetBuffer(asyncByte.buffer, asyncByte.offset, asyncByte.length);
                    if (!this.socket.SendAsync(this.sendEventArgs))
                    {
                        // 同步发送时处理发送完成事件
                        this.ProcessSend(sendEventArgs);
                    }
                    this.waitHandle.WaitOne();
                    lock (this)
                    {
                        this.sending = false;
                    }
                   
                }
                else
                {
                    this.waitHandle.WaitOne();
                    lock (this)
                    {
                        this.sending = false;
                    }
                }
            }
        }

        public void AsyncSend(byte[] buffer, int offset, int length)
        {
            AsyncByte asyncByte = new AsyncByte();
            asyncByte.buffer = buffer;
            asyncByte.offset = offset;
            asyncByte.length = length;
            this.asyncBytes.Enqueue(asyncByte);
            lock (this)
            {
                if (!this.sending)
                {
                    this.waitHandle.Set();
                }
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
