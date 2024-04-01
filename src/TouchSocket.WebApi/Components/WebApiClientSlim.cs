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

        ///<inheritdoc/>
        public object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var strs = invokeKey.Split(':');
            if (strs.Length != 2)
            {
                throw new RpcException("不是有效的url请求。");
            }
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            var request = new HttpRequestMessage();

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
                            request.Content = new ByteArrayContent(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]).ToUTF8Bytes());
                        }
                        break;
                    }
                default:
                    break;
            }

            this.PluginManager.Raise(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = this.HttpClient.SendAsync(request, tokenSource.Token).GetAwaiter().GetResult();

                this.PluginManager.Raise(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return default;
                }

                if (response.IsSuccessStatusCode)
                {
                    return this.Converter.Deserialize(null, response.Content.ReadAsStringAsync().GetFalseAwaitResult(), returnType);
                }
                else if ((int)response.StatusCode == 422)
                {
                    throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).Message);
                }
                else
                {
                    throw new RpcException(response.ReasonPhrase);
                }
            }
        }

        ///<inheritdoc/>
        public void Invoke(string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var strs = invokeKey.Split(':');
            if (strs.Length != 2)
            {
                throw new RpcException("不是有效的url请求。");
            }
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            var request = new HttpRequestMessage();

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
                            request.Content = new ByteArrayContent(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]).ToUTF8Bytes());
                        }
                        break;
                    }
                default:
                    break;
            }

            this.PluginManager.Raise(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = this.HttpClient.SendAsync(request, tokenSource.Token).GetAwaiter().GetResult();

                this.PluginManager.Raise(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else if ((int)response.StatusCode == 422)
                {
                    throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).Message);
                }
                else
                {
                    throw new RpcException(response.ReasonPhrase);
                }
            }
        }

        ///<inheritdoc/>
        public void Invoke(string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(invokeKey, invokeOption, ref parameters, null);
        }

        ///<inheritdoc/>
        public object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke(returnType, invokeKey, invokeOption, ref parameters, null);
        }

        ///<inheritdoc/>
        public async Task InvokeAsync(string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            var strs = invokeKey.Split(':');
            if (strs.Length != 2)
            {
                throw new RpcException("不是有效的url请求。");
            }
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            var request = new HttpRequestMessage();

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
                            request.Content = new ByteArrayContent(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]).ToUTF8Bytes());
                        }
                        break;
                    }
                default:
                    break;
            }

            await this.PluginManager.RaiseAsync(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = await this.HttpClient.SendAsync(request, tokenSource.Token);

                await this.PluginManager.RaiseAsync(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else if ((int)response.StatusCode == 422)
                {
                    throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(await response.Content.ReadAsStringAsync()).Message);
                }
                else
                {
                    throw new RpcException(response.ReasonPhrase);
                }
            }
        }

        ///<inheritdoc/>
        public async Task<object> InvokeAsync(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            var strs = invokeKey.Split(':');
            if (strs.Length != 2)
            {
                throw new RpcException("不是有效的url请求。");
            }
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            var request = new HttpRequestMessage();

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
                            request.Content = new ByteArrayContent(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]).ToUTF8Bytes());
                        }
                        break;
                    }
                default:
                    break;
            }

            await this.PluginManager.RaiseAsync(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = await this.HttpClient.SendAsync(request, tokenSource.Token);

                await this.PluginManager.RaiseAsync(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return default;
                }

                if (response.IsSuccessStatusCode)
                {
                    return this.Converter.Deserialize(null, await response.Content.ReadAsStringAsync(), returnType);
                }
                else if ((int)response.StatusCode == 422)
                {
                    throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(await response.Content.ReadAsStringAsync()).Message);
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