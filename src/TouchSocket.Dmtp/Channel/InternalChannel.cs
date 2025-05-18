//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp;

[DebuggerDisplay("Id={Id},Status={Status}")]
internal sealed partial class InternalChannel : SafetyDisposableObject, IDmtpChannel
{
    private readonly DmtpActor m_actor;
    private readonly ConcurrentQueue<ChannelPackage> m_dataQueue;
    private readonly FlowGate m_flowGate;
    private ByteBlock m_currentData;
    private DateTimeOffset m_lastOperationTime;
    private long m_maxSpeed;

    public InternalChannel(DmtpActor client, string targetId, Metadata metadata)
    {
        this.m_actor = client;
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
        this.TargetId = targetId;
        this.Status = ChannelStatus.Default;
        this.m_dataQueue = new ConcurrentQueue<ChannelPackage>();
        this.m_maxSpeed = int.MaxValue;
        this.m_flowGate = new FlowGate() { Maximum = this.m_maxSpeed };
        this.Metadata = metadata;
    }

    ~InternalChannel()
    {
        this.Dispose(false);
    }

    public int Available => this.m_dataQueue.Count;

    public int CacheCapacity { get; set; }

    public bool CanMoveNext
    {
        get
        {
            return this.Available > 0 || (byte)this.Status < 4;
        }
    }

    public bool CanWrite => (byte)this.Status <= 3;

    public int Id { get; private set; }

    public string LastOperationMes { get; private set; }

    public DateTimeOffset LastOperationTime => this.m_lastOperationTime;

    public long MaxSpeed
    {
        get => this.m_maxSpeed;
        set
        {
            if (value < 1024)
            {
                value = 1024;
            }
            this.m_maxSpeed = value;
            this.m_flowGate.Maximum = value;
        }
    }

    public Metadata Metadata { get; private set; }

    public ChannelStatus Status { get; private set; }

