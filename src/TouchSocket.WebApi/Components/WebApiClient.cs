//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi
{
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
            this.StringConverter = new StringConverter();
        }

        /// <summary>
        /// 字符串转化器
        /// </summary>
        public StringConverter StringConverter { get; }

        #region Rpc调用

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

            var request = new HttpRequest();

            switch (strs[0])
            {
                case "GET":
                    {
                        request.InitHeaders()
                            .SetHost(this.RemoteIPHost.Host)
                            .SetUrl(strs[1].Format(parameters))
                            .AsGet();
                        break;
                    }
                case "POST":
                    {
                        request.InitHeaders()
                            .SetHost(this.RemoteIPHost.Host)
                            .SetUrl(strs[1].Format(parameters))
                            .AsPost();
                        if (parameters.Length > 0)
                        {
                            request.FromJson(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]));
                        }
                        break;
                    }
                default:
                    break;
            }

            this.PluginsManager.Raise(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            var response = this.RequestContent(request, false, invokeOption.Timeout, invokeOption.Token);

            this.PluginsManager.Raise(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return default;
            }

            if (response.StatusCode == 200)
            {
                return this.StringConverter.ConvertFrom(response.GetBody(), returnType);
            }
            else if (response.StatusCode == 422)
            {
                throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(response.GetBody()).Message);
            }
            else
            {
                throw new RpcException(response.StatusMessage);
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

            var request = new HttpRequest();

            switch (strs[0])
            {
                case "GET":
                    {
                        request.InitHeaders()
                            .SetHost(this.RemoteIPHost.Host)
                            .SetUrl(strs[1].Format(parameters))
                            .AsGet();
                        break;
                    }
                case "POST":
                    {
                        request.InitHeaders()
                        .SetHost(this.RemoteIPHost.Host)
                        .SetUrl(strs[1].Format(parameters))
                        .AsPost();
                        if (parameters.Length > 0)
                        {
                            request.FromJson(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]));
                        }
                        break;
                    }
                default:
                    break;
            }

            this.PluginsManager.Raise(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));
            var response = this.RequestContent(request, false, invokeOption.Timeout, invokeOption.Token);
            this.PluginsManager.Raise(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return;
            }

            if (response.StatusCode == 200)
            {
                return;
            }
            else if (response.StatusCode == 422)
            {
                throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(response.GetBody()).Message);
            }
            else
            {
                throw new RpcException(response.StatusMessage);
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

            var request = new HttpRequest();

            switch (strs[0])
            {
                case "GET":
                    {
                        request.InitHeaders()
                            .SetHost(this.RemoteIPHost.Host)
                            .SetUrl(strs[1].Format(parameters))
                            .AsGet();
                        break;
                    }
                case "POST":
                    {
                        request.InitHeaders()
                        .SetHost(this.RemoteIPHost.Host)
                        .SetUrl(strs[1].Format(parameters))
                        .AsPost();
                        if (parameters.Length > 0)
                        {
                            request.FromJson(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]));
                        }
                        break;
                    }
                default:
                    break;
            }

            await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));
            var response = await this.RequestContentAsync(request, false, invokeOption.Timeout, invokeOption.Token);
            await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return;
            }

            if (response.StatusCode == 200)
            {
                return;
            }
            else if (response.StatusCode == 422)
            {
                throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(response.GetBody()).Message);
            }
            else
            {
                throw new RpcException(response.StatusMessage);
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

            var request = new HttpRequest();

            switch (strs[0])
            {
                case "GET":
                    {
                        request.InitHeaders()
                            .SetHost(this.RemoteIPHost.Host)
                            .SetUrl(strs[1].Format(parameters))
                            .AsGet();
                        break;
                    }
                case "POST":
                    {
                        request.InitHeaders()
                            .SetHost(this.RemoteIPHost.Host)
                            .SetUrl(strs[1].Format(parameters))
                            .AsPost();
                        if (parameters.Length > 0)
                        {
                            request.FromJson(SerializeConvert.ToJsonString(parameters[parameters.Length - 1]));
                        }
                        break;
                    }
                default:
                    break;
            }

            await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnRequest), this, new WebApiEventArgs(request, default));

            var response = await this.RequestContentAsync(request, false, invokeOption.Timeout, invokeOption.Token);

            await this.PluginsManager.RaiseAsync(nameof(IWebApiPlugin.OnResponse), this, new WebApiEventArgs(request, response));

            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return default;
            }

            if (response.StatusCode == 200)
            {
                return this.StringConverter.ConvertFrom(response.GetBody(), returnType);
            }
            else if (response.StatusCode == 422)
            {
                throw new RpcException(SerializeConvert.FromJsonString<ActionResult>(response.GetBody()).Message);
            }
            else
            {
                throw new RpcException(response.StatusMessage);
            }
        }

        #endregion Rpc调用
    }
}