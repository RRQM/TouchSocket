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

        #region RPC调用

        ///<inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var strs = method.Split(':');
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
                            request.FromJson(parameters[parameters.Length - 1].ToJson());
                        }
                        break;
                    }
                default:
                    break;
            }

            var response = this.RequestContent(request, false, invokeOption.Timeout, invokeOption.Token);

            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return default;
            }

            if (response.StatusCode == "200")
            {
                return this.StringConverter.ConvertFrom(response.GetBody(), returnType);
            }
            else if (response.StatusCode == "422")
            {
                throw new RpcException(response.GetBody().FromJson<ActionResult>().Message);
            }
            else
            {
                throw new RpcException(response.StatusMessage);
            }
        }

        ///<inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var strs = method.Split(':');
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
                            request.FromJson(parameters[parameters.Length - 1].ToJson());
                        }
                        break;
                    }
                default:
                    break;
            }
            var response = this.RequestContent(request, false, invokeOption.Timeout, invokeOption.Token);
            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return;
            }

            if (response.StatusCode == "200")
            {
                return;
            }
            else if (response.StatusCode == "422")
            {
                throw new RpcException(response.GetBody().FromJson<ActionResult>().Message);
            }
            else
            {
                throw new RpcException(response.StatusMessage);
            }
        }

        ///<inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        ///<inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke(returnType, method, invokeOption, ref parameters, null);
        }

        ///<inheritdoc/>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        ///<inheritdoc/>
        public Task<object> InvokeAsync(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke(returnType, method, invokeOption, parameters);
            });
        }

        #endregion RPC调用
    }
}