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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 通道
    /// </summary>
    [DebuggerDisplay("ID={ID},Status={Status}")]
    public abstract class Channel : DisposableObject
    {
        /// <summary>
        /// 是否具有数据可读
        /// </summary>
        public abstract int Available { get; }

        /// <summary>
        /// 缓存容量
        /// </summary>
        public abstract int CacheCapacity { get; set; }

        /// <summary>
        /// 判断当前通道能否调用<see cref="MoveNext()"/>
        /// </summary>
        public abstract bool CanMoveNext { get; }

        /// <summary>
        /// 能否写入
        /// </summary>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// 通道ID
        /// </summary>
        public abstract int ID { get; }

        /// <summary>
        /// 最后一次操作时显示消息
        /// </summary>
        public abstract string LastOperationMes { get; }

        /// <summary>
        /// 状态
        /// </summary>
        public abstract ChannelStatus Status { get; }

        /// <summary>
        /// 目的ID地址。仅当该通道由两个客户端打通时有效。
        /// </summary>
        public abstract string TargetId { get; }

        /// <summary>
        /// 超时时间，默认1000*10ms。
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 是否被使用
        /// </summary>
        public abstract bool Using { get; }

        /// <summary>
        /// 取消
        /// </summary>
        public abstract void Cancel(string operationMes = null);

        /// <summary>
        /// 异步取消
        /// </summary>
        /// <returns></returns>
        public Task CancelAsync(string operationMes = null)
        {
            return EasyTask.Run(() =>
            {
                this.Cancel(operationMes);
            });
        }

        /// <summary>
        /// 完成操作
        /// </summary>
        public abstract void Complete(string operationMes = null);

        /// <summary>
        /// 异步完成操作
        /// </summary>
        /// <returns></returns>
        public Task CompleteAsync(string operationMes = null)
        {
            return EasyTask.Run(() =>
            {
                this.Complete(operationMes);
            });
        }

        /// <summary>
        /// 获取当前的数据
        /// </summary>
        public abstract byte[] GetCurrent();

        /// <summary>
        /// 继续。
        /// <para>调用该指令时，接收方会跳出接收，但是通道依然可用，所以接收方需要重新调用<see cref="MoveNext()"/></para>
        /// </summary>
        /// <param name="operationMes"></param>
        public abstract void HoldOn(string operationMes = null);

        /// <summary>
        /// 异步调用继续
        /// </summary>
        /// <param name="operationMes"></param>
        /// <returns></returns>
        public Task HoldOnAsync(string operationMes = null)
        {
            return EasyTask.Run(() =>
            {
                this.HoldOn(operationMes);
            });
        }

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        public abstract bool MoveNext();

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        public Task<bool> MoveNextAsync()
        {
            return EasyTask.Run(() =>
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
            return EasyTask.Run(() =>
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
        public async Task<bool> TryWriteAsync(byte[] data, int offset, int length)
        {
            if (this.CanWrite)
            {
                try
                {
                    await this.WriteAsync(data, offset, length);
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
        public Task<bool> TryWriteAsync(byte[] data)
        {
            return this.TryWriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public abstract void Write(byte[] data, int offset, int length);

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
        public Task WriteAsync(byte[] data, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.Write(data, offset, length);
            });
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        public void WriteAsync(byte[] data)
        {
            this.WriteAsync(data, 0, data.Length);
        }
    }

    internal class InternalChannel : Channel
    {
        private readonly RpcActor m_actor;
        private readonly IntelligentDataQueue<ChannelPackage> m_dataQueue;
        private readonly AutoResetEvent m_moveWaitHandle;
        private readonly string m_targetId;
        private readonly Timer m_timer;
        private int m_cacheCapacity;
        private volatile bool m_canFree;
        private byte[] m_currentData;
        private int m_id;
        private string m_lastOperationMes;
        private DateTime m_lastOperationTime;
        private ChannelStatus m_status;
        private bool m_using;

        public InternalChannel(RpcActor client, string targetId)
        {
            this.m_actor = client;
            this.m_lastOperationTime = DateTime.Now;
            this.m_targetId = targetId;
            this.m_status = ChannelStatus.Default;
            this.m_cacheCapacity = 1024 * 1024 * 5;
            this.m_dataQueue = new IntelligentDataQueue<ChannelPackage>(this.m_cacheCapacity)
            {
                OverflowWait = false,
                OnQueueChanged = OnQueueChanged
            };
            this.m_moveWaitHandle = new AutoResetEvent(false);
            this.m_canFree = true;
            this.m_timer = new Timer((o) =>
            {
                if (DateTime.Now - this.m_lastOperationTime > TimeSpan.FromTicks(this.Timeout.Ticks * 3))
                {
                    this.SafeDispose();
                }
            }, null, 1000, 1000);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~InternalChannel()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 是否具有数据可读
        /// </summary>
        public override int Available => this.m_dataQueue.Count;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public override int CacheCapacity
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
        public override bool CanMoveNext
        {
            get
            {
                if (this.Available > 0)
                {
                    return true;
                }
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
        public override bool CanWrite => (byte)this.m_status <= 3;

        /// <summary>
        /// ID
        /// </summary>
        public override int ID => this.m_id;

        /// <summary>
        /// 最后一次操作时显示消息
        /// </summary>
        public override string LastOperationMes
        {
            get => this.m_lastOperationMes;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public override ChannelStatus Status => this.m_status;

        /// <summary>
        /// 目的ID地址。
        /// </summary>
        public override string TargetId => this.m_targetId;

        /// <summary>
        /// 是否被使用
        /// </summary>
        public override bool Using => this.m_using;

        #region 操作

        /// <summary>
        /// 取消
        /// </summary>
        public override void Cancel(string operationMes = null)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }
            try
            {
                this.RequestCancel(true);
                var channelPackage = new ChannelPackage()
                {
                    ChannelId = this.m_id,
                    RunNow = true,
                    DataType = ChannelDataType.CancelOrder,
                    Message = operationMes,
                    SourceId = this.m_actor.ID,
                    TargetId = this.m_targetId,
                    Route = this.m_targetId.HasValue()
                };
                this.m_actor.SendChannelPackage(channelPackage);
                this.m_lastOperationTime = DateTime.Now;
            }
            catch
            {
            }
        }

        /// <summary>
        /// 完成操作
        /// </summary>
        public override void Complete(string operationMes = null)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }

            this.RequestComplete(true);
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.m_id,
                RunNow = true,
                DataType = ChannelDataType.CompleteOrder,
                Message = operationMes,
                SourceId = this.m_actor.ID,
                TargetId = this.m_targetId,
                Route = this.m_targetId.HasValue()
            };
            this.m_actor.SendChannelPackage(channelPackage);
            this.m_lastOperationTime = DateTime.Now;
        }

        /// <summary>
        /// 继续。
        /// <para>调用该指令时，接收方会跳出接收，但是通道依然可用，所以接收方需要重新调用<see cref="MoveNext()"/></para>
        /// </summary>
        /// <param name="operationMes"></param>
        public override void HoldOn(string operationMes = null)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.m_id,
                RunNow = true,
                DataType = ChannelDataType.HoldOnOrder,
                Message = operationMes,
                SourceId = this.m_actor.ID,
                TargetId = this.m_targetId,
                Route = this.m_targetId.HasValue()
            };
            this.m_actor.SendChannelPackage(channelPackage);
            this.m_lastOperationTime = DateTime.Now;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {



            try
            {
                this.m_timer.SafeDispose();
                this.RequestDispose(true);

                if ((byte)this.m_status > 3)
                {
                    return;
                }

                var channelPackage = new ChannelPackage()
                {
                    ChannelId = this.m_id,
                    RunNow = true,
                    DataType = ChannelDataType.HoldOnOrder,
                    SourceId = this.m_actor.ID,
                    TargetId = this.m_targetId,
                    Route = this.m_targetId.HasValue()
                };
                this.m_actor.SendChannelPackage(channelPackage);
                this.m_lastOperationTime = DateTime.Now;
            }
            catch
            {
            }
            base.Dispose(disposing);
        }

        #endregion 操作

        /// <summary>
        /// 获取当前的数据
        /// </summary>
        public override byte[] GetCurrent()
        {
            return this.m_currentData;
        }

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        public override bool MoveNext()
        {
            if (!this.CanMoveNext)
            {
                return false;
            }
            if (this.m_dataQueue.TryDequeue(out ChannelPackage channelPackage))
            {
                switch (channelPackage.DataType)
                {
                    case ChannelDataType.DataOrder:
                        {
                            this.m_currentData = channelPackage.Data.Array;
                            return true;
                        }
                    case ChannelDataType.CompleteOrder:
                        this.RequestComplete(true);
                        return false;

                    case ChannelDataType.CancelOrder:
                        this.RequestCancel(true);
                        return false;

                    case ChannelDataType.DisposeOrder:
                        this.RequestDispose(true);
                        return false;

                    case ChannelDataType.HoldOnOrder:
                        this.m_status = ChannelStatus.HoldOn;
                        return false;

                    case ChannelDataType.QueueRun:
                        this.m_canFree = true;
                        return false;

                    case ChannelDataType.QueuePause:
                        this.m_canFree = false;
                        return false;

                    default:
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
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Write(byte[] data, int offset, int length)
        {
            if ((byte)this.m_status > 3)
            {
                throw new Exception($"通道已{this.m_status}");
            }

            if (!SpinWait.SpinUntil(() => { return this.m_canFree; }, this.Timeout))
            {
                throw new TimeoutException();
            }
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.m_id,
                DataType = ChannelDataType.DataOrder,
                Data = new ArraySegment<byte>(data, offset, length),
                SourceId = this.m_actor.ID,
                TargetId = this.m_targetId,
                Route = this.m_targetId.HasValue()
            };
            this.m_actor.SendChannelPackage(channelPackage);
            this.m_lastOperationTime = DateTime.Now;
        }

        internal void ReceivedData(ChannelPackage channelPackage)
        {
            this.m_lastOperationTime = DateTime.Now;
            if (channelPackage.RunNow)
            {
                switch (channelPackage.DataType)
                {
                    case ChannelDataType.CompleteOrder:
                        this.m_lastOperationMes = channelPackage.Message;
                        this.RequestComplete(false);
                        break;

                    case ChannelDataType.CancelOrder:
                        this.m_lastOperationMes = channelPackage.Message;
                        this.RequestCancel(false);
                        break;

                    case ChannelDataType.DisposeOrder:
                        this.RequestDispose(false);
                        break;

                    case ChannelDataType.HoldOnOrder:
                        this.m_lastOperationMes = channelPackage.Message;
                        this.m_status = ChannelStatus.HoldOn;
                        break;

                    case ChannelDataType.QueueRun:
                        this.m_canFree = true;
                        return;

                    case ChannelDataType.QueuePause:
                        this.m_canFree = false;
                        return;

                    default:
                        return;
                }
            }
            this.m_dataQueue.Enqueue(channelPackage);
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

        private void Clear()
        {
            try
            {
                lock (this)
                {
                    this.m_dataQueue.Clear();
                    if (this.m_actor.RemoveChannel(this.m_id))
                    {
                        this.m_moveWaitHandle.Set();
                        this.m_moveWaitHandle.SafeDispose();
                    }
                }
            }
            catch
            {
            }
        }

        private void OnQueueChanged(bool free)
        {
            if ((byte)this.m_status > 3)
            {
                return;
            }

            try
            {
                var channelPackage = new ChannelPackage()
                {
                    ChannelId = this.m_id,
                    RunNow = true,
                    DataType = free ? ChannelDataType.QueueRun : ChannelDataType.QueuePause,
                    SourceId = this.m_actor.ID,
                    TargetId = this.m_targetId,
                    Route = this.m_targetId.HasValue()
                };
                this.m_actor.SendChannelPackage(channelPackage);
                this.m_lastOperationTime = DateTime.Now;
            }
            catch
            {
            }
        }

        private void RequestCancel(bool clear)
        {
            this.m_status = ChannelStatus.Cancel;
            if (clear)
            {
                this.Clear();
            }
        }

        private void RequestComplete(bool clear)
        {
            this.m_status = ChannelStatus.Completed;
            if (clear)
            {
                this.Clear();
            }
        }

        internal void RequestDispose(bool clear)
        {
            if (clear)
            {
                this.Clear();
            }
            if ((byte)this.m_status > 3)
            {
                return;
            }
            this.m_status = ChannelStatus.Disposed;
        }
    }
}