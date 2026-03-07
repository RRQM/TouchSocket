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
    private DateTimeOffset m_lastOperationTime;
    private bool m_using;

    internal void SetId(int id)
    {
        this.Id = id;
    }

    internal void MakeUsing()
    {
        this.m_using = true;
    }

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

    public async Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!this.CanRead)
        {
            throw new InvalidOperationException($"通道状态为{this.Status}，无法读取数据");
        }

        if (this.DisposedValue)
        {
            throw new ObjectDisposedException(nameof(InternalChannel), "通道已被释放");
        }

        var channelPackage = await this.WaitAsync(cancellationToken).ConfigureDefaultAwait();

        try
        {
            switch (channelPackage.DataType)
            {
                case ChannelDataType.Data:
                    this.Status = ChannelStatus.Default;
                    this.LastOperationMes = string.Empty;
                    return channelPackage.Data;

                case ChannelDataType.Completed:
                    this.Status = ChannelStatus.Completed;
                    this.LastOperationMes = channelPackage.Message;
                    return ReadOnlyMemory<byte>.Empty;

                case ChannelDataType.Canceled:
                    this.Status = ChannelStatus.Cancel;
                    this.LastOperationMes = channelPackage.Message;
                    return ReadOnlyMemory<byte>.Empty;

                case ChannelDataType.HoldOn:
                    this.Status = ChannelStatus.HoldOn;
                    this.LastOperationMes = channelPackage.Message;
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
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureDefaultAwait();
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
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureDefaultAwait();
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
            await this.m_actor.SendChannelPackageAsync(channelPackage, cancellationToken).ConfigureDefaultAwait();
            this.m_lastOperationTime = DateTimeOffset.UtcNow;
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
        if ((byte)this.Status > 1)
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