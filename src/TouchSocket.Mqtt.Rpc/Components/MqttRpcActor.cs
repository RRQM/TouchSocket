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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TouchSocket.Rpc;

namespace TouchSocket.Mqtt.Rpc;

/// <summary>
/// 表示一个 MqttRpcActor 类，用于处理 MqttRpc 请求和响应。
/// </summary>
public sealed class MqttRpcActor : DisposableObject, IMqttRpcClient
{
    private readonly MqttRpcConverter m_converter = new MqttRpcConverter();
    private readonly WaitHandlePool<MqttRpcWaitResult> m_waitHandle = new WaitHandlePool<MqttRpcWaitResult>();
    private IRpcServerProvider m_rpcServerProvider;

    /// <inheritdoc/>
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
    /// 获取或设置 RPC 调度器。
    /// </summary>
    public IRpcDispatcher<MqttRpcActor, IMqttRpcCallContext> RpcDispatcher { get; set; } = new ConcurrencyRpcDispatcher<MqttRpcActor, IMqttRpcCallContext>();

    /// <summary>
    /// 获取或设置发布请求的动作，参数为 (payload, cancellationToken)。
    /// </summary>
    public Func<ReadOnlyMemory<byte>, CancellationToken, Task> SendRequestAction { get; set; }

    /// <summary>
    /// 获取或设置发布响应的动作，参数为 (responseTopic, payload, cancellationToken)。
    /// </summary>
    public Func<string, ReadOnlyMemory<byte>, CancellationToken, Task> SendResponseAction { get; set; }

    /// <summary>
    /// 获取或设置此 Actor 的响应主题。
    /// </summary>
    public string ResponseTopic { get; set; }

    /// <summary>
    /// 将 RPC 添加到映射中。
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务器提供者。</param>
    /// <param name="actionMap">动作映射。</param>
    public static void AddRpcToMap(IRpcServerProvider rpcServerProvider, ActionMap actionMap)
    {
        ThrowHelper.ThrowIfNull(rpcServerProvider, nameof(rpcServerProvider));
        ThrowHelper.ThrowIfNull(actionMap, nameof(actionMap));

        foreach (var rpcMethod in rpcServerProvider.GetMethods())
        {
            if (rpcMethod.GetAttribute<MqttRpcAttribute>() is MqttRpcAttribute attribute)
            {
                actionMap.Add(attribute.GetInvokeKey(rpcMethod), rpcMethod);
            }
        }
    }

    /// <summary>
    /// 异步接收输入并处理请求或响应。
    /// </summary>
    /// <param name="memory">输入内存。</param>
    /// <param name="callContext">调用上下文（非空时表示服务端接收到请求）。</param>
    public async Task InputReceiveAsync(ReadOnlyMemory<byte> memory, MqttRpcCallContext callContext)
    {
        try
        {
            var span = memory.Span;

            if (callContext != null && this.m_converter.TryReadRequest(span, out var mqttRpcRequest))
            {
                callContext.RequestId = mqttRpcRequest.Id;
                callContext.ResponseTopic = mqttRpcRequest.Rt;

                this.ActionMap.TryGetRpcMethod(mqttRpcRequest.Method, out var rpcMethod);
                if (rpcMethod == null || rpcMethod.Reenterable == false || this.RpcDispatcher.Reenterable == false)
                {
                    callContext.SetResolver(this.Resolver);
                }
                else
                {
                    callContext.SetResolver(this.Resolver.CreateScopedResolver());
                }

                this.BuildRequestContext(callContext, mqttRpcRequest);

                await this.RpcDispatcher.Dispatcher(this, callContext, this.ThisInvokeAsync).ConfigureDefaultAwait();
            }
            else if (this.m_converter.TryReadResponse(span, out var waitResult))
            {
                waitResult.Status = 1;
                this.m_waitHandle.Set(waitResult);
            }
            else
            {
                throw new RpcException("无法解析的 MqttRpc 数据");
            }
        }
        catch (Exception ex)
        {
            this.Logger?.Debug(this, ex.Message);
        }
    }

