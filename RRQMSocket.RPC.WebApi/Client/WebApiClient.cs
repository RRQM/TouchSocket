using RRQMCore;
using RRQMCore.Converter;
using RRQMCore.Extensions;
using RRQMSocket.Http;
using RRQMSocket.RPC.RRQMRPC;
using System;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// WebApi客户端
    /// </summary>
    public class WebApiClient : HttpClientBase, IRpcClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiClient()
        {
            this.stringConverter = new StringConverter();
        }

        private StringConverter stringConverter;

        /// <summary>
        /// 字符串转化器
        /// </summary>
        public StringConverter StringConverter
        {
            get { return stringConverter; }
        }

        #region RPC调用

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            string[] strs = method.Split(':');
            if (strs.Length != 2)
            {
                throw new RpcException("不是有效的url请求。");
            }
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            HttpRequest request = new HttpRequest();

            if (parameters.Length > 0)
            {
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
                                .SetUrl(strs[1])
                                .FromJson(parameters[0].ToJsonString())
                                .AsPost();
                            break;
                        }
                    default:
                        break;
                }
            }

            HttpResponse response = this.Request(request, false, invokeOption.Timeout, invokeOption.Token);
            
            if (invokeOption.FeedbackType!= FeedbackType.WaitInvoke)
            {
                return default;
            }
            
            if (response.StatusCode == "200")
            {
                return (T)this.stringConverter.ConvertFrom(response.GetBody(), typeof(T));
            }
            else
            {
                throw new RpcException(response.StatusMessage);
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            string[] strs = method.Split(':');
            if (strs.Length != 2)
            {
                throw new RpcException("不是有效的url请求。");
            }
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            HttpRequest request = new HttpRequest();

            if (parameters.Length > 0)
            {
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
                                .SetUrl(strs[1])
                                .FromJson(parameters[0].ToJsonString())
                                .AsPost();
                            break;
                        }
                    default:
                        break;
                }
            }
            HttpResponse response = this.Request(request, false, invokeOption.Timeout, invokeOption.Token);
            if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
            {
                return;
            }
            if (response.StatusCode != "200")
            {
                throw new RpcException(response.StatusMessage);
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke<T>(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="RRQMException"></exception>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(method, invokeOption, parameters);
            });
        }

        #endregion RPC调用
    }
}