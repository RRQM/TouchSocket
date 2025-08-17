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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// DmtpRpcActor 类，继承自 ConcurrentDictionary，并实现 IDmtpRpcActor 接口。
/// 该类用于管理远程过程调用(RPC)的上下文，通过关联任务和超时逻辑来实现。
/// </summary>
public class DmtpRpcActor : DisposableObject, IDmtpRpcActor
{
    private readonly ConcurrentDictionary<long, DmtpRpcCallContext> m_callContextDic = new ConcurrentDictionary<long, DmtpRpcCallContext>();

    /// <summary>
    /// 初始化DmtpRpcActor类的实例。
    /// </summary>
    /// <param name="dmtpActor">IDmtpActor接口的实现，提供Dmtp通信能力。</param>
    /// <param name="rpcServerProvider">IRpcServerProvider接口的实现，用于提供RPC服务。</param>
    /// <param name="m_resolver">IResolver接口的实现，用于解析服务提供者。</param>
    /// <param name="dispatcher"></param>
    public DmtpRpcActor(IDmtpActor dmtpActor, IRpcServerProvider rpcServerProvider, IResolver m_resolver, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext> dispatcher)
    {
        this.DmtpActor = dmtpActor;
        this.m_rpcServerProvider = rpcServerProvider;
        this.m_resolver = m_resolver;
        this.Dispatcher = dispatcher;
    }

    /// <inheritdoc/>
    public IDmtpActor DmtpActor { get; }

    /// <inheritdoc/>
    public IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext> Dispatcher { get; }

    /// <summary>
    /// 获取调用的函数
    /// </summary>
    public Func<string, RpcMethod> GetInvokeMethod { get; set; }

    /// <inheritdoc/>
    public ISerializationSelector SerializationSelector { get => this.m_serializationSelector; set => this.m_serializationSelector = value; }

    #region 字段

    private readonly IResolver m_resolver;
    private readonly IRpcServerProvider m_rpcServerProvider;
    private ushort m_cancelInvoke;
    private ushort m_invoke_Request;
    private ushort m_invoke_Response;
    private ISerializationSelector m_serializationSelector;

    #endregion 字段

