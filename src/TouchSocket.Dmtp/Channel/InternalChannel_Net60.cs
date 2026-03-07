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

#if NET6_0_OR_GREATER
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TouchSocket.Dmtp;

internal sealed partial class InternalChannel : SafetyDisposableObject, IDmtpChannel
{
    private readonly Channel<ChannelPackage> m_channel;

    public InternalChannel(DmtpActor client, string targetId, Metadata metadata)
    {
        this.m_actor = client;
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
        this.TargetId = targetId;
        this.Status = ChannelStatus.Default;
        this.m_channel = Channel.CreateBounded<ChannelPackage>(new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
        this.Metadata = metadata;
    }

    internal async ValueTask ReceivedDataAsync(ChannelPackage channelPackage)
    {
        this.m_lastOperationTime = DateTimeOffset.UtcNow;
        try
        {
            await this.m_channel.Writer.WriteAsync(channelPackage).ConfigureDefaultAwait();
        }
        catch
        {
            channelPackage.SafeDispose();
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_channel.Writer.TryComplete();
            while (this.m_channel.Reader.TryRead(out var pkg))
            {
                pkg.SafeDispose();
            }
        }
    }

    private async Task<ChannelPackage> WaitAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await this.m_channel.Reader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
        }
        catch (ChannelClosedException)
        {
            throw new ObjectDisposedException(nameof(InternalChannel), "通道已被释放");
        }
    }
}
#endif
