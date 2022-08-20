//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.Collections.Concurrent;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 异步独立线程发送器
    /// </summary>
    internal class AsyncSender : DisposableObject
    {
        private static int m_cacheLength = 1024 * 1024 * 100;
        private readonly IntelligentDataQueue<QueueDataBytes> m_asyncBytes;

        private readonly byte[] m_buffer = new byte[1024 * 1024];
        private readonly Action<Exception> m_onError;
        private readonly SocketAsyncEventArgs m_sendEventArgs;

        private readonly Thread m_sendThread;

        private readonly Socket m_socket;
        private readonly EventWaitHandle m_waitHandle;
        private volatile bool m_sending;

        internal AsyncSender(Socket socket, EndPoint endPoint, Action<Exception> onError)
        {
            this.m_sendEventArgs = new SocketAsyncEventArgs();
            this.m_sendEventArgs.Completed += this.SendEventArgs_Completed;
            this.m_socket = socket;
            this.m_sendEventArgs.RemoteEndPoint = endPoint;
            this.m_onError = onError;
            this.m_asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            this.m_waitHandle = new AutoResetEvent(false);
            this.m_sendThread = new Thread(this.BeginSend);
            this.m_sendThread.IsBackground = true;
            this.m_sendThread.Name = "AsyncSendThread";
            this.m_sendThread.Start();
        }

        /// <summary>
        /// 缓存发送池尺寸，
        /// 默认100*1024*1024字节
        /// </summary>
        public static int CacheLength
        {
            get => m_cacheLength;
            set => m_cacheLength = value;
        }

        internal void AsyncSend(byte[] buffer, int offset, int length)
        {
            QueueDataBytes asyncByte = new QueueDataBytes(buffer, offset, length);
            this.m_asyncBytes.Enqueue(asyncByte);
            if (!this.m_sending)
            {
                this.m_sending = true;
                this.m_waitHandle.Set();
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.m_waitHandle.Set();
            this.m_waitHandle.SafeDispose();
            this.m_sendEventArgs.SafeDispose();
            base.Dispose(disposing);
        }

        private void BeginSend()
        {
            while (!this.m_disposedValue)
            {
                try
                {
                    if (this.TryGet(out QueueDataBytes asyncByte))
                    {
                        this.m_sendEventArgs.SetBuffer(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);

                        if (!this.m_socket.SendAsync(this.m_sendEventArgs))
                        {
                            // 同步发送时处理发送完成事件
                            this.ProcessSend(this.m_sendEventArgs);
                        }
                        else
                        {
                            this.m_waitHandle.WaitOne();
                        }
                    }
                    else
                    {
                        this.m_sending = false;
                        this.m_waitHandle.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    this.m_onError?.Invoke(ex);
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
                this.m_onError?.Invoke(new Exception(e.SocketError.ToString()));
            }
        }

        private void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                this.ProcessSend(e);
                if (!this.m_disposedValue)
                {
                    this.m_waitHandle.Set();
                }
            }
        }

        private bool TryGet(out QueueDataBytes asyncByteDe)
        {
            int len = 0;
            int surLen = this.m_buffer.Length;
            while (true)
            {
                if (this.m_asyncBytes.TryPeek(out QueueDataBytes asyncB))
                {
                    if (surLen > asyncB.Length)
                    {
                        if (this.m_asyncBytes.TryDequeue(out QueueDataBytes asyncByte))
                        {
                            Array.Copy(asyncByte.Buffer, asyncByte.Offset, this.m_buffer, len, asyncByte.Length);
                            len += asyncByte.Length;
                            surLen -= asyncByte.Length;
                        }
                    }
                    else if (asyncB.Length > this.m_buffer.Length)
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
            asyncByteDe = new QueueDataBytes(this.m_buffer, 0, len);
            return true;
        }
    }
}