    /// <summary>
    /// 处理收到的消息
    /// </summary>
    /// <param name="message">接收到的消息对象</param>
    /// <returns>返回一个异步任务，指示处理是否成功</returns>
    public async Task<bool> InputReceivedData(DmtpMessage message)
    {
        var reader = new BytesReader(message.Memory);

        if (message.ProtocolFlags == this.m_invoke_Request)
        {
            try
            {
                var rpcPackage = new DmtpRpcRequestPackage();

                rpcPackage.UnpackageRouter(ref reader);
                if (rpcPackage.Route && this.DmtpActor.AllowRoute)
                {
                    if (await this.DmtpActor.TryRouteAsync(new PackageRouterEventArgs(RouteType.Rpc, rpcPackage)).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                    {
                        if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                        {
                            await actor.SendAsync(this.m_invoke_Request, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            return true;
                        }
                        else
                        {
                            rpcPackage.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                        }
                    }
                    else
                    {
                        rpcPackage.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                    }

                    rpcPackage.SwitchId();
                    await this.DmtpActor.SendAsync(this.m_invoke_Response, rpcPackage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                else
                {
                    var rpcMethod = this.GetInvokeMethod.Invoke(rpcPackage.InvokeKey);
                    DmtpRpcCallContext callContext;
                    if (rpcMethod.Reenterable == false || this.Dispatcher.Reenterable == false)
                    {
                        //不可重入
                        callContext = new DmtpRpcCallContext(this.DmtpActor.Client, rpcMethod, rpcPackage, this.m_resolver);
                    }
                    else
                    {
                        callContext = new DmtpRpcCallContext(this.DmtpActor.Client, rpcMethod, rpcPackage, this.m_resolver.CreateScopedResolver());
                    }

                    rpcPackage.LoadInfo(callContext, this.m_serializationSelector);
                    rpcPackage.UnpackageBody(ref reader);
                    //await this.InvokeThisAsync(callContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    //await EasyTask.Run(this.InvokeThisAsync, callContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    await this.Dispatcher.Dispatcher(this.DmtpActor, callContext, this.InvokeThisAsync).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
            }
            return true;
        }
        else if (message.ProtocolFlags == this.m_invoke_Response)
        {
            try
            {
                var rpcPackage = new DmtpRpcResponsePackage();

                rpcPackage.UnpackageRouter(ref reader);
                if (this.DmtpActor.AllowRoute && rpcPackage.Route)
                {
                    if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                    {
                        await actor.SendAsync(this.m_invoke_Response, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
                else
                {
                    if (this.DmtpActor.WaitHandlePool.TryGetDataAsync(rpcPackage.Sign, out var waitDataAsync))
                    {
                        var sourcePackage = (DmtpRpcRequestPackage)waitDataAsync.PendingData;
                        rpcPackage.LoadInfo(sourcePackage.ReturnType, this.m_serializationSelector, sourcePackage.SerializationType);
                        rpcPackage.UnpackageBody(ref reader);

                        waitDataAsync.Set(rpcPackage);
                    }
                }
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
            }
            return true;
        }
        else if (message.ProtocolFlags == this.m_cancelInvoke)
        {
            try
            {
                var canceledPackage = new CanceledPackage();

                canceledPackage.UnpackageRouter(ref reader);
                if (this.DmtpActor.AllowRoute && canceledPackage.Route)
                {
                    if (await this.DmtpActor.TryFindDmtpActor(canceledPackage.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                    {
                        await actor.SendAsync(this.m_cancelInvoke, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
                else
                {
                    canceledPackage.UnpackageBody(ref reader);
                    if (this.m_callContextDic.TryGetValue(canceledPackage.Sign, out var context))
                    {
                        context.Cancel();
                    }
                }
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 设置处理协议标识的起始标识。
    /// </summary>
    /// <param name="start">起始标识值，将以此值为基准递增分配协议标识。</param>
    public void SetProtocolFlags(ushort start)
    {
        // 设置请求调用协议标识，基于起始值递增
        this.m_invoke_Request = start++;

        // 设置响应调用协议标识，基于上一个标识值递增
        this.m_invoke_Response = start++;

        // 设置取消调用协议标识，使用上一个递增后的值
        this.m_cancelInvoke = start;
    }

    private static void ThrowExceptionIfNotSetRunning(WaitDataStatus status)
    {
        switch (status)
        {
            case WaitDataStatus.Success:
                return;

            case WaitDataStatus.Canceled: throw new OperationCanceledException();
            case WaitDataStatus.Overtime: throw new TimeoutException();
            case WaitDataStatus.Disposed:
            case WaitDataStatus.Default:
            default:
                {
                    throw new UnknownErrorException();
                }
        }
    }

    private async Task CanceledInvokeAsync(CanceledPackage canceled)
    {
        using (var byteBlock = new ByteBlock(1024 * 64))
        {
            var block = byteBlock;
            canceled.Package(ref block);

            try
            {
                await this.DmtpActor.SendAsync(this.m_cancelInvoke, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch
            {
                //不关心异常
            }

        }
    }

    private async Task InvokeThisAsync(IDmtpRpcCallContext o)
    {
        var callContext = (DmtpRpcCallContext)o;
        var rpcRequestPackage = callContext.DmtpRpcPackage;
        callContext.SetParameters(rpcRequestPackage.Parameters);
        var rpcMethod = callContext.RpcMethod;

        try
        {
            DmtpRpcResponsePackage rpcResponsePackage;
            if (rpcRequestPackage.Feedback == FeedbackType.WaitSend)
            {
                //立即返回

                var returnByteBlock = new ValueByteBlock(1024);
                try
                {
                    rpcResponsePackage = new DmtpRpcResponsePackage(rpcRequestPackage, this.m_serializationSelector, null);

                    rpcResponsePackage.Package(ref returnByteBlock);

                    await this.DmtpActor.SendAsync(this.m_invoke_Response, returnByteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                finally
                {
                    returnByteBlock.Dispose();
                }
            }

            var invokeResult = new InvokeResult();
            if (rpcMethod == null)
            {
                invokeResult.Status = InvokeStatus.UnFound;
            }
            else
            {
                if (rpcRequestPackage.Feedback == FeedbackType.WaitInvoke)
                {
                    this.m_callContextDic.AddOrUpdate(rpcRequestPackage.Sign, callContext);
                }
            }

            invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, invokeResult).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            if (rpcRequestPackage.Feedback != FeedbackType.WaitInvoke)
            {
                //调用方不关心结果
                return;
            }
            else if (rpcMethod != null && rpcMethod.HasCallContext)
            {
                this.m_callContextDic.TryRemove(rpcRequestPackage.Sign, out _);
            }

            switch (invokeResult.Status)
            {
                case InvokeStatus.UnFound:
                    {
                        rpcResponsePackage = new DmtpRpcResponsePackage(rpcRequestPackage, this.m_serializationSelector, TouchSocketDmtpStatus.RpcMethodNotFind, default);
                        break;
                    }
                case InvokeStatus.Success:
                    {
                        rpcResponsePackage = new DmtpRpcResponsePackage(rpcRequestPackage, this.m_serializationSelector, invokeResult.Result);
                        break;
                    }
                case InvokeStatus.UnEnable:
                    {
                        rpcResponsePackage = new DmtpRpcResponsePackage(rpcRequestPackage, this.m_serializationSelector, TouchSocketDmtpStatus.RpcMethodDisable, default);
                        break;
                    }
                case InvokeStatus.InvocationException:
                    {
                        rpcResponsePackage = new DmtpRpcResponsePackage(rpcRequestPackage, this.m_serializationSelector, TouchSocketDmtpStatus.RpcInvokeException, invokeResult.Message);
                        break;
                    }
                case InvokeStatus.Exception:
                    {
                        rpcResponsePackage = new DmtpRpcResponsePackage(rpcRequestPackage, this.m_serializationSelector, TouchSocketDmtpStatus.Exception, invokeResult.Message);
                        break;
                    }
                default:
                    return;
            }


            var byteBlock = new ValueByteBlock(1024 * 64);
            try
            {
                rpcResponsePackage.Package(ref byteBlock);

                await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
        catch (Exception ex)
        {
            this.DmtpActor.Logger?.Exception(this, ex);
        }
        finally
        {
            callContext.SafeDispose();
        }
    }

    private async Task<DmtpRpcActor> TryFindDmtpRpcActor(string targetId)
    {
        if (targetId == this.DmtpActor.Id)
        {
            return this;
        }
        if (await this.DmtpActor.TryFindDmtpActor(targetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor dmtpActor)
        {
            if (dmtpActor.GetDmtpRpcActor() is DmtpRpcActor newActor)
            {
                return newActor;
            }
        }
        return default;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Dispatcher.SafeDispose();
        }
        base.Dispose(disposing);
    }

    #region Rpc

    /// <inheritdoc/>
    public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
    {
        invokeOption ??= InvokeOption.WaitInvoke;

        var rpcPackage = new DmtpRpcRequestPackage(invokeKey, invokeOption, parameters, returnType, this.m_serializationSelector)
        {
            SourceId = this.DmtpActor.Id
        };
        var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(rpcPackage);

        try
        {
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                rpcPackage.Package(ref byteBlock);

                await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        return returnType?.GetDefault();
                    }
                case FeedbackType.WaitSend:
                    {
                        ThrowExceptionIfNotSetRunning(await waitData.WaitAsync(invokeOption.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext));
                        return returnType?.GetDefault();
                    }
                case FeedbackType.WaitInvoke:
                    {
                        var waitDataStatus = await waitData.WaitAsync(invokeOption.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        if (waitDataStatus == WaitDataStatus.Canceled)
                        {
                            await this.CanceledInvokeAsync(new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                        ThrowExceptionIfNotSetRunning(waitDataStatus);
                        var resultRpcPackage = (DmtpRpcResponsePackage)waitData.CompletedData;
                        resultRpcPackage.ThrowStatus();
                        return resultRpcPackage.ReturnParameter;
                    }
                default:
                    throw new Exception();
            }
        }
        finally
        {
            waitData.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task<object> InvokeAsync(string targetId, string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        }

        if (this.DmtpActor.AllowRoute && this.TryFindDmtpRpcActor(targetId).GetFalseAwaitResult() is DmtpRpcActor actor)
        {
            return await actor.InvokeAsync(invokeKey, returnType, invokeOption, parameters);
        }

        invokeOption ??= InvokeOption.WaitInvoke;

        var rpcPackage = new DmtpRpcRequestPackage(invokeKey, invokeOption, parameters, returnType, this.m_serializationSelector)
        {
            SourceId = this.DmtpActor.Id,
            TargetId = targetId
        };

        var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(rpcPackage);

        if (invokeOption == default)
        {
            invokeOption = InvokeOption.WaitInvoke;
        }

        try
        {
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                rpcPackage.Package(ref byteBlock);

                await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        return returnType?.GetDefault();
                    }
                case FeedbackType.WaitSend:
                    {
                        ThrowExceptionIfNotSetRunning(await waitData.WaitAsync(invokeOption.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext));
                        return returnType?.GetDefault();
                    }
                case FeedbackType.WaitInvoke:
                    {
                        var waitDataStatus = await waitData.WaitAsync(invokeOption.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        if (waitDataStatus == WaitDataStatus.Canceled)
                        {
                            await this.CanceledInvokeAsync(new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }

                        ThrowExceptionIfNotSetRunning(waitDataStatus);

                        var resultRpcPackage = (DmtpRpcResponsePackage)waitData.CompletedData;
                        resultRpcPackage.ThrowStatus();

                        return resultRpcPackage.ReturnParameter;
                    }
                default:
                    throw new Exception();
            }
        }
        finally
        {
            waitData.Dispose();
        }
    }

    #endregion Rpc
}