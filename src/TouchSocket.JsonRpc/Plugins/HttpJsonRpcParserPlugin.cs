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
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc;

/// <summary>
/// HttpJsonRpcParserPlugin
/// </summary>
[PluginOption(Singleton = true)]
public sealed class HttpJsonRpcParserPlugin : JsonRpcParserPluginBase, IHttpPlugin
{
    private string m_jsonRpcUrl = "/jsonrpc";

    /// <summary>
    /// 构造函数，用于初始化 <see cref="HttpJsonRpcParserPlugin"/> 类的新实例。
    /// </summary>
    /// <param name="rpcServerProvider">IRpcServerProvider 类型的参数，提供 RPC 服务器服务。</param>
    /// <remarks>
    /// 该构造函数调用基类的构造函数，传递 <paramref name="rpcServerProvider"/>参数。
    /// 这对于确保基类能够访问 RPC 服务器提供者和依赖项解析器至关重要。
    /// </remarks>
    public HttpJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider) : base(rpcServerProvider)
    {
    }

    /// <summary>
    /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为<see langword="null"/>或空时，会全部响应。
    /// </summary>
    public string JsonRpcUrl
    {
        get => this.m_jsonRpcUrl;
        set => this.m_jsonRpcUrl = string.IsNullOrEmpty(value) ? "/" : value;
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        if (e.Context.Request.Method == HttpMethod.Post)
        {
            if (this.m_jsonRpcUrl == "/" || e.Context.Request.UrlEquals(this.m_jsonRpcUrl))
            {
                e.Handled = true;

                if (!client.TryGetValue(JsonRpcClientExtension.JsonRpcActorProperty, out var jsonRpcActor))
                {
                    jsonRpcActor = new JsonRpcActor()
                    {
                        Resolver = client.Resolver,
                        SendAction = async (data, token) =>
                        {
                            var response = e.Context.Response;
                            response.SetContent(data);
                            response.SetStatusWithSuccess();
                            await response.AnswerAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    /// <summary>
    /// 设置JSON-RPC URL的匹配规则。
    /// 当挂载在<see cref="HttpService"/>时，根据指定的URL进行匹配并响应请求。
    /// 如果设置为<see langword="null"/>或空，将对所有请求进行响应。
    /// </summary>
    /// <param name="jsonRpcUrl">要匹配的JSON-RPC URL。</param>
    /// <returns>返回当前的<see cref="HttpJsonRpcParserPlugin"/>实例，支持链式调用。</returns>
    public HttpJsonRpcParserPlugin SetJsonRpcUrl(string jsonRpcUrl)
    {
        this.JsonRpcUrl = jsonRpcUrl;
        return this;
    }
}