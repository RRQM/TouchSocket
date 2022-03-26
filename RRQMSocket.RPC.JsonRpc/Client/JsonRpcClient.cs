//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Extensions;
using RRQMCore.Run;
using RRQMCore.XREF.Newtonsoft.Json;
using RRQMCore.XREF.Newtonsoft.Json.Linq;
using RRQMSocket.Http;
using RRQMSocket.RPC.RRQMRPC;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpc客户端
    /// </summary>
    public class JsonRpcClient : WaitingTcpClient, IRpcClient
    {
        private JRPT jrpt;

        private WaitHandlePool<IWaitResult> waitHandle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcClient(JRPT pt)
        {
            this.jrpt = pt;
            this.waitHandle = new WaitHandlePool<IWaitResult>();
        }

        /// <summary>
        /// 协议类型
        /// </summary>
        public JRPT JRPT => this.jrpt;

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
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<IWaitResult> waitData = this.waitHandle.GetWaitData(context);

            using (ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                JObject jobject = new JObject();
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                jobject.Add("method", JToken.FromObject(method));
                jobject.Add("params", JToken.FromObject(parameters));

                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    jobject.Add("id", JToken.FromObject(context.Sign.ToString()));
                }
                else
                {
                    jobject.Add("id", null);
                }
                switch (this.jrpt)
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
                            request.URL = this.RemoteIPHost.GetUrlPath();
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
                            this.waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
                            JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                            this.waitHandle.Destroy(waitData);

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
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<IWaitResult> waitData = this.waitHandle.GetWaitData(context);

            using (ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                JObject jobject = new JObject();
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                jobject.Add("method", JToken.FromObject(method));
                jobject.Add("params", JToken.FromObject(parameters));

                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    jobject.Add("id", JToken.FromObject(context.Sign.ToString()));
                }
                else
                {
                    jobject.Add("id", null);
                }
                switch (this.jrpt)
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
                            request.URL = this.RemoteIPHost.GetUrlPath();
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
                            this.waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
                            JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                            this.waitHandle.Destroy(waitData);

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

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            switch (this.jrpt)
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
                            this.waitHandle.SetRun(waitContext);
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
                            this.waitHandle.SetRun(waitContext);
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
            switch (this.jrpt)
            {
                case JRPT.Tcp:
                    e.DataHandlingAdapter = new TerminatorPackageAdapter(this.MaxPackageSize, "\r\n");
                    break;

                case JRPT.Http:
                    e.DataHandlingAdapter = new HttpClientDataHandlingAdapter(this.MaxPackageSize);
                    break;
            }
            base.OnConnecting(e);
        }
    }
}