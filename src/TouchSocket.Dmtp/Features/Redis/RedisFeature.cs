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

namespace TouchSocket.Dmtp.Redis;

/// <summary>
/// RedisFeature
/// </summary>
public class RedisFeature : PluginBase, IDmtpConnectingPlugin, IDmtpReceivedPlugin, IDmtpFeature
{
    private readonly DmtpRedisOption m_option;

    /// <summary>
    /// RedisFeature
    /// </summary>
    /// <param name="option">配置选项</param>
    public RedisFeature(DmtpRedisOption option)
    {
        this.m_option = ThrowHelper.ThrowArgumentNullExceptionIf(option, nameof(option));
        
        // 设置默认的JSON转换器
        if (this.m_option.Converter != null)
        {
            this.m_option.Converter.Add(new JsonBytesToClassSerializerFormatter<object>());
        }
    }

    /// <inheritdoc/>
    public ushort ReserveProtocolSize => 5;

    /// <inheritdoc/>
    public ushort StartProtocol => this.m_option.StartProtocol;

    /// <summary>
    /// 获取配置选项
    /// </summary>
    public DmtpRedisOption Option => this.m_option;

    /// <inheritdoc/>
    public async Task OnDmtpConnecting(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        var dmtpRedisActor = new DmtpRedisActor(client.DmtpActor)
        {
            ICache = this.m_option.Cache,
            Converter = this.m_option.Converter
        };

        dmtpRedisActor.SetProtocolFlags(this.m_option.StartProtocol);
        client.DmtpActor.TryAddActor<DmtpRedisActor>(dmtpRedisActor);

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
    {
        if (client.DmtpActor.GetDmtpRedisActor() is DmtpRedisActor redisClient)
        {
            if (await redisClient.InputReceivedData(e.DmtpMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                e.Handled = true;
                return;
            }
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}