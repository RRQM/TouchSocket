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

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 表示一个JsonRpcActor类，用于处理JsonRpc请求和响应。
/// </summary>
public sealed class JsonRpcActor : DisposableObject, IJsonRpcClient
{
    private readonly JsonRpcConverter m_jsonRpcConverter = new JsonRpcConverter();
    private readonly WaitHandlePool<JsonRpcWaitResult> m_waitHandle = new();
    private IRpcServerProvider m_rpcServerProvider;

    /// <summary>
    /// 获取或设置用于参数与返回值序列化的 <see cref="JsonSerializerOptions"/>。
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; } = new JsonSerializerOptions();

    /// <summary>
    /// 获取或设置动作映射。
    /// </summary>
    public ActionMap ActionMap { get; private set; } = new ActionMap(true);

    /// <summary>
    /// 获取或设置日志记录器。
    /// </summary>
    public ILog Logger { get; set; }

    /// <summary>
    /// 获取或设置解析器。
    /// </summary>
    public IResolver Resolver { get; set; }

    /// <summary>
    /// 获取或设置RPC调度器。
    /// </summary>
    public IRpcDispatcher<JsonRpcActor, IJsonRpcCallContext> RpcDispatcher { get; set; } = new ConcurrencyRpcDispatcher<JsonRpcActor, IJsonRpcCallContext>();

    /// <summary>
    /// 获取或设置发送动作。
    /// </summary>
    public Func<ReadOnlyMemory<byte>, CancellationToken, Task> SendAction { get; set; }

    /// <summary>
    /// 获取或设置序列化转换器。
    /// </summary>
    public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter { get; set; }

    /// <summary>
    /// 将RPC添加到映射中。
    /// </summary>
    /// <param name="rpcServerProvider">RPC服务器提供者。</param>
    /// <param name="actionMap">动作映射。</param>
    public static void AddRpcToMap(IRpcServerProvider rpcServerProvider, ActionMap actionMap)
    {
        ThrowHelper.ThrowIfNull(rpcServerProvider, nameof(rpcServerProvider));
        ThrowHelper.ThrowIfNull(actionMap, nameof(actionMap));

        foreach (var rpcMethod in rpcServerProvider.GetMethods())
        {
            if (rpcMethod.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
            {
                actionMap.Add(attribute.GetInvokeKey(rpcMethod), rpcMethod);
            }
        }
    }

    /// <summary>
    /// 异步接收输入。
    /// </summary>
    /// <param name="memory">输入内存。</param>
    /// <param name="callContext">调用上下文。</param>
    /// <returns>任务。</returns>
    public async Task InputReceiveAsync(ReadOnlyMemory<byte> memory, JsonRpcCallContextBase callContext)
    {
        try
        {
            var span = memory.Span;

            if (callContext != null && this.m_jsonRpcConverter.TryReadRequest(span, out var jsonRpcRequest))
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

                await this.RpcDispatcher.Dispatcher(this, callContext, this.ThisInvokeAsync).ConfigureDefaultAwait();
            }
            else if (this.m_jsonRpcConverter.TryReadResponse(span, out var internalJsonRpcWaitResult))
            {
                internalJsonRpcWaitResult.Status = 1;
                this.m_waitHandle.Set(internalJsonRpcWaitResult);
            }
            else
            {
                //throw new RpcException(Resources.TouchSocketCoreResource.JsonRpcParseError);
                throw new RpcException("无法解析的JsonRpc数据");
            }
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex.Message);
        }
    }

    /// <summary>
    /// 异步调用。
    /// </summary>
    /// <param name="invokeKey">调用键。</param>
    /// <param name="returnType">返回类型。</param>
    /// <param name="invokeOption">调用选项。</param>
    /// <param name="parameters">参数。</param>
    /// <returns>任务对象。</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "JsonRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JsonRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonRpc基础设施相信动态代码是有效的")]
    public async Task<object> InvokeAsync(string invokeKey, Type returnType, InvokeOption invokeOption, params object[] parameters)
    {
        var waitData = this.m_waitHandle.GetWaitDataAsync(out var sign);
        invokeOption ??= InvokeOption.WaitInvoke;

        parameters ??= [];

        var cancellationToken = invokeOption.Token;
        CancellationTokenSource cts = default;
        if (!cancellationToken.CanBeCanceled)
        {
            cts = new CancellationTokenSource(invokeOption.Timeout);
            cancellationToken = cts.Token;
        }

        var id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? sign : (int?)0;

        try
        {
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                this.m_jsonRpcConverter.WriteRequest(byteBlock, invokeKey, id, parameters, this.SerializerOptions);
                await this.SendAction(byteBlock.Memory, cancellationToken).ConfigureDefaultAwait();
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
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                default:
                    {
                        switch (await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait())
                        {
                            case WaitDataStatus.Success:
                                {
                                    var resultContext = waitData.CompletedData;
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
            waitData.Dispose();
            cts?.Dispose();
        }
    }

    /// <summary>
    /// 设置RPC服务器提供者。
    /// </summary>
    /// <param name="rpcServerProvider">RPC服务器提供者。</param>
    public void SetRpcServerProvider(IRpcServerProvider rpcServerProvider)
    {
        AddRpcToMap(rpcServerProvider, this.ActionMap);
        this.m_rpcServerProvider = rpcServerProvider;
    }

    /// <summary>
    /// 设置RPC服务器提供者和动作映射。
    /// </summary>
    /// <param name="rpcServerProvider">RPC服务器提供者。</param>
    /// <param name="actionMap">动作映射。</param>
    public void SetRpcServerProvider(IRpcServerProvider rpcServerProvider, ActionMap actionMap)
    {
        ThrowHelper.ThrowIfNull(rpcServerProvider, nameof(rpcServerProvider));
        ThrowHelper.ThrowIfNull(actionMap, nameof(actionMap));
        this.m_rpcServerProvider = rpcServerProvider;
        this.ActionMap = actionMap;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.RpcDispatcher.SafeDispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 获取JsonRpc错误。
    /// </summary>
    /// <param name="invokeResult">调用结果。</param>
    /// <returns>JsonRpc错误。</returns>
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

    /// <summary>
    /// 构建请求上下文。
    /// </summary>
    /// <param name="callContext">调用上下文。</param>
    /// <param name="jsonRpcRequest">JsonRpc请求。</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JsonRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "JsonRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonRpc基础设施相信动态代码是有效的")]
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
            if (jsonRpcRequest.ParamsObject is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object)
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
                        else if (element.TryGetProperty(parameter.Name, out var property))
                        {
                            ps[i] = property.Deserialize(parameter.Type, this.SerializerOptions);
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
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    var index = 0;
                    var arrayLength = element.GetArrayLength();
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
                        else if (index < arrayLength)
                        {
                            ps[i] = element[index++].Deserialize(parameter.Type, this.SerializerOptions);
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
            }
            else
            {
                throw new RpcException("未知参数类型");
            }

            callContext.SetParameters(ps);
        }
    }

    /// <summary>
    /// 异步响应。
    /// </summary>
    /// <param name="callContext">调用上下文。</param>
    /// <param name="result">结果。</param>
    /// <param name="error">错误。</param>
    /// <returns>任务。</returns>
    private async Task ResponseAsync(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
    {
        try
        {
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                this.m_jsonRpcConverter.WriteResponse(byteBlock, callContext.JsonRpcId, result, error.Code, error.Message, this.SerializerOptions);
                await this.SendAction(byteBlock.Memory, CancellationToken.None).ConfigureDefaultAwait();
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex.Message);
        }
    }

    /// <summary>
    /// 结果解析为类型。
    /// </summary>
    /// <param name="result">结果。</param>
    /// <param name="returnType">返回类型。</param>
    /// <returns>解析后的对象。</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JsonRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonRpc基础设施相信动态代码是有效的")]
    private object ResultParseToType(string result, Type returnType)
    {
        if (returnType == default)
        {
            return default;
        }

        //if (returnType.IsPrimitive || returnType == typeof(string))
        //{
        //    return result.ParseToType(returnType);
        //}

        return JsonSerializer.Deserialize(result, returnType, this.SerializerOptions);
    }

    /// <summary>
    /// 异步调用。
    /// </summary>
    /// <param name="obj">对象。</param>
    /// <returns>任务。</returns>
    private async Task ThisInvokeAsync(object obj)
    {
        var callContext = (JsonRpcCallContextBase)obj;
        try
        {
            var invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, new InvokeResult(InvokeStatus.Ready)).ConfigureDefaultAwait();

            if (!callContext.JsonRpcId.HasValue)
            {
                return;
            }
            var error = GetJsonRpcError(invokeResult);
            await this.ResponseAsync(callContext, invokeResult.Result, error).ConfigureDefaultAwait();
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex.Message);
        }
        finally
        {
            callContext.Dispose();
        }
    }

    #region Class

    /// <summary>
    /// JsonRpc错误。
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

}