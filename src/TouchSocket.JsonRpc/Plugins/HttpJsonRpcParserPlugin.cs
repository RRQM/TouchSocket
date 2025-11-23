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
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc;

/// <summary>
/// HttpJsonRpcParserPlugin
/// </summary>
[PluginOption(Singleton = true)]
public sealed class HttpJsonRpcParserPlugin : JsonRpcParserPluginBase, IHttpPlugin
{
    private readonly Func<IHttpSessionClient,HttpContext, Task<bool>> m_allowJsonRpc;

    /// <summary>
    /// 构造函数，用于初始化 <see cref="HttpJsonRpcParserPlugin"/> 类的新实例。
    /// </summary>
    /// <param name="rpcServerProvider">IRpcServerProvider 类型的参数，提供 RPC 服务器服务。</param>
    /// <param name="option">Http JsonRpc配置选项</param>
    /// <remarks>
    /// 该构造函数调用基类的构造函数，传递 <paramref name="rpcServerProvider"/>和<paramref name="option"/>参数。
    /// 这对于确保基类能够访问 RPC 服务器提供者和依赖项解析器至关重要。
    /// </remarks>
    public HttpJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider, HttpJsonRpcOption option) 
        : base(rpcServerProvider, option)
    {
        this.m_allowJsonRpc = option.AllowJsonRpc;
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (e.Context.Request.Method == HttpMethod.Post)
        {
           var allowJsonRpc = await this.m_allowJsonRpc.Invoke(client,e.Context);
            if (allowJsonRpc)
            {
                e.Handled = true;

                if (!client.TryGetValue(JsonRpcClientExtension.JsonRpcActorProperty, out var jsonRpcActor))
                {
                    jsonRpcActor = new JsonRpcActor()
                    {
                        Resolver = client.Resolver,
                        SendAction = async (data, cancellationToken) =>
                        {
                            var response = e.Context.Response;
                            response.SetContent(data);
                            response.SetStatusWithSuccess();
                            await response.AnswerAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        },
                        SerializerConverter = this.SerializerConverter,
                        RpcDispatcher = new ImmediateRpcDispatcher<JsonRpcActor, IJsonRpcCallContext>()
                    };

                    jsonRpcActor.SetRpcServerProvider(this.RpcServerProvider, this.ActionMap);
                    client.SetValue(JsonRpcClientExtension.JsonRpcActorProperty, jsonRpcActor);
                }

                await jsonRpcActor.InputReceiveAsync(await e.Context.Request.GetContentAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext), new HttpJsonRpcCallContext(client, e.Context)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}