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
using RRQMCore.Collections.Concurrent;
using RRQMCore.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 通道
    /// </summary>
    public class Channel : IDisposable
    {
        internal ByteBlock currentByteBlock;

        internal int id = 0;

        internal ConcurrentDictionary<int, Channel> parent;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly BytePool bytePool;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ProtocolClient client1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ProtocolSocketClient client2;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IntelligentDataQueue<ChannelData> dataQueue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly AutoResetEvent waitHandle;

        private int bufferLength;
        private bool moving;
        private ChannelStatus status;

        internal Channel(ProtocolClient client)
        {
            this.status = ChannelStatus.Moving;
            this.client1 = client;
            this.dataQueue = new IntelligentDataQueue<ChannelData>(cacheCapacity);
            this.waitHandle = new AutoResetEvent(false);
            this.bytePool = client.BytePool;
            this.bufferLength = client.BufferLength;
        }

        internal Channel(ProtocolSocketClient client)
        {
            this.status = ChannelStatus.Moving;
            this.client2 = client;
            this.dataQueue = new IntelligentDataQueue<ChannelData>(cacheCapacity);
            this.waitHandle = new AutoResetEvent(false);
            this.bytePool = client.BytePool;
            this.bufferLength = client.BufferLength;
        }

        private static int cacheCapacity= 1024 * 1024 * 20;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public static int CacheCapacity
        {
            get { return cacheCapacity; }
            set 
            {
                if (value<0)
                {
                    value = 1024;
                }
                cacheCapacity = value; 
            }
        }


        /// <summary>
        /// 析构函数
        /// </summary>
        ~Channel()
        {
            this.Dispose();
        }

        /// <summary>
        /// 获取当前的数据
        /// </summary>
        public byte[] GetCurrent()
        {
            this.currentByteBlock.Pos = 6;
            byte[] data = this.currentByteBlock.ReadBytesPackage();
            this.currentByteBlock.SetHolding(false);
            return data;
        }

        /// <summary>
        /// 获取当前数据的存储块，设置pos=6，调用ReadBytesPackage获取数据。
        /// 使用完成后的数据必须手动释放，且必须调用SetHolding(false)进行释放。
        /// </summary>
        /// <returns></returns>
        public ByteBlock GetCurrentByteBlock()
        {
            return this.currentByteBlock;
        }

        /// <summary>
        /// ID
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public ChannelStatus Status
        {
            get { return status; }
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel()
        {
            if ((byte)this.status > 3)
            {
                return;
            }

            ByteBlock byteBlock = this.bytePool.GetByteBlock(this.bufferLength);
            try
            {
                this.RequestCancel();
                byteBlock.Write(this.id);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(-5, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(-5, byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 异步取消
        /// </summary>
        /// <returns></returns>
        public Task CancelAsync()
        {
            return Task.Run(() =>
            {
                this.Cancel();
            });
        }

        /// <summary>
        /// 完成操作
        /// </summary>
        public void Complete()
        {
            if ((byte)this.status > 3)
            {
                return;
            }

            ByteBlock byteBlock = this.bytePool.GetByteBlock(this.bufferLength);
            try
            {
                this.RequestComplete();
                byteBlock.Write(this.id);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(-4, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(-4, byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 异步完成操作
        /// </summary>
        /// <returns></returns>
        public Task CompleteAsync()
        {
            return Task.Run(() =>
             {
                 this.Complete();
             });
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if ((byte)this.status > 3)
            {
                return;
            }
            GC.SuppressFinalize(this);
            ByteBlock byteBlock = this.bytePool.GetByteBlock(this.bufferLength);
            try
            {
                this.RequestDispose();
                byteBlock.Write(this.id);
                if (this.client1 != null)
                {
                    if (this.client1.Online)
                    {
                        this.client1.SocketSend(-6, byteBlock.Buffer, 0, byteBlock.Len);
                    }
                }
                else
                {
                    if (this.client2.Online)
                    {
                        this.client2.SocketSend(-6, byteBlock.Buffer, 0, byteBlock.Len);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 异步释放
        /// </summary>
        /// <returns></returns>
        public Task DisposeAsync()
        {
            return Task.Run(() =>
            {
                this.Dispose();
            });
        }

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool MoveNext(int timeout = 60 * 1000)
        {
            moving = true;
            if (this.status != ChannelStatus.Moving)
            {
                moving = false;
                return false;
            }

            if (this.dataQueue.TryDequeue(out ChannelData channelData))
            {
                switch (channelData.type)
                {
                    case -3:
                        {
                            this.currentByteBlock = channelData.byteBlock;
                            return true;
                        }
                    case -4:
                        {
                            this.RequestComplete();
                            break;
                        }
                    case -5:
                        {
                            this.RequestCancel();
                            break;
                        }
                    case -6:
                        {
                            this.RequestDispose();
                            break;
                        }
                    default:
                        break;
                }
                moving = false;
                return false;
            }
            else
            {
                this.waitHandle.Reset();
                if (this.waitHandle.WaitOne(timeout))
                {
                    return this.MoveNext(timeout);
                }
                else
                {
                    this.status = ChannelStatus.Timeout;
                    moving = false;
                    return false;
                }
            }
        }

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        public Task<bool> MoveNextAsync(int timeout = 60 * 1000)
        {
            return Task.Run(() =>
             {
                 return this.MoveNext(timeout);
             });
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Write(byte[] data, int offset, int length)
        {
            if ((byte)this.status > 3)
            {
                throw new RRQMException($"通道已{this.status}");
            }
            ByteBlock byteBlock = this.bytePool.GetByteBlock(length + 4);
            try
            {
                byteBlock.Write(this.id);
                byteBlock.WriteBytesPackage(data, offset, length);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(-3, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(-3, byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data)
        {
            this.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void WriteAsync(byte[] data, int offset, int length)
        {
            if ((byte)this.status > 3)
            {
                throw new RRQMException($"通道已{this.status}");
            }

            ByteBlock byteBlock = this.bytePool.GetByteBlock(length + 4);
            try
            {
                byteBlock.Write(this.id);
                byteBlock.WriteBytesPackage(data, offset, length);
                if (this.client1 != null)
                {
                    this.client1.SocketSendAsync(-3, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSendAsync(-3, byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        public void WriteAsync(byte[] data)
        {
            this.WriteAsync(data, 0, data.Length);
        }

        internal void ReceivedData(ChannelData data)
        {
            this.dataQueue.Enqueue(data);
            this.waitHandle.Set();
            if (!moving)
            {
                if (data.type == -4)
                {
                    this.RequestComplete();
                }
                else if (data.type == -5)
                {
                    this.RequestCancel();
                }
                else if (data.type == -6)
                {
                    this.RequestDispose();
                }
            }
        }

        private void RequestCancel()
        {
            this.currentByteBlock = null;
            this.status = ChannelStatus.Cancel;
            this.waitHandle.Set();
        }

        private void RequestComplete()
        {
            this.currentByteBlock = null;
            this.status = ChannelStatus.Completed;
            this.waitHandle.Set();
        }

        private void RequestDispose()
        {
            if ((byte)this.status > 3)
            {
                return;
            }
            this.status = ChannelStatus.Disposed;

            this.currentByteBlock = null;
            this.waitHandle.Set();
            this.waitHandle.Dispose();
            this.status = ChannelStatus.Disposed;
            this.parent.TryRemove(this.id, out _);
            while (this.dataQueue.TryDequeue(out  ChannelData channelData))
            {
                if (channelData.byteBlock!=null)
                {
                    channelData.byteBlock.SetHolding(false);
                }
            }
        }
    }
}