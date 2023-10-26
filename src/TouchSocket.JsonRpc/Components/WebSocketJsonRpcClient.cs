using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Resources;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于WebSocket协议的JsonRpc客户端。
    /// </summary>
    public class WebSocketJsonRpcClient : WebSocketClientBase, IWebSocketJsonRpcClient
    {
        private readonly WaitHandlePool<JsonRpcWaitResult> m_waitHandle = new WaitHandlePool<JsonRpcWaitResult>();

        /// <inheritdoc/>
        public RpcStore RpcStore { get; private set; }

        /// <summary>
        /// WaitHandle
        /// </summary>
        public WaitHandlePool<JsonRpcWaitResult> WaitHandle => this.m_waitHandle;

        /// <inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            parameters ??= new object[0];
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                this.SendWithWS(jsonRpcRequest.ToJsonString());
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                    default:
                        {
                            if (invokeOption.Token.CanBeCanceled)
                            {
                                waitData.SetCancellationToken(invokeOption.Token);
                            }

                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                                        if (resultContext.Error != null)
                                        {
                                            throw new RpcException(resultContext.Error.Message);
                                        }

                                        if (resultContext.Result == null)
                                        {
                                            return default;
                                        }
                                        else
                                        {
                                            return JsonRpcUtility.ResultParseToType(resultContext.Result, returnType);
                                        }
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException("等待结果超时");
                                case WaitDataStatus.Canceled:
                                    return default;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            parameters ??= new object[0];
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                this.SendWithWS(jsonRpcRequest.ToJsonString());
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                    default:
                        {
                            if (invokeOption.Token.CanBeCanceled)
                            {
                                waitData.SetCancellationToken(invokeOption.Token);
                            }

                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                                        if (resultContext.Error != null)
                                        {
                                            throw new RpcException(resultContext.Error.Message);
                                        }
                                        return;
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException("等待结果超时");
                                case WaitDataStatus.Canceled:
                                    return;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke(returnType, method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            parameters ??= new object[0];
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                await this.SendWithWSAsync(jsonRpcRequest.ToJsonString());
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                    default:
                        {
                            if (invokeOption.Token.CanBeCanceled)
                            {
                                waitData.SetCancellationToken(invokeOption.Token);
                            }

                            switch (await waitData.WaitAsync(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                                        if (resultContext.Error != null)
                                        {
                                            throw new RpcException(resultContext.Error.Message);
                                        }
                                        return;
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException("等待结果超时");
                                case WaitDataStatus.Canceled:
                                    return;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            parameters ??= new object[0];
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                await this.SendWithWSAsync(jsonRpcRequest.ToJsonString());
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                    default:
                        {
                            if (invokeOption.Token.CanBeCanceled)
                            {
                                waitData.SetCancellationToken(invokeOption.Token);
                            }

                            switch (await waitData.WaitAsync(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                                        if (resultContext.Error != null)
                                        {
                                            throw new RpcException(resultContext.Error.Message);
                                        }

                                        if (resultContext.Result == null)
                                        {
                                            return default;
                                        }
                                        else
                                        {
                                            return JsonRpcUtility.ResultParseToType(resultContext.Result, returnType);
                                        }
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException("等待结果超时");
                                case WaitDataStatus.Canceled:
                                    return default;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (this.Container.IsRegistered(typeof(RpcStore)))
            {
                this.RpcStore = this.Container.Resolve<RpcStore>();
            }
            else
            {
                this.RpcStore = new RpcStore(this.Container);
            }
            this.RpcStore.AddRpcParser(this);
        }

        #region Rpc解析器

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap { get; private set; } = new ActionMap(true);

        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Remove(attribute.GetInvokenKey(methodInstance));
                }
            }
        }

        #endregion Rpc解析器

        private void ThisInvoke(object obj)
        {
            var callContext = (JsonRpcCallContextBase)obj;
            var invokeResult = new InvokeResult();

            try
            {
                JsonRpcUtility.BuildRequestContext(this.ActionMap, ref callContext);
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
            }

            if (callContext.MethodInstance != null)
            {
                if (!callContext.MethodInstance.IsEnable)
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
            }
            else
            {
                invokeResult.Status = InvokeStatus.UnFound;
            }

            if (invokeResult.Status == InvokeStatus.Ready)
            {
                var rpcServer = callContext.MethodInstance.ServerFactory.Create(callContext, callContext.JsonRpcContext.Parameters);
                if (rpcServer is ITransientRpcServer transientRpcServer)
                {
                    transientRpcServer.CallContext = callContext;
                }

                invokeResult = RpcStore.Execute(rpcServer, callContext.JsonRpcContext.Parameters, callContext);
            }

            if (!callContext.JsonRpcContext.Id.HasValue)
            {
                return;
            }
            var error = JsonRpcUtility.GetJsonRpcError(invokeResult);
            this.Response(callContext, invokeResult.Result, error);
        }

        private void Response(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
        {
            try
            {
                JsonRpcResponseBase response;
                if (error == null)
                {
                    response = new JsonRpcSuccessResponse
                    {
                        Result = result,
                        Id = callContext.JsonRpcContext.Id
                    };
                }
                else
                {
                    response = new JsonRpcErrorResponse
                    {
                        Error = error,
                        Id = callContext.JsonRpcContext.Id
                    };
                }
                var str = JsonRpcUtility.ToJsonRpcResponseString(response);
                this.SendWithWS(str);
            }
            catch
            {
            }
        }

        /// <inheritdoc/>
        protected override async Task OnReceivedWSDataFrame(WSDataFrame dataFrame)
        {
            string jsonString = null;
            if (dataFrame.Opcode == WSDataType.Text)
            {
                jsonString = dataFrame.ToText();
            }

            if (string.IsNullOrEmpty(jsonString))
            {
                await base.OnReceivedWSDataFrame(dataFrame);
                return;
            }

            if (this.ActionMap.Count > 0 && JsonRpcUtility.IsJsonRpcRequest(jsonString))
            {
                _ = Task.Factory.StartNew(this.ThisInvoke, new WebSocketJsonRpcCallContext(this, jsonString));
            }
            else
            {
                var waitResult = JsonRpcUtility.ToJsonRpcWaitResult(jsonString);
                if (waitResult != null)
                {
                    waitResult.Status = 1;
                    this.m_waitHandle.SetRun(waitResult);
                }
            }

            await base.OnReceivedWSDataFrame(dataFrame);
        }
    }
}