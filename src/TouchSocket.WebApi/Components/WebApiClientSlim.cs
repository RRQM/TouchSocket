﻿#if !NET45
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
    public class WebApiClientSlim : TouchSocket.Http.HttpClientSlim, IWebApiClientBase
    {
        /// <summary>
        /// 使用<see cref="HttpClient"/>为基础的WebApi客户端。
        /// </summary>
        /// <param name="httpClient"></param>
        public WebApiClientSlim(HttpClient httpClient = default) : base(httpClient)
        {
            this.StringConverter = new StringConverter();
        }

        /// <summary>
        /// 字符串转化器
        /// </summary>
        public StringConverter StringConverter { get; }

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

            this.PluginsManager.Raise(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = this.HttpClient.SendAsync(request, tokenSource.Token).GetAwaiter().GetResult();

                this.PluginsManager.Raise(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return default;
                }

                if (response.IsSuccessStatusCode)
                {
                    return this.StringConverter.ConvertFrom(response.Content.ReadAsStringAsync().GetAwaiter().GetResult(), returnType);
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

            this.PluginsManager.Raise(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = this.HttpClient.SendAsync(request, tokenSource.Token).GetAwaiter().GetResult();

                this.PluginsManager.Raise(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

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

            await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = await this.HttpClient.SendAsync(request, tokenSource.Token);

                await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

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

            await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            using (var tokenSource = new CancellationTokenSource(invokeOption.Timeout))
            {
                if (invokeOption.Token.CanBeCanceled)
                {
                    invokeOption.Token.Register(tokenSource.Cancel);
                }
                var response = await this.HttpClient.SendAsync(request, tokenSource.Token);

                await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return default;
                }

                if (response.IsSuccessStatusCode)
                {
                    return this.StringConverter.ConvertFrom(await response.Content.ReadAsStringAsync(), returnType);
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