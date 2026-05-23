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

using System.Net;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 服务端，基于 UDP 协议接收和处理 CoAP（RFC 7252）消息。
/// </summary>
public class CoAPServer : UdpSessionBase, ICoAPSession
{
    /// <summary>
    /// 初始化 <see cref="CoAPServer"/> 类的新实例。
    /// </summary>
    public CoAPServer()
    {
        this.Protocol = CoAPUtility.CoAPUdp;
    }

    /// <inheritdoc/>
    public async Task SendCoAPMessageAsync(EndPoint remoteEndPoint, CoAPMessage message, CancellationToken cancellationToken = default)
    {
        await this.ProtectedSendAsync(remoteEndPoint, message, cancellationToken).ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        this.SetAdapter(new CoAPUdpAdapter());
        base.LoadConfig(config);
    }

    /// <inheritdoc/>
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        if (e.RequestInfo is CoAPMessage message)
        {
            await this.OnCoAPReceived(new CoAPMessageReceivedEventArgs(e.EndPoint, message)).ConfigureDefaultAwait();
        }

        await base.OnUdpReceived(e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 触发 CoAP 消息接收插件事件。
    /// </summary>
    /// <param name="e">包含消息信息的事件参数。</param>
    protected virtual async Task OnCoAPReceived(CoAPMessageReceivedEventArgs e)
    {
        await this.PluginManager.RaiseICoAPReceivedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }
}