    public string TargetId { get; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    public bool Using { get; private set; }

    #region 操作

    public async Task<Result> CancelAsync(string operationMes = null)
    {
        if ((byte)this.Status > 3)
        {
            return Result.Success;
        }
        try
        {
            this.RequestCancel(true);
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                RunNow = true,
                DataType = ChannelDataType.CancelOrder,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId
            };
            await this.m_actor.SendChannelPackageAsync(channelPackage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> CompleteAsync(string operationMes = null)
    {
        if ((byte)this.Status > 3)
        {
            return Result.Success;
        }
        try
        {
            this.RequestComplete(true);
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                RunNow = true,
                DataType = ChannelDataType.CompleteOrder,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId
            };
            await this.m_actor.SendChannelPackageAsync(channelPackage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }

    }

    public async Task<Result> HoldOnAsync(string operationMes = null)
    {
        if ((byte)this.Status > 3)
        {
            return Result.Success;
        }
        try
        {
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                RunNow = true,
                DataType = ChannelDataType.HoldOnOrder,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId
            };
            await this.m_actor.SendChannelPackageAsync(channelPackage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }

    }


    protected override void SafetyDispose(bool disposing)
    {
        //不判断disposing，能够让GC也能发送释放指令
        try
        {
            this.RequestDispose(true);

            if ((byte)this.Status > 3)
            {
                return;
            }

            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                RunNow = true,
                DataType = ChannelDataType.HoldOnOrder,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId
            };
            this.m_actor.SendChannelPackageAsync(channelPackage).GetFalseAwaitResult();
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
        }
        catch
        {
        }
    }
    
    #endregion 操作

    public ByteBlock GetCurrent()
    {
        return this.m_currentData;
    }

    public bool MoveNext()
    {
        if (!this.CanMoveNext)
        {
            return false;
        }
        if (this.m_dataQueue.TryDequeue(out var channelPackage))
        {
            switch (channelPackage.DataType)
            {
                case ChannelDataType.DataOrder:
                    {
                        this.m_currentData = channelPackage.Data;
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
                    this.Status = ChannelStatus.HoldOn;
                    return false;

                case ChannelDataType.QueueRun:
                    //this.m_canFree = true;
                    return false;

                case ChannelDataType.QueuePause:
                    //this.m_canFree = false;
                    return false;

                default:
                    return false;
            }
        }

        //this.Reset();
        if (this.Wait())
        {
            return this.MoveNext();
        }
        else
        {
            this.Status = ChannelStatus.Overtime;
            return false;
        }
    }

    public async Task<bool> MoveNextAsync()
    {
        if (!this.CanMoveNext)
        {
            return false;
        }
        if (this.m_dataQueue.TryDequeue(out var channelPackage))
        {
            switch (channelPackage.DataType)
            {
                case ChannelDataType.DataOrder:
                    {
                        this.m_currentData = channelPackage.Data;
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
                    this.Status = ChannelStatus.HoldOn;
                    return false;

                case ChannelDataType.QueueRun:
                    //this.m_canFree = true;
                    return false;

                case ChannelDataType.QueuePause:
                    //this.m_canFree = false;
                    return false;

                default:
                    return false;
            }
        }

        //this.Reset();
        if (await this.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            return await this.MoveNextAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            this.Status = ChannelStatus.Overtime;
            return false;
        }
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> memory)
    {
        if ((byte)this.Status > 3)
        {
            throw new Exception($"通道已{this.Status}");
        }

        await this.m_flowGate.AddCheckWaitAsync(memory.Length).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var channelPackage = new ChannelPackage()
        {
            ChannelId = this.Id,
            DataType = ChannelDataType.DataOrder,
            SourceId = this.m_actor.Id,
            TargetId = this.TargetId
        };

        var byteBlock = new ByteBlock(memory.Length);
        byteBlock.Write(memory.Span);
        channelPackage.Data = byteBlock;

        using (channelPackage.Data)
        {
            await this.m_actor.SendChannelPackageAsync(channelPackage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
        }
    }

    internal void ReceivedData(ChannelPackage channelPackage)
    {
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
        if (channelPackage.RunNow)
        {
            switch (channelPackage.DataType)
            {
                case ChannelDataType.CompleteOrder:
                    this.LastOperationMes = channelPackage.Message;
                    this.RequestComplete(false);
                    break;

                case ChannelDataType.CancelOrder:
                    this.LastOperationMes = channelPackage.Message;
                    this.RequestCancel(false);
                    break;

                case ChannelDataType.DisposeOrder:
                    this.RequestDispose(false);
                    break;

                case ChannelDataType.HoldOnOrder:
                    this.LastOperationMes = channelPackage.Message;
                    this.Status = ChannelStatus.HoldOn;
                    break;

                case ChannelDataType.QueueRun:
                    //this.m_canFree = true;
                    return;

                case ChannelDataType.QueuePause:
                    //this.m_canFree = false;
                    return;

                default:
                    return;
            }
        }
        this.m_dataQueue.Enqueue(channelPackage);
    }

    internal void RequestDispose(bool clear)
    {
        if (clear)
        {
            this.Clear();
        }
        if ((byte)this.Status > 3)
        {
            return;
        }
        this.Status = ChannelStatus.Disposed;
    }

    internal void SetId(int id)
    {
        this.Id = id;
    }

    internal void MakeUsing()
    {
        this.Using = true;
    }

    private void Clear()
    {
        try
        {
            this.m_dataQueue.Clear(package =>
            {
                package.Data.SafeDispose();
            });
            this.m_actor.RemoveChannel(this.Id);
        }
        catch
        {
        }
    }

    private void RequestCancel(bool clear)
    {
        this.Status = ChannelStatus.Cancel;
        if (clear)
        {
            this.Clear();
        }
    }

    private void RequestComplete(bool clear)
    {
        this.Status = ChannelStatus.Completed;
        if (clear)
        {
            this.Clear();
        }
    }

    private bool Wait()
    {
        var spinWait = new SpinWait();
        var now = DateTimeOffset.UtcNow;
        while (true)
        {
            if (!this.m_dataQueue.IsEmpty)
            {
                return true;
            }
            if (DateTimeOffset.UtcNow - now > this.Timeout)
            {
                return false;
            }
            spinWait.SpinOnce();
        }
    }

    private async Task<bool> WaitAsync()
    {
        var now = DateTimeOffset.UtcNow;
        while (true)
        {
            if (!this.m_dataQueue.IsEmpty) // Replaced Count with IsEmpty  
            {
                return true;
            }
            if (DateTimeOffset.UtcNow - now > this.Timeout)
            {
                return false;
            }
            await Task.Delay(1).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    #region 迭代器

    public IEnumerator<ByteBlock> GetEnumerator()
    {
        ByteBlock byteBlock = null;
        while (this.MoveNext())
        {
            byteBlock.SafeDispose();
            byteBlock = this.GetCurrent();
            yield return byteBlock;
        }
        byteBlock.SafeDispose();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    #endregion 迭代器
}