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

using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer;

/// <summary>
/// 能够基于Dmtp协议，提供文件传输的能力
/// </summary>
public sealed class DmtpFileTransferFeature : PluginBase, IDmtpHandshakingPlugin, IDmtpReceivedPlugin, IDmtpFeature
{
    private readonly IFileResourceController m_fileResourceController;
    private IPluginManager m_pluginManager;

    /// <summary>
    /// 能够基于Dmtp协议，提供文件传输的能力
    /// </summary>
    /// <param name="resolver"></param>
    public DmtpFileTransferFeature(IResolver resolver)
    {
        this.m_fileResourceController = resolver.Resolve<IFileResourceController>() ?? FileResourceController.Default;
        this.MaxSmallFileLength = 1024 * 1024;
        this.SetProtocolFlags(30);
    }

    /// <inheritdoc/>
    protected override void Loaded(IPluginManager pluginManager)
    {
        base.Loaded(pluginManager);
        this.m_pluginManager = pluginManager;
    }

    /// <inheritdoc cref="IDmtpFileTransferActor.MaxSmallFileLength"/>
    public int MaxSmallFileLength { get; set; }

    /// <inheritdoc/>
    public ushort ReserveProtocolSize => 20;

    /// <inheritdoc cref="IDmtpFileTransferActor.RootPath"/>
    public string RootPath { get; set; }

    /// <inheritdoc/>
    public ushort StartProtocol { get; set; }

    /// <inheritdoc/>
    public async Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        var dmtpFileTransferActor = new DmtpFileTransferActor(client.DmtpActor, this.m_fileResourceController)
        {
            OnFileTransferring = this.OnFileTransfering,
            OnFileTransferred = this.OnFileTransfered,
            RootPath = this.RootPath,
            MaxSmallFileLength = this.MaxSmallFileLength
        };
        dmtpFileTransferActor.SetProtocolFlags(this.StartProtocol);
        client.DmtpActor.AddActor<DmtpFileTransferActor>(dmtpFileTransferActor);
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

    /// <inheritdoc cref="IDmtpFileTransferActor.MaxSmallFileLength"/>
    public DmtpFileTransferFeature SetMaxSmallFileLength(int maxSmallFileLength)
    {
        this.MaxSmallFileLength = maxSmallFileLength;
        return this;
    }

    /// <summary>
    /// 设置<see cref="DmtpFileTransferFeature"/>的起始协议。
    /// <para>
    /// 默认起始为：30，保留20个协议长度。
    /// </para>
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public DmtpFileTransferFeature SetProtocolFlags(ushort start)
    {
        this.StartProtocol = start;
        return this;
    }

    /// <inheritdoc cref="IDmtpFileTransferActor.RootPath"/>
    public DmtpFileTransferFeature SetRootPath(string rootPath)
    {
        this.RootPath = rootPath;
        return this;
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