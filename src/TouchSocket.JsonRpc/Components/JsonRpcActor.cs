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
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    public sealed class JsonRpcActor : IJsonRpcClient
    {
        private readonly JsonRpcRequestConverter m_jsonRpcRequestConverter = new JsonRpcRequestConverter();
        private readonly JsonRpcWaitResultConverter m_jsonRpcWaitResultConverter = new JsonRpcWaitResultConverter();
        private readonly WaitHandlePool<InternalJsonRpcWaitResult> m_waitHandle = new WaitHandlePool<InternalJsonRpcWaitResult>();
        private IRpcServerProvider m_rpcServerProvider;

        public ActionMap ActionMap { get; private set; } = new ActionMap(true);

        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public ILog Logger { get; set; }
        public IResolver Resolver { get; set; }
        public IRpcDispatcher<JsonRpcActor, IJsonRpcCallContext> RpcDispatcher { get; set; } = new ConcurrencyRpcDispatcher<JsonRpcActor, IJsonRpcCallContext>();
        public Func<ReadOnlyMemory<byte>, Task> SendAction { get; set; }
        public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter { get; set; }

        public static void AddRpcToMap(IRpcServerProvider rpcServerProvider, ActionMap actionMap)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(rpcServerProvider, nameof(rpcServerProvider));
            ThrowHelper.ThrowArgumentNullExceptionIf(actionMap, nameof(actionMap));

            foreach (var rpcMethod in rpcServerProvider.GetMethods())
            {
                if (rpcMethod.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    actionMap.Add(attribute.GetInvokeKey(rpcMethod), rpcMethod);
                }
            }
        }

        public async Task InputReceiveAsync(ReadOnlyMemory<byte> memory, JsonRpcCallContextBase callContext)
        {
            try
            {
                var str = memory.Span.ToString(this.Encoding);

                if (callContext != null && this.TryParseRequest(str, out var jsonRpcRequest))
                {
                    callContext.SetJsonRpcRequest(jsonRpcRequest);
                    var rpcMethod = callContext.RpcMethod;
                    if (rpcMethod == null || rpcMethod.Reenterable == false || this.RpcDispatcher.Reenterable == false)
                    {
                        callContext.SetResolver(this.Resolver);
                    }
                    else
                    {
                        callContext.SetResolver(this.Resolver.CreateScopedResolver());
                    }

                    this.BuildRequestContext(callContext, jsonRpcRequest);

                    await this.RpcDispatcher.Dispatcher(this, callContext, this.ThisInvokeAsync).ConfigureAwait(false);
                }
                else if (this.TryParseResponse(str, out var internalJsonRpcWaitResult))
                {
                    internalJsonRpcWaitResult.Status = 1;
                    this.m_waitHandle.SetRun(internalJsonRpcWaitResult);
                }
                else
                {
                    //throw new RpcException(Resources.TouchSocketCoreResource.JsonRpcParseError);
                    throw new RpcException("无法解析的JsonRpc数据");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Debug(ex.Message);
            }
        }

        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            var waitData = this.m_waitHandle.GetWaitDataAsync(out var sign);
            invokeOption ??= InvokeOption.WaitInvoke;

            parameters ??= new object[0];

            var strs = new string[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                strs[i] = this.SerializerConverter.Serialize(this, parameters[i]);
            }

            var jsonRpcRequest = new InternalJsonRpcRequest
            {
                Method = invokeKey,
                ParamsStrings = strs,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? sign : 0
            };

            try
            {
                using (var byteBlock = new ByteBlock(1024 * 64))
                {
                    var str = this.BuildJsonRpcRequest(jsonRpcRequest);
                    byteBlock.WriteNormalString(str, this.Encoding);
                    await this.SendAction(byteBlock.Memory).ConfigureAwait(false);
                }

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

                            switch (await waitData.WaitAsync(invokeOption.Timeout).ConfigureAwait(false))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = waitData.WaitResult;
                                        if (resultContext.ErrorCode != 0)
                                        {
                                            throw new RpcException(resultContext.ErrorMessage);
                                        }

                                        if (resultContext.Result == null)
                                        {
                                            return default;
                                        }
                                        else
                                        {
                                            return this.ResultParseToType(resultContext.Result, returnType);
                                        }
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException(Resources.TouchSocketCoreResource.OperationOvertime);
                                case WaitDataStatus.Canceled:
                                    return default;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new UnknownErrorException();
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
            }
        }

        public void SetRpcServerProvider(IRpcServerProvider rpcServerProvider)
        {
            AddRpcToMap(rpcServerProvider, this.ActionMap);
            this.m_rpcServerProvider = rpcServerProvider;
        }

        public void SetRpcServerProvider(IRpcServerProvider rpcServerProvider, ActionMap actionMap)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(rpcServerProvider, nameof(rpcServerProvider));
            ThrowHelper.ThrowArgumentNullExceptionIf(actionMap, nameof(actionMap));
            this.m_rpcServerProvider = rpcServerProvider;
            this.ActionMap = actionMap;
        }

        /// <summary>
        /// GetJsonRpcError
        /// </summary>
        /// <param name="invokeResult"></param>
        /// <returns></returns>
        private static JsonRpcError GetJsonRpcError(InvokeResult invokeResult)
        {
            switch (invokeResult.Status)
            {
                case InvokeStatus.Success:
                    {
                        return new JsonRpcError(0, string.Empty);
                    }
                case InvokeStatus.UnFound:
                    {
                        return new JsonRpcError(-32601, "函数未找到");
                    }
                case InvokeStatus.UnEnable:
                    {
                        return new JsonRpcError(-32601, "函数已被禁用");
                    }
                case InvokeStatus.InvocationException:
                    {
                        return new JsonRpcError(-32603, invokeResult.Message);
                    }
                case InvokeStatus.Exception:
                default:
                    {
                        return new JsonRpcError(-32602, invokeResult.Message);
                    }
            }
        }

        private string BuildJsonRpcRequest(InternalJsonRpcRequest jsonRpcRequest)
        {
            return JsonConvert.SerializeObject(jsonRpcRequest, this.m_jsonRpcRequestConverter);
        }

        private void BuildRequestContext(JsonRpcCallContextBase callContext, InternalJsonRpcRequest jsonRpcRequest)
        {
            if (this.ActionMap.TryGetRpcMethod(jsonRpcRequest.Method, out var rpcMethod))
            {
                callContext.SetRpcMethod(rpcMethod);
                var ps = new object[rpcMethod.Parameters.Length];

                if (jsonRpcRequest.ParamsObject == null)
                {
                    for (var i = 0; i < ps.Length; i++)
                    {
                        var parameter = rpcMethod.Parameters[i];
                        if (parameter.IsCallContext)
                        {
                            ps[i] = callContext;
                        }
                        else if (parameter.IsFromServices)
                        {
                            ps[i] = callContext.Resolver.Resolve(parameter.Type);
                        }
                        else if (parameter.ParameterInfo.HasDefaultValue)
                        {
                            ps[i] = parameter.ParameterInfo.DefaultValue;
                        }
                        else
                        {
                            ps[i] = parameter.Type.GetDefault();
                        }
                    }
                }
                if (jsonRpcRequest.ParamsObject is JObject obj)
                {
                    for (var i = 0; i < ps.Length; i++)
                    {
                        var parameter = rpcMethod.Parameters[i];
                        if (parameter.IsCallContext)
                        {
                            ps[i] = callContext;
                        }
                        else if (parameter.IsFromServices)
                        {
                            ps[i] = callContext.Resolver.Resolve(parameter.Type);
                        }
                        else if (obj.TryGetValue(parameter.Name, out var jToken))
                        {
                            var str = jToken.ToString();
                            ps[i] = this.ResultParseToType(str, parameter.Type);
                        }
                        else if (parameter.ParameterInfo.HasDefaultValue)
                        {
                            ps[i] = parameter.ParameterInfo.DefaultValue;
                        }
                        else
                        {
                            ps[i] = parameter.Type.GetDefault();
                        }
                    }
                }
                else if (jsonRpcRequest.ParamsObject is JArray array)
                {
                    var index = 0;
                    for (var i = 0; i < ps.Length; i++)
                    {
                        var parameter = rpcMethod.Parameters[i];
                        if (parameter.IsCallContext)
                        {
                            ps[i] = callContext;
                        }
                        else if (parameter.IsFromServices)
                        {
                            ps[i] = callContext.Resolver.Resolve(parameter.Type);
                        }
                        else if (index < array.Count)
                        {
                            var str = array[index++].ToString();

                            ps[i] = this.ResultParseToType(str, parameter.Type);
                        }
                        else if (parameter.ParameterInfo.HasDefaultValue)
                        {
                            ps[i] = parameter.ParameterInfo.DefaultValue;
                        }
                        else
                        {
                            ps[i] = parameter.Type.GetDefault();
                        }
                    }
                }
                else
                {
                    throw new RpcException("未知参数类型");
                }

                callContext.SetParameters(ps);
            }
        }

        private async Task ResponseAsync(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
        {
            try
            {
                var response = new InternalJsonRpcWaitResult()
                {
                    ErrorCode = error.Code,
                    ErrorMessage = error.Message,
                    Id = callContext.JsonRpcId,
                    Result = this.SerializerConverter.Serialize(this, result)
                };

                var str = JsonConvert.SerializeObject(response, this.m_jsonRpcWaitResultConverter);
                using (var byteBlock = new ByteBlock(1024 * 64))
                {
                    byteBlock.WriteNormalString(str, this.Encoding);
                    await this.SendAction(byteBlock.Memory).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Debug(ex.Message);
            }
        }

        /// <summary>
        /// ResultParseToType
        /// </summary>
        /// <param name="result"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        private object ResultParseToType(string result, Type returnType)
        {
            if (returnType == default)
            {
                return default;
            }

            if (returnType.IsPrimitive || returnType == typeof(string))
            {
                return result.ParseToType(returnType);
            }

            return this.SerializerConverter.Deserialize(this, result, returnType);
        }

        private async Task ThisInvokeAsync(object obj)
        {
            var callContext = (JsonRpcCallContextBase)obj;
            try
            {
                var invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, new InvokeResult(InvokeStatus.Ready)).ConfigureAwait(false);

                if (!callContext.JsonRpcId.HasValue)
                {
                    return;
                }
                var error = GetJsonRpcError(invokeResult);
                await this.ResponseAsync(callContext, invokeResult.Result, error).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger?.Debug(ex.Message);
            }
            finally
            {
                callContext.Dispose();
            }
        }

        #region Class

        /// <summary>
        /// JsonRpcError
        /// </summary>
        private readonly struct JsonRpcError
        {
            public JsonRpcError(int code, string message)
            {
                this.Code = code;
                this.Message = message;
            }

            public int Code { get; }
            public string Message { get; }
        }

        #endregion Class

        private bool TryParseRequest(string str, out InternalJsonRpcRequest request)
        {
            var jsonRpcRequest = JsonConvert.DeserializeObject<InternalJsonRpcRequest>(str, this.m_jsonRpcRequestConverter);

            if (jsonRpcRequest == null)
            {
                request = default;
                return false;
            }
            request = jsonRpcRequest;
            return true;
        }

        private bool TryParseResponse(string str, out InternalJsonRpcWaitResult response)
        {
            var rpcWaitResult = JsonConvert.DeserializeObject<InternalJsonRpcWaitResult>(str, this.m_jsonRpcWaitResultConverter);

            if (rpcWaitResult == null)
            {
                response = default;
                return false;
            }
            response = rpcWaitResult;
            return true;
        }
    }
}