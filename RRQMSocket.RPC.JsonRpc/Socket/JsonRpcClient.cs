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

using RRQMCore.Helper;
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
    public class JsonRpcClient : TcpClient, IJsonRpcClient
    {
        private int maxPackageSize;

        private JsonRpcProtocolType protocolType;

        private RRQMWaitHandlePool<IWaitResult> waitHandle;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSetDataHandlingAdapter => false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcClient()
        {
            this.waitHandle = new RRQMWaitHandlePool<IWaitResult>();
        }

        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public int MaxPackageSize
        {
            get { return this.maxPackageSize; }
        }

        /// <summary>
        /// 协议类型
        /// </summary>
        public JsonRpcProtocolType ProtocolType
        {
            get { return this.protocolType; }
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<IWaitResult> waitData = this.waitHandle.GetWaitData(context);

            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
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
                switch (this.protocolType)
                {
                    case JsonRpcProtocolType.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jobject.ToString(Formatting.None)));
                            break;
                        }
                    case JsonRpcProtocolType.Http:
                        {
                            HttpRequest httpRequest = new HttpRequest();
                            httpRequest.Method = "POST";
                            httpRequest.FromJson(jobject.ToString(Formatting.None));
                            httpRequest.Build(byteBlock);
                        }
                        break;
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SendAsync(byteBlock);
                        }
                        break;

                    case FeedbackType.WaitSend:
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                        }
                        break;

                    default:
                        break;
                }
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.Timeout);
                        JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new TimeoutException("等待结果超时");
                        }
                        if (resultContext.error != null)
                        {
                            throw new RRQMRPCException(resultContext.error.message);
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

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<IWaitResult> waitData = this.waitHandle.GetWaitData(context);

            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
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
                switch (this.protocolType)
                {
                    case JsonRpcProtocolType.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jobject.ToString(Formatting.None)));
                            break;
                        }
                    case JsonRpcProtocolType.Http:
                        {
                            HttpRequest httpRequest = new HttpRequest();
                            httpRequest.Method = "POST";
                            httpRequest.FromJson(jobject.ToString(Formatting.None));
                            httpRequest.Build(byteBlock);
                        }
                        break;
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SendAsync(byteBlock);
                        }
                        break;

                    case FeedbackType.WaitSend:
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                        }
                        break;

                    default:
                        break;
                }
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.Timeout);
                        JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new TimeoutException("等待结果超时");
                        }
                        if (resultContext.error != null)
                        {
                            throw new RRQMRPCException(resultContext.error.message);
                        }
                        return;
                    }
                default:
                    return;
            }
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
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
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
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
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            switch (this.protocolType)
            {
                case JsonRpcProtocolType.Tcp:
                    {
                        string jsonString = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                        JsonResponseContext responseContext = (JsonResponseContext)JsonConvert.DeserializeObject(jsonString, typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitContext waitContext = new JsonRpcWaitContext();
                            waitContext.Status = 1;
                            waitContext.Sign = int.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.Return = responseContext.result;
                            this.waitHandle.SetRun(waitContext);
                        }
                        break;
                    }

                case JsonRpcProtocolType.Http:
                    {
                        HttpResponse httpResponse = (HttpResponse)requestInfo;
                        JsonResponseContext responseContext = (JsonResponseContext)JsonConvert.DeserializeObject(httpResponse.Body, typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitContext waitContext = new JsonRpcWaitContext();
                            waitContext.Status = 1;
                            waitContext.Sign = int.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.Return = responseContext.result;
                            this.waitHandle.SetRun(waitContext);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 禁用适配器赋值
        /// </summary>
        /// <param name="adapter"></param>
        public override sealed void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            throw new RRQMException($"{nameof(JsonRpcSocketClient)}不允许设置适配器。");
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            this.maxPackageSize = (int)clientConfig.GetValue(JsonRpcClientConfig.MaxPackageSizeProperty);
            this.protocolType = (JsonRpcProtocolType)clientConfig.GetValue(JsonRpcClientConfig.ProtocolTypeProperty);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ClientConnectingEventArgs e)
        {
            switch (this.protocolType)
            {
                case JsonRpcProtocolType.Tcp:
                    base.SetAdapter(new TerminatorPackageAdapter(this.maxPackageSize, "\r\n"));
                    break;

                case JsonRpcProtocolType.Http:
                    base.SetAdapter(new HttpDataHandlingAdapter(this.maxPackageSize, HttpType.Client));
                    break;
            }
            base.OnConnecting(e);
        }
    }
}