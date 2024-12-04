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

#if NETSTANDARD2_0_OR_GREATER || NET481_OR_GREATER || NET6_0_OR_GREATER
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using HttpClient = System.Net.Http.HttpClient;
using HttpMethod = System.Net.Http.HttpMethod;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 使用<see cref="HttpClient"/>为基础的WebApi客户端。
    /// </summary>
    public class WebApiClientSlim : Http.HttpClientSlim, IWebApiClientBase
    {
        /// <summary>
        /// 使用<see cref="HttpClient"/>为基础的WebApi客户端。
        /// </summary>
        /// <param name="httpClient"></param>
        public WebApiClientSlim(HttpClient httpClient = default) : base(httpClient)
        {
            this.Converter = new StringSerializerConverter<HttpRequestMessage>(
                new StringToPrimitiveSerializerFormatter<HttpRequestMessage>(),
                new JsonStringToClassSerializerFormatter<HttpRequestMessage>());
        }

        /// <summary>
        /// 字符串转化器
        /// </summary>
        public StringSerializerConverter<HttpRequestMessage> Converter { get; }

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            if (parameters.Length != 1 || parameters[0] is not WebApiRequest webApiRequest)
            {
                throw new Exception("参数不正确");
            }

            var request = new HttpRequestMessage();
            
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

            if (webApiRequest.Headers != null)
            {
                foreach (var item in webApiRequest.Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            if (webApiRequest.Querys != null)
            {
                invokeKey= invokeKey +"?"+ string.Join("&", webApiRequest.Querys.Select(a => $"{a.Key}={a.Value}"));
            }

            request.RequestUri = new Uri(this.HttpClient.BaseAddress, invokeKey);

            if (webApiRequest.Body != null)
            {
                var body = this.Converter.Serialize(request, webApiRequest.Body);
                request.Content = new StringContent(body);
            }
            else if (webApiRequest.Forms!=null)
            {
                var content = new FormUrlEncodedContent(webApiRequest.Forms);
                request.Content = content;
            }
           
            invokeOption ??= InvokeOption.WaitInvoke;

            await this.PluginManager.RaiseAsync(typeof(IWebApiRequestPlugin), this.Resolver, this, new WebApiEventArgs(request, default)).ConfigureAwait(false);

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = await this.HttpClient.SendAsync(request, tokenSource.Token).ConfigureAwait(false);

                await this.PluginManager.RaiseAsync(typeof(IWebApiRequestPlugin), this.Resolver, this, new WebApiEventArgs(request, response)).ConfigureAwait(false);

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return default;
                }

                if (response.IsSuccessStatusCode)
                {
                    if (returnType != null)
                    {
                        return this.Converter.Deserialize(null, await response.Content.ReadAsStringAsync().ConfigureAwait(false), returnType);
                    }
                    return default;
                }
                else if ((int)response.StatusCode == 422)
                {
                    throw new RpcException(((ActionResult)this.Converter.Deserialize(null, await response.Content.ReadAsStringAsync().ConfigureAwait(false), typeof(ActionResult))).Message);
                }
                else
                {
                    throw new RpcException(response.ReasonPhrase);
                }
            }
        }
    }
}

#endif