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

using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// 能够基于Dmtp协议，提供Rpc的功能
/// </summary>
public class DmtpRpcFeature : PluginBase, IDmtpFeature, IDmtpConnectingPlugin, IDmtpReceivedPlugin
{
    private readonly IRpcServerProvider m_rpcServerProvider;
    private readonly DmtpRpcOption m_option;

    /// <summary>
    /// 能够基于Dmtp协议，提供Rpc的功能
    /// </summary>
    /// <param name="resolver">服务解析器</param>
    /// <param name="option">配置选项</param>
    public DmtpRpcFeature(IServiceProvider resolver, DmtpRpcOption option)
    {
        this.m_option = ThrowHelper.ThrowArgumentNullExceptionIf(option, nameof(option));

        // 设置默认的创建DmtpRpcActor委托
        this.m_option.CreateDmtpRpcActor ??= PrivateCreateDmtpRpcActor;

        // 设置默认的调度器为并发调度器
        this.m_option.CreateDispatcher ??= (actor) => new ConcurrencyRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>();

        var rpcServerProvider = resolver.Resolve<IRpcServerProvider>();

        if (rpcServerProvider != null)
        {
            this.RegisterServer(rpcServerProvider.GetMethods());
            this.m_rpcServerProvider = rpcServerProvider;
        }
    }

    /// <summary>
    /// 方法映射表
    /// </summary>
    public ActionMap ActionMap { get; } = new ActionMap(false);

    /// <inheritdoc/>
    public ushort ReserveProtocolSize => 5;

    /// <inheritdoc/>
    public ushort StartProtocol => this.m_option.StartProtocol;

    /// <summary>
    /// 获取配置选项
    /// </summary>
    public DmtpRpcOption Option => this.m_option;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_option.m_globalQueueRpcDispatcher.Pin = false;
            this.m_option.m_globalQueueRpcDispatcher.Dispose();
        }
        base.Dispose(disposing);
    }

    private static DmtpRpcActor PrivateCreateDmtpRpcActor(IDmtpActor dmtpActor, IRpcServerProvider rpcServerProvider, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext> dispatcher)
    {
        return new DmtpRpcActor(dmtpActor, rpcServerProvider, dmtpActor.Client.Resolver, dispatcher);
    }

    private RpcMethod GetInvokeMethod(string name)
    {
        return this.ActionMap.GetRpcMethod(name);
    }

    private void RegisterServer(RpcMethod[] rpcMethods)
    {
        foreach (var rpcMethod in rpcMethods)
        {
            if (rpcMethod.GetAttribute<DmtpRpcAttribute>() is DmtpRpcAttribute attribute)
            {
                this.ActionMap.Add(attribute.GetInvokeKey(rpcMethod), rpcMethod);
            }
        }
    }

    #region Config

    /// <inheritdoc/>
    public async Task OnDmtpConnecting(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        var dmtpRpcActor = this.m_option.CreateDmtpRpcActor(client.DmtpActor, this.m_rpcServerProvider, this.m_option.CreateDispatcher.Invoke(client.DmtpActor));

        dmtpRpcActor.SerializationSelector = this.m_option.SerializationSelector;
        dmtpRpcActor.GetInvokeMethod = this.GetInvokeMethod;

        dmtpRpcActor.SetProtocolFlags(this.m_option.StartProtocol);
        client.DmtpActor.TryAddActor<DmtpRpcActor>(dmtpRpcActor);

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
    {
        var dmtpRpcActor = client.DmtpActor.GetActor<DmtpRpcActor>();
        if (dmtpRpcActor != null)
        {
            if (await dmtpRpcActor.InputReceivedData(e.DmtpMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                e.Handled = true;
                return;
            }
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion Config
}