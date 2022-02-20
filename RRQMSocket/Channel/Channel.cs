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
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Collections.Concurrent;

using System;
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
        private ByteBlock currentByteBlock;

        internal int id = 0;

        private int cacheCapacity;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ProtocolClient client1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ProtocolSocketClient client2;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IntelligentDataQueue<ChannelData> dataQueue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private AutoResetEvent moveWaitHandle;

        private int bufferLength;
        private bool canFree;
        private string lastOperationMes;
        private bool moving;
        private ChannelStatus status;
        internal bool @using;
        private string targetClientID;
        private short dataOrder;
        private short completeOrder;
        private short cancelOrder;
        private short disposeOrder;
        private short holdOnOrder;
        private short queueChangedOrder;

        internal Channel(ProtocolClient client, string targetClientID)
        {
            this.client1 = client;
            this.bufferLength = client.BufferLength;
            this.OnCreate(targetClientID);
        }

        internal Channel(ProtocolSocketClient client, string targetClientID)
        {
            this.client2 = client;
            this.bufferLength = client.BufferLength;
            this.OnCreate(targetClientID);
        }

        private void OnCreate(string targetClientID)
        {
            if (targetClientID == null)
            {
                this.dataOrder = -3;
                this.completeOrder = -4;
                this.cancelOrder = -5;
                this.disposeOrder = -6;
                this.holdOnOrder = -11;
                this.queueChangedOrder = -10;
            }
            else
            {
                this.dataOrder = -14;
                this.completeOrder = -15;
                this.cancelOrder = -16;
                this.disposeOrder = -17;
                this.holdOnOrder = -18;
                this.queueChangedOrder = -19;

                this.targetClientID = targetClientID;
            }

            this.status = ChannelStatus.Moving;
            this.cacheCapacity = 1024 * 1024 * 20;
            this.dataQueue = new IntelligentDataQueue<ChannelData>(this.cacheCapacity)
            {
                OverflowWait = false,

                OnQueueChanged = OnQueueChanged
            };
            this.moveWaitHandle = new AutoResetEvent(false);
            this.canFree = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~Channel()
        {
            this.Dispose();
        }

        /// <summary>
        /// 目的ID地址。
        /// </summary>
        public string TargetClientID
        {
            get { return this.targetClientID; }
        }

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int CacheCapacity
        {
            get { return this.cacheCapacity; }
            set
            {
                if (value < 0)
                {
                    value = 1024;
                }
                this.cacheCapacity = value;
                this.dataQueue.MaxSize = value;
            }
        }

        /// <summary>
        /// 是否具有数据可读
        /// </summary>
        public bool Available
        {
            get { return this.dataQueue.Count > 0 ? true : false; }
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
            get { return this.id; }
        }

        /// <summary>
        /// 最后一次操作时显示消息
        /// </summary>
        public string LastOperationMes
        {
            get { return this.lastOperationMes; }
            set { this.lastOperationMes = value; }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public ChannelStatus Status
        {
            get { return this.status; }
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
                if (this.targetClientID != null)
                {
                    byteBlock.Write(this.targetClientID);
                }
                byteBlock.Write(this.id);
                byteBlock.Write(operationMes);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(this.cancelOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(this.cancelOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
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
                if (this.targetClientID != null)
                {
                    byteBlock.Write(this.targetClientID);
                }
                byteBlock.Write(this.id);
                byteBlock.Write(operationMes);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(this.completeOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(this.completeOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 继续。
        /// <para>调用该指令时，接收方会跳出接收，但是通道依然可用，所以接收方需要重新调用<see cref="MoveNext(int)"/></para>
        /// </summary>
        /// <param name="operationMes"></param>
        public void HoldOn(string operationMes = null)
        {
            if ((byte)this.status > 3)
            {
                return;
            }

            ByteBlock byteBlock = BytePool.GetByteBlock(this.bufferLength);
            try
            {
                if (this.targetClientID != null)
                {
                    byteBlock.Write(this.targetClientID);
                }
                byteBlock.Write(this.id);
                byteBlock.Write(operationMes);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(this.holdOnOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(this.holdOnOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 异步调用继续
        /// </summary>
        /// <param name="operationMes"></param>
        /// <returns></returns>
        public Task HoldOnAsync(string operationMes = null)
        {
            return Task.Run(() =>
            {
                this.HoldOn(operationMes);
            });
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
                if (this.targetClientID != null)
                {
                    byteBlock.Write(this.targetClientID);
                }
                byteBlock.Write(this.id);
                if (this.client1 != null)
                {
                    if (this.client1.Online)
                    {
                        this.client1.SocketSend(this.disposeOrder, byteBlock.Buffer, 0, byteBlock.Len);
                    }
                }
                else
                {
                    if (this.client2.Online)
                    {
                        this.client2.SocketSend(this.disposeOrder, byteBlock.Buffer, 0, byteBlock.Len);
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
        /// 判断当前通道能否调用<see cref="MoveNext(int)"/>
        /// </summary>
        public bool CanMoveNext
        {
            get
            {
                if ((byte)this.status > 4)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool MoveNext(int timeout = 60 * 1000)
        {
            if (this.status == ChannelStatus.HoldOn)
            {
                this.lastOperationMes = null;
                this.status = ChannelStatus.Moving;
            }
            this.moving = true;
            if (this.status != ChannelStatus.Moving)
            {
                this.moving = false;
                return false;
            }

            if (this.dataQueue.TryDequeue(out ChannelData channelData))
            {
                if (channelData.type == this.dataOrder)
                {
                    this.currentByteBlock = channelData.byteBlock;
                    return true;
                }
                else if (channelData.type == this.completeOrder)
                {
                    this.RequestComplete();
                    this.moving = false;
                    return false;
                }
                else if (channelData.type == this.cancelOrder)
                {
                    this.RequestCancel();
                    this.moving = false;
                    return false;
                }
                else if (channelData.type == this.disposeOrder)
                {
                    this.RequestDispose();
                    this.moving = false;
                    return false;
                }
                else if (channelData.type == this.holdOnOrder)
                {
                    channelData.byteBlock.Pos = 6;
                    this.lastOperationMes = channelData.byteBlock.ReadString();
                    channelData.byteBlock.Dispose();
                    this.status = ChannelStatus.HoldOn;
                    this.moving = false;
                    return false;
                }
                else
                {
                    return false;
                }
            }

            this.moveWaitHandle.Reset();
            if (this.moveWaitHandle.WaitOne(timeout))
            {
                return this.MoveNext(timeout);
            }
            else
            {
                this.status = ChannelStatus.Overtime;
                this.moving = false;
                return false;
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

            SpinWait.SpinUntil(() => { return this.canFree; });
            ByteBlock byteBlock = BytePool.GetByteBlock(length + 4);
            try
            {
                if (this.targetClientID != null)
                {
                    byteBlock.Write(this.targetClientID);
                }
                byteBlock.Write(this.id);
                byteBlock.WriteBytesPackage(data, offset, length);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(this.dataOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(this.dataOrder, byteBlock.Buffer, 0, byteBlock.Len);
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
                if (this.targetClientID != null)
                {
                    byteBlock.Write(this.targetClientID);
                }
                byteBlock.Write(this.id);
                byteBlock.WriteBytesPackage(data, offset, length);
                if (this.client1 != null)
                {
                    this.client1.SocketSendAsync(this.dataOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSendAsync(this.dataOrder, byteBlock.Buffer, 0, byteBlock.Len);
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
            this.moveWaitHandle.Set();
            if (!this.moving)
            {
                if (data.type == this.completeOrder)
                {
                    data.byteBlock.Pos = 6;
                    this.lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.Dispose();

                    this.RequestComplete();
                }
                else if (data.type == this.cancelOrder)
                {
                    data.byteBlock.Pos = 6;
                    this.lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.Dispose();
                    this.RequestCancel();
                }
                else if (data.type == this.disposeOrder)
                {
                    data.byteBlock.Pos = 6;
                    this.lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.Dispose();
                    this.RequestDispose();
                }
                else if (data.type == this.queueChangedOrder)
                {
                    data.byteBlock.Pos = 6;
                    this.canFree = data.byteBlock.ReadBoolean();
                    data.byteBlock.Dispose();
                }
            }
        }

        private void Clear()
        {
            try
            {
                lock (this)
                {
                    this.moveWaitHandle.Set();
                    this.moveWaitHandle.Dispose();
                    if (this.client1 != null)
                    {
                        this.client1.RemoveChannel(this.id);
                    }
                    else if (this.client2 != null)
                    {
                        this.client2.RemoveChannel(this.id);
                    }
                    else
                    {
                        return;
                    }

                    this.dataQueue.Clear((data) =>
                    {
                        if (data.byteBlock != null)
                        {
                            data.byteBlock.SetHolding(false);
                        }
                    });
                }
            }
            catch
            {
            }
        }

        private void OnQueueChanged(bool free)
        {
            if ((byte)this.status > 3)
            {
                return;
            }

            ByteBlock byteBlock = BytePool.GetByteBlock(this.bufferLength);
            try
            {
                if (this.targetClientID != null)
                {
                    byteBlock.Write(this.targetClientID);
                }
                byteBlock.Write(this.id);
                byteBlock.Write(free);
                if (this.client1 != null)
                {
                    this.client1.SocketSend(this.queueChangedOrder, byteBlock.Buffer, 0, byteBlock.Len);
                }
                else
                {
                    this.client2.SocketSend(this.queueChangedOrder, byteBlock.Buffer, 0, byteBlock.Len);
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
    }
}