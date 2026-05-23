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
/// CoAP 客户端，基于 UDP 协议实现 CoAP（RFC 7252）消息的发送与接收。
/// 支持 CON（Confirmable）消息的请求-响应匹配。
/// </summary>
public class CoAPClient : UdpSessionBase, ICoAPSession
{
    private readonly WaitHandlePool<CoAPResponse> m_waitHandlePool;

    /// <summary>
    /// 初始化 <see cref="CoAPClient"/> 类的新实例。
    /// </summary>
    public CoAPClient()
    {
        this.Protocol = CoAPUtility.CoAPUdp;
        this.m_waitHandlePool = new WaitHandlePool<CoAPResponse>(0, ushort.MaxValue);
    }

    /// <inheritdoc/>
    public async Task SendCoAPMessageAsync(EndPoint remoteEndPoint, CoAPMessage message, CancellationToken cancellationToken = default)
    {
        await this.ProtectedSendAsync(remoteEndPoint, message, cancellationToken).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 向指定端点发送 CoAP CON 请求，并等待 ACK 响应。
    /// </summary>
    /// <param name="remoteEndPoint">目标端点。</param>
    /// <param name="request">要发送的 CoAP 请求（Type 应为 <see cref="CoAPMessageType.CON"/>）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>收到的 CoAP 响应。</returns>
    public async Task<CoAPResponse> SendCoAPRequestAsync(EndPoint remoteEndPoint, CoAPRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Type != CoAPMessageType.CON)
        {
            await this.ProtectedSendAsync(remoteEndPoint, request, cancellationToken).ConfigureDefaultAwait();
            return null;
        }

        var waitData = this.m_waitHandlePool.GetWaitDataAsync(out var sign);
        try
        {
            request.MessageId = (ushort)sign;
            await this.ProtectedSendAsync(remoteEndPoint, request, cancellationToken).ConfigureDefaultAwait();

            var status = await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait();
            status.ThrowIfNotRunning();

            var response = waitData.CompletedData;
            CoAPThrowHelper.ThrowIfErrorResponse(response.ResponseCode);
            return response;
        }
        finally
        {
            waitData.Dispose();
        }
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
        if (e.RequestInfo is CoAPResponse response)
        {
            if (response.Type == CoAPMessageType.ACK || response.Type == CoAPMessageType.RST)
            {
                this.m_waitHandlePool.Set(response);
            }
            else
            {
                await this.OnCoAPReceived(new CoAPMessageReceivedEventArgs(e.EndPoint, response)).ConfigureDefaultAwait();
            }
        }
        else if (e.RequestInfo is CoAPRequest request)
        {
            await this.OnCoAPReceived(new CoAPMessageReceivedEventArgs(e.EndPoint, request)).ConfigureDefaultAwait();
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
