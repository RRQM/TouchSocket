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

        private static int cacheCapacity = 1024 * 1024 * 20;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ProtocolClient client1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ProtocolSocketClient client2;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IntelligentDataQueue<ChannelData> dataQueue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly AutoResetEvent waitHandle;

        private int bufferLength;
        private string lastOperationMes;
        private bool moving;
        private ChannelStatus status;

        internal Channel(ProtocolClient client)
        {
            this.status = ChannelStatus.Moving;
            this.client1 = client;
            this.dataQueue = new IntelligentDataQueue<ChannelData>(cacheCapacity);
            this.waitHandle = new AutoResetEvent(false);
            this.bufferLength = client.BufferLength;
        }

        internal Channel(ProtocolSocketClient client)
        {
            this.status = ChannelStatus.Moving;
            this.client2 = client;
            this.dataQueue = new IntelligentDataQueue<ChannelData>(cacheCapacity);
            this.waitHandle = new AutoResetEvent(false);
            this.bufferLength = client.BufferLength;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~Channel()
        {
            this.Dispose();
        }

        /// <summary>
        /// 收到数据时触发，
        /// 执行该事件时，数据还未到缓存池，所以无法触发已到缓存池的数据。
        /// </summary>
        public event RRQMChannelReceivedEventHandler Received;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public static int CacheCapacity
        {
            get { return cacheCapacity; }
            set
            {
                if (value < 0)
                {
                    value = 1024;
                }
                cacheCapacity = value;
            }
        }

        /// <summary>
        /// 是否具有数据可读
        /// </summary>
        public bool Available
        {
            get { return dataQueue.Count > 0 ? true : false; }
        }

        /// <summary>
        /// 能否写入
        /// </summary>
        public bool CanWrite { get => (byte)this.status > 3 ? false : true; }

        /// <summary>
        /// ID
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// 最后一次操作时显示消息
        /// </summary>
        public string LastOperationMes
        {
            get { return lastOperationMes; }
            set { lastOperationMes = value; }
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
        public void Cancel(string operationMes = null)
        {
            if ((byte)this.status > 3)
            {
                return;
            }

            ByteBlock byteBlock = BytePool.GetByteBlock(this.bufferLength);
            try
            {
                this.RequestCancel();
                byteBlock.Write(this.id);
                byteBlock.Write(operationMes);
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
        public Task CancelAsync(string operationMes = null)
        {
            return Task.Run(() =>
            {
                this.Cancel(operationMes);
            });
        }

        /// <summary>
        /// 完成操作
        /// </summary>
        public void Complete(string operationMes = null)
        {
            if ((byte)this.status > 3)
            {
                return;
            }

            ByteBlock byteBlock = BytePool.GetByteBlock(this.bufferLength);
            try
            {
                this.RequestComplete();
                byteBlock.Write(this.id);
                byteBlock.Write(operationMes);
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
        public Task CompleteAsync(string operationMes = null)
        {
            return Task.Run(() =>
             {
                 this.Complete(operationMes);
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
            ByteBlock byteBlock = BytePool.GetByteBlock(this.bufferLength);
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
        /// 获取当前的数据
        /// </summary>
        public byte[] GetCurrent()
        {
            if (this.currentByteBlock != null && this.currentByteBlock.CanRead)
            {
                this.currentByteBlock.Pos = 6;
                byte[] data = this.currentByteBlock.ReadBytesPackage();
                this.currentByteBlock.SetHolding(false);
                return data;
            }
            return null;
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
                    this.status = ChannelStatus.Overtime;
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
        /// 尝试写入。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool TryWrite(byte[] data, int offset, int length)
        {
            if (this.CanWrite)
            {
                try
                {
                    this.Write(data, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试写入
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryWrite(byte[] data)
        {
            return this.TryWrite(data, 0, data.Length);
        }

        /// <summary>
        /// 异步尝试写入
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool TryWriteAsync(byte[] data, int offset, int length)
        {
            if (this.CanWrite)
            {
                try
                {
                    this.WriteAsync(data, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 异步尝试写入
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryWriteAsync(byte[] data)
        {
            return this.TryWriteAsync(data, 0, data.Length);
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
            ByteBlock byteBlock = BytePool.GetByteBlock(length + 4);
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

            ByteBlock byteBlock = BytePool.GetByteBlock(length + 4);
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
            if (data.type == -3 && this.Received != null)
            {
                data.byteBlock.Pos = 6;
                byte[] dataBuffer = data.byteBlock.ReadBytesPackage();
                BytesHandledEventArgs args = new BytesHandledEventArgs(dataBuffer);
                this.Received.Invoke(this, args);

                data.byteBlock.Pos = 6;

                if (args.Handled)
                {
                    data.byteBlock.SetHolding(false);
                    return;
                }
            }

            this.dataQueue.Enqueue(data);
            this.waitHandle.Set();
            if (!moving)
            {
                if (data.type == -4)
                {
                    data.byteBlock.Pos = 6;
                    this.lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.Dispose();

                    this.RequestComplete();
                }
                else if (data.type == -5)
                {
                    data.byteBlock.Pos = 6;
                    this.lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.Dispose();
                    this.RequestCancel();
                }
                else if (data.type == -6)
                {
                    data.byteBlock.Pos = 6;
                    this.lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.Dispose();
                    this.RequestDispose();
                }
            }
        }

        private void RequestCancel()
        {
            this.status = ChannelStatus.Cancel;
            this.Clear();
        }

        private void RequestComplete()
        {
            this.status = ChannelStatus.Completed;
            this.Clear();
        }

        private void RequestDispose()
        {
            if ((byte)this.status > 3)
            {
                return;
            }
            this.status = ChannelStatus.Disposed;
            this.Clear();
        }

        private void Clear()
        {
            try
            {
                this.currentByteBlock = null;
                this.waitHandle.Set();
                this.waitHandle.Dispose();
                this.parent.TryRemove(this.id, out _);
                while (this.dataQueue.TryDequeue(out ChannelData channelData))
                {
                    if (channelData.byteBlock != null)
                    {
                        channelData.byteBlock.SetHolding(false);
                    }
                }
            }
            catch
            {
            }
        }
    }
}