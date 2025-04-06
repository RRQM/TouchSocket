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

using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi;

/// <summary>
/// WebApi客户端
/// </summary>
public class WebApiClient : HttpClientBase, IWebApiClient
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public WebApiClient()
    {
        this.Converter = new StringSerializerConverter<HttpRequest>(
            new StringToPrimitiveSerializerFormatter<HttpRequest>(),
            new JsonStringToClassSerializerFormatter<HttpRequest>());
    }

    /// <summary>
    /// 字符串转化器
    /// </summary>
    public StringSerializerConverter<HttpRequest> Converter { get; }

    /// <inheritdoc/>
    public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        return this.TcpConnectAsync(millisecondsTimeout, token);
    }

    #region Rpc调用

    /// <inheritdoc/>
    public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
    {
        if (parameters.Length != 1 || parameters[0] is not WebApiRequest webApiRequest)
        {
            throw new Exception("参数不正确");
        }

        var request = new HttpRequest();
        request.URL = (invokeKey);
        switch (webApiRequest.Method)
        {
            case HttpMethodType.Get:
                request.Method = HttpMethod.Get;
                break;
            case HttpMethodType.Post:
                request.Method = HttpMethod.Post;
                break;
            case HttpMethodType.Put:
                request.Method = HttpMethod.Put;
                break;
            case HttpMethodType.Delete:
                request.Method = HttpMethod.Delete;
                break;
            default:
                break;
        }
        request.InitHeaders();
        if (webApiRequest.Headers != null)
        {
            foreach (var item in webApiRequest.Headers)
            {
                request.Headers.Add(item.Key, item.Value);
            }
        }
        if (webApiRequest.Querys != null)
        {
            foreach (var item in webApiRequest.Querys)
            {
                request.Query.Add(item.Key, item.Value);
            }
        }
        request.SetHost(this.RemoteIPHost.Host);

        if (webApiRequest.Body != null)
        {
            request.SetContent(this.Converter.Serialize(request, webApiRequest.Body));
        }
        else if (webApiRequest.Forms != null)
        {
            request.SetFormUrlEncodedContent(webApiRequest.Forms);
        }

        invokeOption ??= InvokeOption.WaitInvoke;

        await this.PluginManager.RaiseAsync(typeof(IWebApiRequestPlugin), this.Resolver, this, new WebApiEventArgs(request, default));

        using (var responseResult = await this.ProtectedRequestContentAsync(request, invokeOption.Timeout, invokeOption.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            var response = responseResult.Response;
            await this.PluginManager.RaiseAsync(typeof(IWebApiResponsePlugin), this.Resolver, this, new WebApiEventArgs(request, response));

            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return default;
            }

            if (response.StatusCode == 200)
            {
                if (returnType != null)
                {
                    var body = await response.GetBodyAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    return this.Converter.Deserialize(request, body, returnType);
                }
                else
                {
                    return default;
                }

            }
            else if (response.StatusCode == 422)
            {
                throw new RpcException(((ActionResult)this.Converter.Deserialize(request, await response.GetBodyAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext), typeof(ActionResult))).Message);
            }
            else
            {
                throw new RpcException(response.StatusMessage);
            }
        }
    }
    #endregion Rpc调用
}