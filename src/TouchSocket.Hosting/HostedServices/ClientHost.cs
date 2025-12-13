// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Hosting.HostedServices;

internal class ClientHost<TService> : SetupConfigObjectHostedService<TService> where TService : ISetupConfigObject, IConnectableClient,IClosableClient
{
    private ILogger<TService> m_logger;

    protected override void OnSetResolver(IResolver resolver)
    {
        base.OnSetResolver(resolver);
        this.m_logger = resolver.GetService<ILogger<TService>>();
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.StartAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            await this.ConfigObject.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            this.m_logger.LogInformation("{Message}", TouchSocketHostingResource.HostServerStarted);
        }
        catch (Exception ex)
        {
            this.m_logger.LogError(ex, "{Message}", ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.ConfigObject.CloseAsync("服务停止",cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await base.StopAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}