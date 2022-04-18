//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Collections.Concurrent;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 异步独立线程发送器
    /// </summary>
    public class AsyncSender
    {
        private readonly IntelligentDataQueue<QueueDataBytes> asyncBytes;

        private readonly SocketAsyncEventArgs sendEventArgs;

        private readonly Thread sendThread;

        private readonly EventWaitHandle waitHandle;

        private byte[] buffer = new byte[1024 * 1024];

        private volatile bool dispose;

        private Action<Exception> onError;

        private volatile bool sending;

        private Socket socket;

        private static int cacheLength = 1024 * 1024 * 100;

        /// <summary>
        /// 缓存发送池尺寸，
        /// 默认100*1024*1024字节
        /// </summary>
        public static int CacheLength
        {
            get => cacheLength;
            set => cacheLength = value;
        }

        internal AsyncSender(Socket socket, EndPoint endPoint, Action<Exception> onError)
        {
            this.sendEventArgs = new SocketAsyncEventArgs();
            this.sendEventArgs.Completed += this.SendEventArgs_Completed;
            this.socket = socket;
            this.sendEventArgs.RemoteEndPoint = endPoint;
            this.onError = onError;
            this.asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            this.waitHandle = new AutoResetEvent(false);
            this.sendThread = new Thread(this.BeginSend);
            this.sendThread.IsBackground = true;
            this.sendThread.Name = "AsyncSendThread";
            this.sendThread.Start();
        }

        internal void AsyncSend(byte[] buffer, int offset, int length)
        {
            QueueDataBytes asyncByte = new QueueDataBytes(buffer, offset, length);
            this.asyncBytes.Enqueue(asyncByte);
            if (!this.sending)
            {
                this.sending = true;
                this.waitHandle.Set();
            }
        }

        internal void Dispose()
        {
            this.dispose = true;
            this.waitHandle.Set();
            this.waitHandle.Dispose();
            this.sendEventArgs.Dispose();
        }

        private void BeginSend()
        {
            while (!this.dispose)
            {
                try
                {
                    if (this.tryGet(out QueueDataBytes asyncByte))
                    {
                        this.sendEventArgs.SetBuffer(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);

                        if (!this.socket.SendAsync(this.sendEventArgs))
                        {
                            // 同步发送时处理发送完成事件
                            this.ProcessSend(this.sendEventArgs);
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
                    this.onError?.Invoke(ex);
                }
            }
        }

        /// <summary>
        /// 发送完成时处理函数
        /// </summary>
        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                this.onError?.Invoke(new Exception(e.SocketError.ToString()));
            }
        }

        private void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                this.ProcessSend(e);
                if (!this.dispose)
                {
                    this.waitHandle.Set();
                }
            }
        }

        private bool tryGet(out QueueDataBytes asyncByteDe)
        {
            int len = 0;
            int surLen = this.buffer.Length;
            while (true)
            {
                if (this.asyncBytes.TryPeek(out QueueDataBytes asyncB))
                {
                    if (surLen > asyncB.Length)
                    {
                        if (this.asyncBytes.TryDequeue(out QueueDataBytes asyncByte))
                        {
                            Array.Copy(asyncByte.Buffer, asyncByte.Offset, this.buffer, len, asyncByte.Length);
                            len += asyncByte.Length;
                            surLen -= asyncByte.Length;
                        }
                    }
                    else if (asyncB.Length > this.buffer.Length)
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
                    if (len > 0)
                    {
                        break;
                    }
                    else
                    {
                        asyncByteDe = default;
                        return false;
                    }
                }
            }
            asyncByteDe = new QueueDataBytes(this.buffer, 0, len);
            return true;
        }
    }
}