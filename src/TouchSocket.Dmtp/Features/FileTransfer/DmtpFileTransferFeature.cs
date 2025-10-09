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

namespace TouchSocket.Dmtp.FileTransfer;

/// <summary>
/// 能够基于Dmtp协议，提供文件传输的能力
/// </summary>
public sealed class DmtpFileTransferFeature : PluginBase, IDmtpConnectingPlugin, IDmtpReceivedPlugin, IDmtpFeature
{
    private readonly DmtpFileTransferOption m_option;
    private IPluginManager m_pluginManager;

    /// <summary>
    /// 能够基于Dmtp协议，提供文件传输的能力
    /// </summary>
    /// <param name="resolver">服务解析器</param>
    /// <param name="option">配置选项</param>
    public DmtpFileTransferFeature(IResolver resolver, DmtpFileTransferOption option)
    {
        this.m_option = ThrowHelper.ThrowArgumentNullExceptionIf(option, nameof(option));
        this.m_option.FileResourceController ??= resolver.Resolve<IFileResourceController>() ?? FileResourceController.Default;
    }

    /// <inheritdoc/>
    protected override void Loaded(IPluginManager pluginManager)
    {
        base.Loaded(pluginManager);
        this.m_pluginManager = pluginManager;
    }

    /// <inheritdoc/>
    public ushort ReserveProtocolSize => 20;

    /// <inheritdoc/>
    public ushort StartProtocol => this.m_option.StartProtocol;

    /// <summary>
    /// 获取配置选项
    /// </summary>
    public DmtpFileTransferOption Option => this.m_option;

    /// <inheritdoc/>
    public async Task OnDmtpConnecting(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        var dmtpFileTransferActor = new DmtpFileTransferActor(client.DmtpActor, this.m_option.FileResourceController)
        {
            OnFileTransferring = this.OnFileTransfering,
            OnFileTransferred = this.OnFileTransfered,
            RootPath = this.m_option.RootPath,
            MaxSmallFileLength = this.m_option.MaxSmallFileLength
        };
        dmtpFileTransferActor.SetProtocolFlags(this.m_option.StartProtocol);
        client.DmtpActor.TryAddActor<DmtpFileTransferActor>(dmtpFileTransferActor);
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
    {
        if (client.DmtpActor.GetDmtpFileTransferActor() is DmtpFileTransferActor dmtpFileTransferActor)
        {
            if (await dmtpFileTransferActor.InputReceivedData(e.DmtpMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                e.Handled = true;
                return;
            }
        }

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task OnFileTransfered(IDmtpActor actor, FileTransferredEventArgs e)
    {
        await this.m_pluginManager.RaiseAsync(typeof(IDmtpFileTransferredPlugin), actor.Client.Resolver, actor.Client, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task OnFileTransfering(IDmtpActor actor, FileTransferringEventArgs e)
    {
        await this.m_pluginManager.RaiseAsync(typeof(IDmtpFileTransferringPlugin), actor.Client.Resolver, actor.Client, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}