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

using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的TcpJsonRpc客户端
    /// </summary>
    public class TcpJsonRpcClient : TcpClientBase, ITcpJsonRpcClient
    {
        private readonly WaitHandlePool<IWaitResult> m_waitHandle = new WaitHandlePool<IWaitResult>();
        private IRpcServerProvider m_rpcServerProvider;

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap { get; private set; } = new ActionMap(true);

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
            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        waitData.Wait(invokeOption.Timeout);
                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                        this.m_waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new TimeoutException("等待结果超时");
                        }
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
                            if (returnType.IsPrimitive || returnType == typeof(string))
                            {
                                return resultContext.Result.ToString().ParseToType(returnType);
                            }
                            else
                            {
                                return resultContext.Result.ToJsonString().FromJsonString(returnType);
                            }
                        }
                    }
                default:
                    return default;
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
            var jsonRpcRequest = new JsonRpcRequest()
            {
                Method = method,
                Params = parameters
            };

            jsonRpcRequest.Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0;

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitSend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        waitData.Wait(invokeOption.Timeout);
                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                        this.m_waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new TimeoutException("等待结果超时");
                        }
                        if (resultContext.Error != null)
                        {
                            throw new RpcException(resultContext.Error.Message);
                        }
                        break;
                    }
                default:
                    return;
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
            var jsonRpcRequest = new JsonRpcRequest()
            {
                Method = method,
                Params = parameters
            };

            jsonRpcRequest.Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0;

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitSend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        await waitData.WaitAsync(invokeOption.Timeout);
                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                        this.m_waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new TimeoutException("等待结果超时");
                        }
                        if (resultContext.Error != null)
                        {
                            throw new RpcException(resultContext.Error.Message);
                        }
                        break;
                    }
                default:
                    return;
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

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        this.Send(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                        await waitData.WaitAsync(invokeOption.Timeout);
                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                        this.m_waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new TimeoutException("等待结果超时");
                        }
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
                            if (returnType.IsPrimitive || returnType == typeof(string))
                            {
                                return resultContext.Result.ToString().ParseToType(returnType);
                            }
                            else
                            {
                                return resultContext.Result.ToJsonString().FromJsonString(returnType);
                            }
                        }
                    }
                default:
                    return default;
            }
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (this.Resolver.IsRegistered(typeof(IRpcServerProvider)))
            {
                var rpcServerProvider = this.Resolver.Resolve<IRpcServerProvider>();
                this.RegisterServer(rpcServerProvider.GetMethods());
                this.m_rpcServerProvider = rpcServerProvider;
            }
        }

        /// <inheritdoc/>
        protected override Task ReceivedData(ReceivedDataEventArgs e)
        {
            string jsonString = null;
            if (e.ByteBlock == null)
            {
                if (e.RequestInfo is IJsonRpcRequestInfo jsonRpcRequest)
                {
                    jsonString = jsonRpcRequest.GetJsonRpcString();
                }
            }
            else
            {
                jsonString = e.ByteBlock.ToString();
            }

            if (string.IsNullOrEmpty(jsonString))
            {
                return base.ReceivedData(e);
            }

            try
            {
                if (this.ActionMap.Count > 0 && JsonRpcUtility.IsJsonRpcRequest(jsonString))
                {
                    Task.Factory.StartNew(this.ThisInvoke, new WebSocketJsonRpcCallContext(this, jsonString));
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
            }
            catch
            {
            }
            return base.ReceivedData(e);
        }

        private void RegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
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
                this.Send(str);
            }
            catch
            {
            }
        }

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
                invokeResult = this.m_rpcServerProvider.Execute(callContext, callContext.JsonRpcContext.Parameters);
            }

            if (!callContext.JsonRpcContext.Id.HasValue)
            {
                return;
            }
            var error = JsonRpcUtility.GetJsonRpcError(invokeResult);
            this.Response(callContext, invokeResult.Result, error);
        }
    }
}