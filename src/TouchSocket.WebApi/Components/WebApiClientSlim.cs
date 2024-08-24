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
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 使用<see cref="HttpClient"/>为基础的WebApi客户端。
    /// </summary>
    public class WebApiClientSlim : Http.HttpClientSlim, IWebApiClientBase
    {
        private readonly object[] m_empty = new object[0];
        /// <summary>
        /// 使用<see cref="HttpClient"/>为基础的WebApi客户端。
        /// </summary>
        /// <param name="httpClient"></param>
        public WebApiClientSlim(HttpClient httpClient = default) : base(httpClient)
        {
            this.Converter = new StringSerializerConverter(new JsonStringToClassSerializerFormatter<object>());
        }

        /// <summary>
        /// 字符串转化器
        /// </summary>
        public StringSerializerConverter Converter { get; }


        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            var strs = invokeKey.Split(':');
            if (strs.Length != 2)
            {
                throw new RpcException("不是有效的url请求。");
            }

            invokeOption ??= InvokeOption.WaitInvoke;
            parameters ??= m_empty;

            using var request = new HttpRequestMessage();

            switch (strs[0])
            {
                case "GET":
                    {
                        request.RequestUri = new Uri(this.HttpClient.BaseAddress, strs[1].Format(parameters));
                        request.Method = HttpMethod.Get;
                        break;
                    }
                case "POST":
                    {
                        request.RequestUri = new Uri(this.HttpClient.BaseAddress, strs[1].Format(parameters));
                        request.Method = HttpMethod.Post;

                        if (parameters.Length > 0)
                        {
                            request.Content = new ByteArrayContent(JsonConvert.SerializeObject(parameters[parameters.Length - 1]).ToUTF8Bytes());
                        }
                        break;
                    }
                default:
                    break;
            }

            await this.PluginManager.RaiseAsync(typeof(IWebApiRequestPlugin), this, new WebApiEventArgs(request, default)).ConfigureAwait(false);

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = await this.HttpClient.SendAsync(request, tokenSource.Token).ConfigureAwait(false);

                await this.PluginManager.RaiseAsync(typeof(IWebApiRequestPlugin), this, new WebApiEventArgs(request, response)).ConfigureAwait(false);

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