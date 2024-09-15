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

using Newtonsoft.Json;
using System;
using System.Threading;
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
        private readonly object[] m_empty = new object[0];
        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiClient()
        {
            this.Converter = new StringSerializerConverter(new JsonStringToClassSerializerFormatter<object>());
        }

        /// <summary>
        /// 字符串转化器
        /// </summary>
        public StringSerializerConverter Converter { get; }

        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }

        #region Rpc调用

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
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

            parameters ??= m_empty;
            var request = new HttpRequest(this);

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
                            request.FromJson(JsonConvert.SerializeObject(parameters[parameters.Length - 1]));
                        }
                        break;
                    }
                default:
                    break;
            }

            await this.PluginManager.RaiseAsync(typeof(IWebApiRequestPlugin), this, new WebApiEventArgs(request, default));

            using (var responseResult = await this.ProtectedRequestContentAsync(request, invokeOption.Timeout, invokeOption.Token))
            {
                var response = responseResult.Response;
                await this.PluginManager.RaiseAsync(typeof(IWebApiResponsePlugin), this, new WebApiEventArgs(request, response));

                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                {
                    return default;
                }

                if (response.StatusCode == 200)
                {
                    if (returnType != null)
                    {
                        return this.Converter.Deserialize(null, await response.GetBodyAsync().ConfigureAwait(false), returnType);
                    }
                    else
                    {
                        return default;
                    }

                }
                else if (response.StatusCode == 422)
                {
                    throw new RpcException(JsonConvert.DeserializeObject<ActionResult>(response.GetBody()).Message);
                }
                else
                {
                    throw new RpcException(response.StatusMessage);
                }
            }
        }

        #endregion Rpc调用
    }
}