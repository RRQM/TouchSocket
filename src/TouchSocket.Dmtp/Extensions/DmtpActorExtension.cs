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

namespace TouchSocket.Dmtp;

/// <summary>
/// 适用于<see cref="IDmtpActor"/>的扩展。
/// </summary>
public static class DmtpActorExtension
{
    #region Ping

    /// <inheritdoc cref="IDmtpActor.PingAsync(CancellationToken)"/>
    public static Task<Result> PingAsync(this IDmtpActorObject client, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.PingAsync(cancellationToken);
    }

    /// <inheritdoc cref="IDmtpActor.PingAsync(string,CancellationToken)"/>
    public static Task<Result> PingAsync(this IDmtpActorObject client, string targetId, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.PingAsync(targetId, cancellationToken);
    }

    #endregion Ping

    #region IDmtpChannel

    /// <inheritdoc cref="IDmtpActor.ChannelExisted(int)"/>
    public static bool ChannelExisted(this IDmtpActorObject client, int id)
    {
        return client.DmtpActor.ChannelExisted(id);
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(Metadata,CancellationToken)"/>
    [AsyncToSyncWarning]
    public static IDmtpChannel CreateChannel(this IDmtpActorObject client, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(metadata, cancellationToken).GetFalseAwaitResult();
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(int, Metadata,CancellationToken)"/>
    [AsyncToSyncWarning]
    public static IDmtpChannel CreateChannel(this IDmtpActorObject client, int id, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(id, metadata, cancellationToken).GetFalseAwaitResult();
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(string, int, Metadata,CancellationToken)"/>
    [AsyncToSyncWarning]
    public static IDmtpChannel CreateChannel(this IDmtpActorObject client, string targetId, int id, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(targetId, id, metadata, cancellationToken).GetFalseAwaitResult();
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(string, Metadata,CancellationToken)"/>
    [AsyncToSyncWarning]
    public static IDmtpChannel CreateChannel(this IDmtpActorObject client, string targetId, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(targetId, metadata, cancellationToken).GetFalseAwaitResult();
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(Metadata,CancellationToken)"/>
    public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(metadata, cancellationToken);
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(int, Metadata,CancellationToken)"/>
    public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, int id, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(id, metadata, cancellationToken);
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(string, int, Metadata,CancellationToken)"/>
    public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, string targetId, int id, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(targetId, id, metadata, cancellationToken);
    }

    /// <inheritdoc cref="IDmtpActor.CreateChannelAsync(string, Metadata,CancellationToken)"/>
    public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, string targetId, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.CreateChannelAsync(targetId, metadata, cancellationToken);
    }

    /// <inheritdoc cref="IDmtpActor.TrySubscribeChannel(int, out IDmtpChannel)"/>
    public static bool TrySubscribeChannel(this IDmtpActorObject client, int id, out IDmtpChannel channel)
    {
        return client.DmtpActor.TrySubscribeChannel(id, out channel);
    }

    #endregion IDmtpChannel

    #region 尝试异步发送

    /// <summary>
    /// 异步尝试发送数据。
    /// </summary>
    /// <param name="client">发送数据的客户端对象。</param>
    /// <param name="protocol">发送数据时使用的协议标识。</param>
    /// <param name="memory">待发送的数据，存储在只读内存中。</param>
    /// <returns>返回一个布尔任务，表示数据是否发送成功。</returns>
    public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol, ReadOnlyMemory<byte> memory)
    {
        try
        {
            // 调用客户端的SendAsync方法发送数据，不等待任务完成后再继续执行下面的代码。
            await client.DmtpActor.SendAsync(protocol, memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 异步尝试发送数据给指定的客户端。
    /// </summary>
    /// <param name="client">要发送数据的客户端对象。</param>
    /// <param name="protocol">要发送的数据协议类型。</param>
    /// <returns>返回一个布尔值，表示是否成功发送数据。</returns>
    public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol)
    {
        // 尝试发送数据，如果成功则返回<see langword="true"/>，失败则返回<see langword="false"/>。
        try
        {
            // 使用空的内存数据发送协议命令。
            await client.DmtpActor.SendAsync(protocol, ReadOnlyMemory<byte>.Empty).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return true;
        }
        catch
        {
            // 如果发送过程中发生异常，则返回<see langword="false"/>。
            return false;
        }
    }

    #endregion 尝试异步发送

    #region 发送Package


    /// <summary>
    /// 异步发送数据包。
    /// 此方法扩展了<see cref="IDmtpActorObject"/>接口，使其具有发送数据包的能力。
    /// </summary>
    /// <param name="client">要发送数据包的客户端对象。</param>
    /// <param name="protocol">发送数据包所使用的协议。</param>
    /// <param name="package">要发送的数据包实例。</param>
    /// <param name="maxSize">数据包的预估最大大小，用于指导<see cref="ByteBlock"/>内存的分配。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    public static async Task SendAsync(this IDmtpActorObject client, ushort protocol, IPackage package, int maxSize, CancellationToken cancellationToken = default)
    {
        // 使用ByteBlock管理内存，根据预估的最大大小来分配内存。
        using (var byteBlock = new ByteBlock(maxSize))
        {
            // 将ByteBlock对象赋予block变量，便于后续操作。
            var block = byteBlock;
            // 准备数据包，将数据写入到block中。
            package.Package(ref block);
            // 使用异步方法发送数据包和协议。
            await client.DmtpActor.SendAsync(protocol, byteBlock.Memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <summary>
    /// 异步发送估计大小小于64K的<see cref="IPackage"/>。
    /// 此方法重载允许指定自定义最大传输单元大小。
    /// </summary>
    /// <param name="client">要发送包的客户端对象。</param>
    /// <param name="protocol">发送包所使用的协议。</param>
    /// <param name="package">要发送的包实例。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个Task对象，表示异步操作的结果。</returns>
    public static Task SendAsync(this IDmtpActorObject client, ushort protocol, IPackage package, CancellationToken cancellationToken = default)
    {
        // 调用重载的SendAsync方法，使用默认的最大传输单元大小64K
        return SendAsync(client, protocol, package, 1024 * 64, cancellationToken);
    }

    #endregion 发送Package

    #region 尝试发送Package

    /// <summary>
    /// 异步尝试发送数据包。
    /// 此方法通过指定的协议将数据包发送到客户端。
    /// 它使用估计的最大数据包大小来优化内存申请。
    /// </summary>
    /// <param name="client">要发送数据包的客户端对象。</param>
    /// <param name="protocol">发送数据包所使用的协议标识符。</param>
    /// <param name="package">要发送的数据包实例。</param>
    /// <param name="maxSize">估计的数据包最大大小，用于优化内存分配。</param>
    /// <returns>返回一个布尔值，表示发送操作是否成功。</returns>
    public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol, IPackage package, int maxSize)
    {
        try
        {
            // 实际执行发送操作。
            await SendAsync(client, protocol, package, maxSize).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 发送成功，返回<see langword="true"/>。
            return true;
        }
        catch
        {
            // 发送失败，返回<see langword="false"/>。
            return false;
        }
    }

    /// <summary>
    /// 异步尝试发送一个估计大小小于64K的<see cref="IPackage"/>。
    /// </summary>
    /// <param name="client">要发送包的客户端。</param>
    /// <param name="protocol">使用的协议。</param>
    /// <param name="package">要发送的包。</param>
    /// <returns>如果发送成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol, IPackage package)
    {
        // 尝试发送包，如果成功则返回<see langword="true"/>，失败则返回<see langword="false"/>
        try
        {
            // 实际执行发送操作，64KB是根据业务需求预设的限制
            await SendAsync(client, protocol, package, 1024 * 64).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion 尝试发送Package

    #region 发送

    /// <summary>
    /// 异步发送数据
    /// </summary>
    /// <param name="client">客户端对象，实现IDmtpActorObject接口</param>
    /// <param name="protocol">协议标识符</param>
    /// <param name="memory">待发送的数据，以只读内存块形式提供</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个Task对象，标识异步操作</returns>
    public static Task SendAsync(this IDmtpActorObject client, ushort protocol, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.SendAsync(protocol, memory, cancellationToken);
    }

    /// <summary>
    /// 异步发送空数据
    /// </summary>
    /// <param name="client">客户端对象，实现IDmtpActorObject接口</param>
    /// <param name="protocol">协议标识符</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个Task对象，标识异步操作</returns>
    public static Task SendAsync(this IDmtpActorObject client, ushort protocol, CancellationToken cancellationToken = default)
    {
        return client.DmtpActor.SendAsync(protocol, ReadOnlyMemory<byte>.Empty, cancellationToken);
    }

    #endregion 发送

    /// <summary>
    /// 将频道状态转换为结果代码
    /// </summary>
    /// <param name="channelStatus">当前频道状态</param>
    /// <returns>对应的ResultCode枚举值</returns>
    public static ResultCode ToResultCode(this ChannelStatus channelStatus)
    {
        // 根据频道状态channelStatus的值，转换为相应的ResultCode
        return channelStatus switch
        {
            ChannelStatus.Default => ResultCode.Default,
            ChannelStatus.HoldOn => ResultCode.Default,
            ChannelStatus.Cancel => ResultCode.Canceled,
            ChannelStatus.Completed => ResultCode.Success,
            _ => ResultCode.Error,
        };
    }
}