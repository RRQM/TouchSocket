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

namespace RRQMSocket
{
    internal class AsyncSender:IDisposable
    {
        internal AsyncSender()
        {
            byteBlockQueue = new ConcurrentQueue<ByteBlock>();
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
        private ConcurrentQueue<ByteBlock> byteBlockQueue;
        private Socket socket;
        private bool sending;

        private void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                ProcessSend(e);
            }
            else
            {
                try
                {
                    ((ByteBlock)e.UserToken).SetHolding(false);
                }
                catch
                {

                }
                finally
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
                ((ByteBlock)e.UserToken).SetHolding(false);
                if (e.SocketError != SocketError.Success)
                {
                    //this.Logger.Debug(LogType.Error, this, "异步发送错误。");
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
                this.waitHandle.WaitOne();
                if (this.byteBlockQueue.TryDequeue(out ByteBlock byteBlock))
                {
                    this.sending = true;
                    this.sendEventArgs.UserToken = byteBlock;
                    this.sendEventArgs.SetBuffer(byteBlock.Buffer, 0, (int)byteBlock.Length);
                    if (!this.socket.SendAsync(this.sendEventArgs))
                    {
                        // 同步发送时处理发送完成事件
                        this.ProcessSend(sendEventArgs);
                    }
                }
                else
                {
                    this.sending = false;
                }
            }
        }

        public void AsyncSend(ByteBlock byteBlock)
        {
            if (byteBlock!=null)
            {
                byteBlock.SetHolding(true);
                this.byteBlockQueue.Enqueue(byteBlock);
                if (!this.sending)
                {
                    this.waitHandle.Set();
                }
            }
        }

        public void Load(Socket socket,EndPoint endPoint)
        {
            this.socket = socket;
            this.sendEventArgs.RemoteEndPoint = endPoint;
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