    /// <summary>
    /// 异步调用 RPC 方法。
    /// </summary>
    /// <param name="invokeKey">调用键。</param>
    /// <param name="returnType">返回类型。</param>
    /// <param name="invokeOption">调用选项。</param>
    /// <param name="parameters">参数。</param>
    /// <returns>任务对象。</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "MqttRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "MqttRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "MqttRpc基础设施相信动态代码是有效的")]
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
                this.m_converter.WriteRequest(byteBlock, invokeKey, id, this.ResponseTopic, parameters, this.SerializerOptions);
                await this.SendRequestAction(byteBlock.Memory, cancellationToken).ConfigureDefaultAwait();
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    return default;
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
    /// 设置 RPC 服务器提供者。
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务器提供者。</param>
    public void SetRpcServerProvider(IRpcServerProvider rpcServerProvider)
    {
        AddRpcToMap(rpcServerProvider, this.ActionMap);
        this.m_rpcServerProvider = rpcServerProvider;
    }

    /// <summary>
    /// 设置 RPC 服务器提供者和动作映射。
    /// </summary>
    /// <param name="rpcServerProvider">RPC 服务器提供者。</param>
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

    private static MqttRpcError GetMqttRpcError(InvokeResult invokeResult)
    {
        return invokeResult.Status switch
        {
            InvokeStatus.Success => new MqttRpcError(0, string.Empty),
            InvokeStatus.UnFound => new MqttRpcError(-1, "函数未找到"),
            InvokeStatus.UnEnable => new MqttRpcError(-2, "函数已被禁用"),
            InvokeStatus.InvocationException => new MqttRpcError(-3, invokeResult.Message),
            _ => new MqttRpcError(-4, invokeResult.Message),
        };
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "MqttRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "MqttRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "MqttRpc基础设施相信动态代码是有效的")]
    private void BuildRequestContext(MqttRpcCallContext callContext, InternalMqttRpcRequest mqttRpcRequest)
    {
        if (this.ActionMap.TryGetRpcMethod(mqttRpcRequest.Method, out var rpcMethod))
        {
            callContext.SetRpcMethod(rpcMethod);
            var ps = new object[rpcMethod.Parameters.Length];

            if (mqttRpcRequest.Params == null)
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
                    else if (parameter.Type == typeof(CancellationToken))
                    {
                        ps[i] = callContext.Token;
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
            else if (mqttRpcRequest.Params is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Array)
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
                        else if (parameter.Type == typeof(CancellationToken))
                        {
                            ps[i] = callContext.Token;
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
                else if (element.ValueKind == JsonValueKind.Object)
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
                        else if (parameter.Type == typeof(CancellationToken))
                        {
                            ps[i] = callContext.Token;
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
                else
                {
                    throw new RpcException("未知参数类型");
                }
            }

            callContext.SetParameters(ps);
        }
    }

    private async Task ResponseAsync(MqttRpcCallContext callContext, object result, MqttRpcError error)
    {
        try
        {
            var responseTopic = callContext.ResponseTopic;
            if (string.IsNullOrEmpty(responseTopic))
            {
                return;
            }

            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                this.m_converter.WriteResponse(byteBlock, callContext.RequestId, result, error.Code, error.Message, this.SerializerOptions);
                await this.SendResponseAction(responseTopic, byteBlock.Memory, CancellationToken.None).ConfigureDefaultAwait();
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "MqttRpc基础设施相信动态代码是有效的")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "MqttRpc基础设施相信动态代码是有效的")]
    private object ResultParseToType(string result, Type returnType)
    {
        if (returnType == default)
        {
            return default;
        }
        return JsonSerializer.Deserialize(result, returnType, this.SerializerOptions);
    }

    private async Task ThisInvokeAsync(object obj)
    {
        var callContext = (MqttRpcCallContext)obj;
        try
        {
            var invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, new InvokeResult(InvokeStatus.Ready)).ConfigureDefaultAwait();

            if (!callContext.RequestId.HasValue)
            {
                return;
            }
            var error = GetMqttRpcError(invokeResult);
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

    private readonly struct MqttRpcError
    {
        public MqttRpcError(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public int Code { get; }
        public string Message { get; }
    }
}
