//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.Run;
using TouchSocket.Core.XREF.Newtonsoft.Json;
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Http;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.JsonRpc
{
    /// <summary>
    /// JsonRpc客户端
    /// </summary>
    public class JsonRpcClient : TcpClientBase, IJsonRpcClient
    {
        private JRPT m_jrpt;

        private WaitHandlePool<IWaitResult> m_waitHandle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcClient()
        {
            this.m_waitHandle = new WaitHandlePool<IWaitResult>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<IRpcClient, bool> TryCanInvoke { get; set; }

        /// <summary>
        /// 协议类型
        /// </summary>
        public JRPT JRPT => this.m_jrpt;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"></param>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.m_jrpt = config.GetValue<JRPT>(JsonRpcConfigExtensions.JRPTProperty);
            base.LoadConfig(config);
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
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<IWaitResult> waitData = this.m_waitHandle.GetWaitData(context);

            using (ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                JObject jobject = new JObject();
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                jobject.Add("method", JToken.FromObject(method));
                if (parameters == null)
                {
                    parameters = new object[0];
                }
                jobject.Add("params", JToken.FromObject(parameters));

                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    jobject.Add("id", JToken.FromObject(context.Sign.ToString()));
                }
                else
                {
                    jobject.Add("id", null);
                }
                switch (this.m_jrpt)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jobject.ToString(Formatting.None)));
                            break;
                        }
                    case JRPT.Http:
                        {
                            HttpRequest request = new HttpRequest();
                            request.Method = "POST";
                            request.SetUrl(this.RemoteIPHost.GetUrlPath());
                            request.FromJson(jobject.ToString(Formatting.None));
                            request.Build(byteBlock);
                        }
                        break;
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SendAsync(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
                            JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                            this.m_waitHandle.Destroy(waitData);

                            if (resultContext.Status == 0)
                            {
                                throw new TimeoutException("等待结果超时");
                            }
                            if (resultContext.error != null)
                            {
                                throw new RpcException(resultContext.error.message);
                            }

                            if (resultContext.Return == null)
                            {
                                return default;
                            }
                            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                            {
                                return (T)resultContext.Return.ToString().ParseToType(typeof(T));
                            }

                            return JsonConvert.DeserializeObject<T>(resultContext.Return.ToString());
                        }
                    default:
                        return default;
                }
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
        /// <exception cref="Exception"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<IWaitResult> waitData = this.m_waitHandle.GetWaitData(context);

            using (ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                JObject jobject = new JObject();
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                jobject.Add("method", JToken.FromObject(method));
                if (parameters == null)
                {
                    parameters = new object[0];
                }
                jobject.Add("params", JToken.FromObject(parameters));
                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    jobject.Add("id", JToken.FromObject(context.Sign.ToString()));
                }
                else
                {
                    jobject.Add("id", null);
                }
                switch (this.m_jrpt)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jobject.ToString(Formatting.None)));
                            break;
                        }
                    case JRPT.Http:
                        {
                            HttpRequest request = new HttpRequest();
                            request.Method = "POST";
                            request.SetUrl(this.RemoteIPHost.GetUrlPath());
                            request.FromJson(jobject.ToString(Formatting.None));
                            request.Build(byteBlock);
                        }
                        break;
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SendAsync(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
                            JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                            this.m_waitHandle.Destroy(waitData);

                            if (resultContext.Status == 0)
                            {
                                throw new TimeoutException("等待结果超时");
                            }
                            if (resultContext.error != null)
                            {
                                throw new RpcException(resultContext.error.message);
                            }
                            break;
                        }
                    default:
                        return;
                }
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
        /// <exception cref="Exception"></exception>
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
        /// <exception cref="Exception"></exception>
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
        /// <exception cref="Exception"></exception>
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
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(method, invokeOption, parameters);
            });
        }

        #endregion RPC调用

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            switch (this.m_jrpt)
            {
                case JRPT.Tcp:
                    {
                        string jsonString = byteBlock.ToString();
                        JsonResponseContext responseContext = (JsonResponseContext)JsonConvert.DeserializeObject(jsonString, typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitContext waitContext = new JsonRpcWaitContext();
                            waitContext.Status = 1;
                            waitContext.Sign = long.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.Return = responseContext.result;
                            this.m_waitHandle.SetRun(waitContext);
                        }
                        break;
                    }

                case JRPT.Http:
                    {
                        HttpResponse httpResponse = (HttpResponse)requestInfo;
                        JsonResponseContext responseContext = (JsonResponseContext)JsonConvert.DeserializeObject(httpResponse.GetBody(), typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitContext waitContext = new JsonRpcWaitContext();
                            waitContext.Status = 1;
                            waitContext.Sign = long.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.Return = responseContext.result;
                            this.m_waitHandle.SetRun(waitContext);
                        }
                        break;
                    }
            }
            base.HandleReceivedData(byteBlock, requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ClientConnectingEventArgs e)
        {
            switch (this.m_jrpt)
            {
                case JRPT.Tcp:
                    this.SetDataHandlingAdapter(new TerminatorPackageAdapter("\r\n"));
                    break;

                case JRPT.Http:
                    this.SetDataHandlingAdapter(new HttpClientDataHandlingAdapter());
                    break;
            }
            base.OnConnecting(e);
        }
    }
}