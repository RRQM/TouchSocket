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
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 基于Tcp协议的JsonRpc功能插件
/// </summary>
[PluginOption(Singleton = true)]
public sealed class TcpJsonRpcParserPlugin : JsonRpcParserPluginBase, ITcpConnectedPlugin, ITcpReceivedPlugin
{
    private readonly TcpJsonRpcOption m_option;

    /// <summary>
    /// 基于Tcp协议的JsonRpc功能插件
    /// </summary>
    /// <param name="rpcServerProvider"></param>
    /// <param name="option">配置选项</param>
    public TcpJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider, TcpJsonRpcOption option) : base(rpcServerProvider, option)
    {
        this.m_option = option;
    }

    /// <inheritdoc/>
    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        if (client is IClientSender clientSender)
        {
            if (this.m_option.AllowJsonRpc != null)
            {
                if (await this.m_option.AllowJsonRpc.Invoke(client)
                    .ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    var jsonRpcActor = new JsonRpcActor()
                    {
                        SerializerConverter = this.SerializerConverter,
                        Resolver = client.Resolver,
                        SendAction = clientSender.SendAsync,
                        Logger = client.Logger
                    };

                    jsonRpcActor.SetRpcServerProvider(this.RpcServerProvider, this.ActionMap);
                    client.SetValue(JsonRpcClientExtension.JsonRpcActorProperty, jsonRpcActor);
                }
            }
        }

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        if (client.TryGetValue(JsonRpcClientExtension.JsonRpcActorProperty, out var jsonRpcActor))
        {
            var jsonRpcMemory = ReadOnlyMemory<byte>.Empty;
            if (e.RequestInfo is IJsonRpcRequestInfo requestInfo)
            {
                jsonRpcMemory = requestInfo.GetJsonRpcMemory();
            }
            else if (e.RequestInfo is JsonPackage jsonPackage)
            {
                jsonRpcMemory = jsonPackage.Data;
            }
            else if (!e.Memory.IsEmpty)
            {
                jsonRpcMemory = e.Memory;
            }

            if (jsonRpcMemory.IsEmpty)
            {
                return;
            }
            await jsonRpcActor.InputReceiveAsync(jsonRpcMemory, new TcpJsonRpcCallContext(client)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}