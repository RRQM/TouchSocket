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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Collections.Concurrent;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 通道
    /// </summary>
    public class Channel : DisposableObject
    {
        private readonly IInternalRpc m_client;
        private int m_cacheCapacity;

        private volatile bool m_canFree;
        private ByteBlock m_currentByteBlock;

        private IntelligentDataQueue<ChannelData> m_dataQueue;
        private int m_id;

        private string m_lastOperationMes;
        private AutoResetEvent m_moveWaitHandle;
        private ChannelStatus m_status;
        private string m_targetClientID;
        private bool m_using;

        internal Channel(IInternalRpc client, string targetClientID)
        {
            this.m_client = client;
            this.OnCreate(targetClientID);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~Channel()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 是否具有数据可读
        /// </summary>
        public bool Available => this.m_dataQueue.Count > 0 ? true : false;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int CacheCapacity
        {
            get => this.m_cacheCapacity;
            set
            {
                if (value < 0)
                {
                    value = 1024;
                }
                this.m_cacheCapacity = value;
                this.m_dataQueue.MaxSize = value;
            }
        }

        /// <summary>
        /// 判断当前通道能否调用<see cref="MoveNext()"/>
        /// </summary>
        public bool CanMoveNext
        {
            get
            {
                if ((byte)this.m_status >= 4)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 能否写入
        /// </summary>
        public bool CanWrite => (byte)this.m_status > 3 ? false : true;

        /// <summary>
        /// ID
        /// </summary>
        public int ID => this.m_id;

        /// <summary>
        /// 最后一次操作时显示消息
        /// </summary>
        public string LastOperationMes
        {
            get => this.m_lastOperationMes;
            set => this.m_lastOperationMes = value;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public ChannelStatus Status => this.m_status;

        /// <summary>
        /// 目的ID地址。
        /// </summary>
        public string TargetClientID => this.m_targetClientID;

        /// <summary>
        /// 超时时间，默认1000*10ms。
        /// </summary>
        public int Timeout { get; set; } = 1000 * 10;

        /// <summary>
        /// 是否被使用
        /// </summary>
        public bool Using => this.m_using;

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel(string operationMes = null)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }

            ByteBlock byteBlock = new ByteBlock();
            try
            {
                this.RequestCancel();
                if (this.m_targetClientID != null)
                {
                    byteBlock.Write(this.m_targetClientID);
                }
                byteBlock.Write(this.m_id);
                byteBlock.Write(operationMes);
                this.m_client.SocketSend(this.m_targetClientID == null ? TouchRpcUtility.P_103_CancelOrder : TouchRpcUtility.P_113_CancelOrder_2C, byteBlock);
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
            if ((byte)this.m_status > 3)
            {
                return;
            }

            ByteBlock byteBlock = new ByteBlock();
            try
            {
                this.RequestComplete();
                if (this.m_targetClientID != null)
                {
                    byteBlock.Write(this.m_targetClientID);
                }
                byteBlock.Write(this.m_id);
                byteBlock.Write(operationMes);
                this.m_client.SocketSend(this.m_targetClientID == null ? TouchRpcUtility.P_102_CompleteOrder : TouchRpcUtility.P_112_CompleteOrder_2C, byteBlock.Buffer, 0, byteBlock.Len);
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
            if (this.m_currentByteBlock != null && this.m_currentByteBlock.CanRead)
            {
                this.m_currentByteBlock.Pos = 6;
                byte[] data = this.m_currentByteBlock.ReadBytesPackage();
                this.m_currentByteBlock.SetHolding(false);
                return data;
            }
            return null;
        }

        /// <summary>
        /// 获取当前数据的存储块，设置pos=6，调用ReadBytesPackage获取数据。
        /// 使用完成后的数据必须调用SetHolding(false)进行释放。
        /// </summary>
        /// <returns></returns>
        public ByteBlock GetCurrentByteBlock()
        {
            return this.m_currentByteBlock;
        }

        /// <summary>
        /// 继续。
        /// <para>调用该指令时，接收方会跳出接收，但是通道依然可用，所以接收方需要重新调用<see cref="MoveNext()"/></para>
        /// </summary>
        /// <param name="operationMes"></param>
        public void HoldOn(string operationMes = null)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }

            ByteBlock byteBlock = new ByteBlock();
            try
            {
                if (this.m_targetClientID != null)
                {
                    byteBlock.Write(this.m_targetClientID);
                }
                byteBlock.Write(this.m_id);
                byteBlock.Write(operationMes);
                this.m_client.SocketSend(this.m_targetClientID == null ? TouchRpcUtility.P_105_HoldOnOrder : TouchRpcUtility.P_115_HoldOnOrder_2C, byteBlock.Buffer, 0, byteBlock.Len);
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
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (this.m_status == ChannelStatus.HoldOn || this.m_status == ChannelStatus.Default)
            {
                this.m_lastOperationMes = null;
                this.m_status = ChannelStatus.Moving;
            }
            if (this.m_status != ChannelStatus.Moving)
            {
                return false;
            }

            if (this.m_dataQueue.TryDequeue(out ChannelData channelData))
            {
                if (channelData.type == TouchRpcUtility.P_101_DataOrder)
                {
                    this.m_currentByteBlock = channelData.byteBlock;
                    return true;
                }
                else if (channelData.type == TouchRpcUtility.P_102_CompleteOrder)
                {
                    this.RequestComplete();
                    return false;
                }
                else if (channelData.type == TouchRpcUtility.P_103_CancelOrder)
                {
                    this.RequestCancel();
                    return false;
                }
                else if (channelData.type == TouchRpcUtility.P_104_DisposeOrder)
                {
                    this.RequestDispose();
                    return false;
                }
                else if (channelData.type == TouchRpcUtility.P_105_HoldOnOrder)
                {
                    channelData.byteBlock.Pos = 6;
                    this.m_lastOperationMes = channelData.byteBlock.ReadString();
                    channelData.byteBlock.Dispose();
                    this.m_status = ChannelStatus.HoldOn;
                    return false;
                }
                else
                {
                    return false;
                }
            }

            this.m_moveWaitHandle.Reset();
            if (this.m_moveWaitHandle.WaitOne(this.Timeout))
            {
                return this.MoveNext();
            }
            else
            {
                this.m_status = ChannelStatus.Overtime;
                return false;
            }
        }

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        public Task<bool> MoveNextAsync()
        {
            return Task.Run(() =>
            {
                return this.MoveNext();
            });
        }

        /// <summary>
        /// 阻塞读取数据，直到有数据，或者超时。
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> ReadAsync()
        {
            return Task.Run(() =>
             {
                 if (this.MoveNext())
                 {
                     return this.GetCurrent();
                 }
                 return null;
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
            if ((byte)this.m_status > 3)
            {
                throw new Exception($"通道已{this.m_status}");
            }

            if (!SpinWait.SpinUntil(() => { return this.m_canFree; }, this.Timeout))
            {
                throw new TimeoutException();
            }
            ByteBlock byteBlock = BytePool.GetByteBlock(length + 4);
            try
            {
                if (this.m_targetClientID != null)
                {
                    byteBlock.Write(this.m_targetClientID);
                }
                byteBlock.Write(this.m_id);
                byteBlock.WriteBytesPackage(data, offset, length);
                this.m_client.SocketSend(this.m_targetClientID == null ? TouchRpcUtility.P_101_DataOrder : TouchRpcUtility.P_111_DataOrder_2C, byteBlock.Buffer, 0, byteBlock.Len);
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
            if ((byte)this.m_status > 3)
            {
                throw new Exception($"通道已{this.m_status}");
            }

            ByteBlock byteBlock = BytePool.GetByteBlock(length + 4);
            try
            {
                if (this.m_targetClientID != null)
                {
                    byteBlock.Write(this.m_targetClientID);
                }
                byteBlock.Write(this.m_id);
                byteBlock.WriteBytesPackage(data, offset, length);
                this.m_client.SocketSendAsync(this.m_targetClientID == null ? TouchRpcUtility.P_101_DataOrder : TouchRpcUtility.P_111_DataOrder_2C, byteBlock.Buffer, 0, byteBlock.Len);
            }
            catch (System.Exception)
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
            if (this.m_status != ChannelStatus.Moving)
            {
                if (data.type == TouchRpcUtility.P_102_CompleteOrder)
                {
                    data.byteBlock.Pos = 6;
                    this.m_lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.SetHolding(false);

                    this.RequestComplete();
                    return;
                }
                else if (data.type == TouchRpcUtility.P_103_CancelOrder)
                {
                    data.byteBlock.Pos = 6;
                    this.m_lastOperationMes = data.byteBlock.ReadString();
                    data.byteBlock.SetHolding(false);
                    this.RequestCancel();
                    return;
                }
                else if (data.type == TouchRpcUtility.P_104_DisposeOrder)
                {
                    data.byteBlock.Pos = 6;
                    data.byteBlock.SetHolding(false);
                    this.RequestDispose();
                    return;
                }
                else if (data.type == TouchRpcUtility.P_106_QueueChangedOrder)
                {
                    data.byteBlock.Pos = 6;
                    this.m_canFree = data.byteBlock.ReadBoolean();
                    data.byteBlock.SetHolding(false);
                    return;
                }
            }

            this.m_dataQueue.Enqueue(data);
            this.m_moveWaitHandle.Set();
        }

        internal void SetID(int id)
        {
            this.m_id = id;
        }

        internal void SetUsing()
        {
            this.m_using = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }
            ByteBlock byteBlock = new ByteBlock();
            try
            {
                this.RequestDispose();
                if (this.m_targetClientID != null)
                {
                    byteBlock.Write(this.m_targetClientID);
                }
                byteBlock.Write(this.m_id);
                this.m_client.SocketSend(this.m_targetClientID == null ? TouchRpcUtility.P_104_DisposeOrder : TouchRpcUtility.P_114_DisposeOrder_2C, byteBlock);
            }
            catch
            {
            }
            finally
            {
                byteBlock.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Clear()
        {
            try
            {
                lock (this)
                {
                    this.m_moveWaitHandle.Set();
                    this.m_moveWaitHandle.Dispose();
                    this.m_client.RemoveChannel(this.m_id);
                    this.m_dataQueue.Clear((data) =>
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

        private void OnCreate(string targetClientID)
        {
            this.m_targetClientID = targetClientID;

            //if (upgrade)
            //{
            //    this.m_dataOrder = TouchRpcUtility.P_111_DataOrder_2C;
            //    this.m_completeOrder = TouchRpcUtility.P_112_CompleteOrder_2C;
            //    this.m_cancelOrder = TouchRpcUtility.P_113_CancelOrder_2C;
            //    this.m_disposeOrder = TouchRpcUtility.P_114_DisposeOrder_2C;
            //    this.m_holdOnOrder = TouchRpcUtility.P_115_HoldOnOrder_2C;
            //    this.m_queueChangedOrder = TouchRpcUtility.P_116_QueueChangedOrder_2C;
            //}
            //else
            //{
            //    this.m_dataOrder = TouchRpcUtility.P_101_DataOrder;
            //    this.m_completeOrder = TouchRpcUtility.P_102_CompleteOrder;
            //    this.m_cancelOrder = TouchRpcUtility.P_103_CancelOrder;
            //    this.m_disposeOrder = TouchRpcUtility.P_104_DisposeOrder;
            //    this.m_holdOnOrder = TouchRpcUtility.P_105_HoldOnOrder;
            //    this.m_queueChangedOrder = TouchRpcUtility.P_106_QueueChangedOrder;
            //}

            this.m_status = ChannelStatus.Default;
            this.m_cacheCapacity = 1024 * 1024 * 10;
            this.m_dataQueue = new IntelligentDataQueue<ChannelData>(this.m_cacheCapacity)
            {
                OverflowWait = false,

                OnQueueChanged = OnQueueChanged
            };
            this.m_moveWaitHandle = new AutoResetEvent(false);
            this.m_canFree = true;
        }

        private void OnQueueChanged(bool free)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }

            ByteBlock byteBlock = new ByteBlock();
            try
            {
                if (this.m_targetClientID != null)
                {
                    byteBlock.Write(this.m_targetClientID);
                }
                byteBlock.Write(this.m_id);
                byteBlock.Write(free);
                this.m_client.SocketSend(this.m_targetClientID == null ? TouchRpcUtility.P_106_QueueChangedOrder : TouchRpcUtility.P_116_QueueChangedOrder_2C, byteBlock.Buffer, 0, byteBlock.Len);
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
            this.m_status = ChannelStatus.Cancel;
            this.Clear();
        }

        private void RequestComplete()
        {
            this.m_status = ChannelStatus.Completed;
            this.Clear();
        }

        private void RequestDispose()
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }
            this.m_status = ChannelStatus.Disposed;
            this.Clear();
        }
    }
}