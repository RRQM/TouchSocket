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

using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc;

/// <summary>
/// WebSocketJsonRpcParserPlugin
/// </summary>
[PluginOption(Singleton = true)]
public sealed class WebSocketJsonRpcParserPlugin : JsonRpcParserPluginBase, IWebSocketConnectedPlugin, IWebSocketReceivedPlugin
{
    private readonly WebSocketJsonRpcOption m_option;

    /// <summary>
    /// WebSocketJsonRpcParserPlugin
    /// </summary>
    /// <param name="rpcServerProvider"></param>
    /// <param name="option">WebSocket JsonRpc配置选项</param>
    public WebSocketJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider, WebSocketJsonRpcOption option) : base(rpcServerProvider, option)
    {
        this.m_option = option;
    }

    /// <inheritdoc/>
    public async Task OnWebSocketConnected(IWebSocket client, HttpContextEventArgs e)
    {
        if (this.m_option.AllowJsonRpc != null)
        {
            if (await this.m_option.AllowJsonRpc.Invoke(client, e.Context).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                var jsonRpcActor = new JsonRpcActor()
                {
                    SerializerConverter = this.SerializerConverter,
                    Resolver = client.Client.Resolver,
                    SendAction = (data, cancellationToken) => client.SendAsync(data, WSDataType.Text, true, cancellationToken)
                };

                jsonRpcActor.SetRpcServerProvider(this.RpcServerProvider, this.ActionMap);
                client.Client.SetValue(JsonRpcClientExtension.JsonRpcActorProperty, jsonRpcActor);
            }
        }

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    public async Task OnWebSocketReceived(IWebSocket client, WSDataFrameEventArgs e)
    {
        var dataFrame = e.DataFrame;
        if (dataFrame.Opcode == WSDataType.Text && client.Client.TryGetValue(JsonRpcClientExtension.JsonRpcActorProperty, out var jsonRpcActor))
        {
            e.Handled = true;

            await jsonRpcActor.InputReceiveAsync(dataFrame.PayloadData, new WebSocketJsonRpcCallContext(client.Client)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }
}