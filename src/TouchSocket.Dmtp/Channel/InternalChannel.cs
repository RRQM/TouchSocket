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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Dmtp;

[DebuggerDisplay("Id={Id},Status={Status}")]
internal sealed partial class InternalChannel : SafetyDisposableObject, IDmtpChannel
{
    private readonly DmtpActor m_actor;
    private readonly Lock m_syncRoot = new();
    private volatile bool m_closed;
    private DateTimeOffset m_lastOperationTime;
    private int m_using;
    private int m_reading;
    private ChannelPackage m_readPackage;

    internal void SetId(int id)
    {
        this.Id = id;
    }

    internal void MakeUsing()
    {
        Interlocked.Exchange(ref this.m_using, 1);
    }

    internal bool TryMakeUsing() => !this.DisposedValue && Interlocked.CompareExchange(ref this.m_using, 1, 0) == 0;

    public bool CanRead => !this.m_closed && !this.DisposedValue && (this.Status == ChannelStatus.Default || this.Status == ChannelStatus.HoldOn);

    public bool CanWrite => !this.m_closed && !this.DisposedValue && this.m_actor.Online && (byte)this.Status <= 1;

    public int Id { get; private set; }

    public string LastOperationMes { get; private set; }

    public DateTimeOffset LastOperationTime => this.m_lastOperationTime;

    public Metadata Metadata { get; private set; }

    public ChannelStatus Status { get; private set; }

    public string TargetId { get; }

    public bool Using => Volatile.Read(ref this.m_using) != 0;

    #region 读取

    public async Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (!this.CanRead)
        {
            throw new InvalidOperationException($"通道状态为{this.Status}，无法读取数据");
        }
        if (Interlocked.CompareExchange(ref this.m_reading, 1, 0) != 0)
        {
            throw new InvalidOperationException("同一通道不支持并发读取。");
        }
        ChannelPackage package = null;
        try
        {
            Interlocked.Exchange(ref this.m_readPackage, null).SafeDispose();
            package = await this.WaitAsync(cancellationToken).ConfigureDefaultAwait();
            lock (this.m_syncRoot)
            {
                if (this.m_closed)
                {
                    throw new ObjectDisposedException(nameof(InternalChannel));
                }
                if (package.DataType == ChannelDataType.Data)
                {
                    this.Status = ChannelStatus.Default;
                    this.LastOperationMes = string.Empty;
                    Interlocked.Exchange(ref this.m_readPackage, package).SafeDispose();
                    var memory = package.Data;
                    package = null;
                    return memory;
                }
            }
            switch (package.DataType)
            {
                case ChannelDataType.Completed:
                    this.Status = ChannelStatus.Completed;
                    break;
                case ChannelDataType.Canceled:
                    this.Status = ChannelStatus.Cancel;
                    break;
                case ChannelDataType.HoldOn:
                    this.Status = ChannelStatus.HoldOn;
                    break;
                default:
                    throw new InvalidOperationException("未知的通道数据类型");
            }
            this.LastOperationMes = package.Message;
            if ((byte)this.Status > 1)
            {
                this.Dispose();
            }
            return ReadOnlyMemory<byte>.Empty;
        }
        finally
        {
            package.SafeDispose();
            Volatile.Write(ref this.m_reading, 0);
        }
    }

    private void DisposeState()
    {
        lock (this.m_syncRoot)
        {
            this.m_closed = true;
            this.m_actor.RemoveChannel(this.Id, this);
            Interlocked.Exchange(ref this.m_readPackage, null).SafeDispose();
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
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureDefaultAwait();
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            if ((byte)this.Status > 1)
            {
                this.Dispose();
            }
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
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureDefaultAwait();
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            if ((byte)this.Status > 1)
            {
                this.Dispose();
            }
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
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureDefaultAwait();
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
            if ((byte)this.Status > 1)
            {
                this.Dispose();
            }
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    #endregion 操作

    public async Task WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (!this.m_actor.Online)
        {
            ThrowHelper.ThrowClientNotConnectedException();
        }
        if (!this.CanWrite)
        {
            throw new InvalidOperationException($"通道已{this.Status}");
        }
        var channelPackage = new ChannelPackage()
        {
            ChannelId = this.Id,
            DataType = ChannelDataType.Data,
            SourceId = this.m_actor.Id,
            TargetId = this.TargetId,
            Data = memory
        };
        await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureDefaultAwait();
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
    }
}