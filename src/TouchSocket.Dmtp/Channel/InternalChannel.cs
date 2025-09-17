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
    private readonly SemaphoreSlim m_dataAvailable;
    private readonly Queue<ChannelPackage> m_dataQueue;
    private DateTimeOffset m_lastOperationTime;
    private bool m_using;

    public InternalChannel(DmtpActor client, string targetId, Metadata metadata)
    {
        this.m_actor = client;
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
        this.TargetId = targetId;
        this.Status = ChannelStatus.Default;
        this.m_dataQueue = new Queue<ChannelPackage>();
        this.m_dataAvailable = new SemaphoreSlim(0);
        this.Metadata = metadata;
    }

    /// <inheritdoc/>
    public bool CanRead => this.Status == ChannelStatus.Default ||
                                   this.Status == ChannelStatus.HoldOn;

    public bool CanWrite => this.m_actor.Online && (byte)this.Status <= 1;
    public int Id { get; private set; }

    public string LastOperationMes { get; private set; }

    public DateTimeOffset LastOperationTime => this.m_lastOperationTime;

    public Metadata Metadata { get; private set; }

    public ChannelStatus Status { get; private set; }

    public string TargetId { get; }

    public bool Using => this.m_using;

    #region 读取

    /// <inheritdoc/>
    public async Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken token = default)
    {
        if (!this.CanRead)
        {
            throw new InvalidOperationException($"通道状态为{this.Status}，无法读取数据");
        }

        if (this.DisposedValue)
        {
            throw new ObjectDisposedException(nameof(InternalChannel), "通道已被释放");
        }

        var channelPackage = await this.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        try
        {
            switch (channelPackage.DataType)
            {
                case ChannelDataType.Data:
                    var data = channelPackage.Data;
                    return data;

                case ChannelDataType.Completed:
                    this.Status = ChannelStatus.Completed;
                    this.LastOperationMes = channelPackage.Message;
                    // 通道正常完成，返回空数据表示没有更多数据
                    return ReadOnlyMemory<byte>.Empty;

                case ChannelDataType.Canceled:
                    this.Status = ChannelStatus.Cancel;
                    this.LastOperationMes = channelPackage.Message;
                    // 通道被取消，返回空数据，调用者可以检查状态决定是否继续
                    return ReadOnlyMemory<byte>.Empty;

                case ChannelDataType.HoldOn:
                    this.Status = ChannelStatus.HoldOn;
                    this.LastOperationMes = channelPackage.Message;
                    // 通道暂停，返回空数据，调用者可以检查状态决定是否继续
                    return ReadOnlyMemory<byte>.Empty;

                default:
                    throw new InvalidOperationException("未知的通道数据类型");
            }
        }
        finally
        {
            channelPackage.SafeDispose();
        }
    }

    #endregion 读取

    #region 操作

    public async Task<Result> CancelAsync(string operationMes = null, CancellationToken cancellationToken = default)
    {
        if ((byte)this.Status > 1)
        {
            return Result.Success;
        }
        try
        {
            this.Status = ChannelStatus.Cancel;
            this.LastOperationMes = operationMes;
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                DataType = ChannelDataType.Canceled,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId
            };
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> CompleteAsync(string operationMes = null, CancellationToken cancellationToken = default)
    {
        if ((byte)this.Status > 1)
        {
            return Result.Success;
        }
        try
        {
            this.Status = ChannelStatus.Completed;
            this.LastOperationMes = operationMes;
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                DataType = ChannelDataType.Completed,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId
            };
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> HoldOnAsync(string operationMes = null, CancellationToken cancellationToken = default)
    {
        if ((byte)this.Status > 1)
        {
            return Result.Success;
        }
        try
        {
            this.Status = ChannelStatus.HoldOn;
            this.LastOperationMes = operationMes;
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                DataType = ChannelDataType.HoldOn,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId
            };
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    #endregion 操作

    public async Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        if ((byte)this.Status > 1)
        {
            throw new Exception($"通道已{this.Status}");
        }
        var channelPackage = new ChannelPackage()
        {
            ChannelId = this.Id,
            DataType = ChannelDataType.Data,
            SourceId = this.m_actor.Id,
            TargetId = this.TargetId,
            Data = memory
        };
        await this.m_actor.SendChannelPackageAsync(channelPackage, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
    }

    internal void MakeUsing()
    {
        this.m_using = true;
    }

    internal void ReceivedData(ChannelPackage channelPackage)
    {
        if (this.DisposedValue)
        {
            return;
        }
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
        lock (this.m_dataQueue)
        {
            this.m_dataQueue.Enqueue(channelPackage);
        }
        this.m_dataAvailable.Release();
    }

    internal void SetId(int id)
    {
        this.Id = id;
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_dataAvailable.SafeDispose();
            lock (this.m_dataQueue)
            {
                while (this.m_dataQueue.Count > 0)
                {
                    this.m_dataQueue.Dequeue().SafeDispose();
                }
            }
        }
    }

    private async Task<ChannelPackage> WaitAsync(CancellationToken token)
    {
        await this.m_dataAvailable.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        lock (this.m_dataQueue)
        {
            if (this.m_dataQueue.Count > 0)
            {
                return this.m_dataQueue.Dequeue();
            }
        }
        // 理论上不会走到这里
        throw new InvalidOperationException("信号量与队列不同步");
    }
}