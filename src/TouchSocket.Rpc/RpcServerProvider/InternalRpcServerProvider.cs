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
using System.Reflection;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc;

internal sealed class InternalRpcServerProvider : IRpcServerProvider
{
    private readonly ILog m_logger;
    private readonly RpcStore m_rpcStore;

    public InternalRpcServerProvider(ILog logger, RpcStore rpcStore)
    {
        this.m_logger = logger;
        this.m_rpcStore = rpcStore;
    }

    public async Task<InvokeResult> ExecuteAsync(ICallContext callContext, InvokeResult invokeResult)
    {
        var rpcCallContextAccessor=callContext.Resolver.Resolve<IRpcCallContextAccessor>();
        if (rpcCallContextAccessor is not null)
        {
            rpcCallContextAccessor.CallContext = callContext;
        }
        var ps = callContext.Parameters;
        var rpcMethod = callContext.RpcMethod;
        if (rpcMethod is null)
        {
            return new InvokeResult(InvokeStatus.UnFound);
        }

        var filters = callContext.RpcMethod.GetFilters();
        try
        {
            for (var i = 0; i < filters.Count; i++)
            {
                invokeResult = await filters[i].ExecutingAsync(callContext, ps, invokeResult)
                    .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            if (invokeResult.Status == InvokeStatus.Ready)
            {
                var rpcServer = this.GetRpcServer(callContext);

                //调用
                switch (callContext.RpcMethod.TaskType)
                {
                    case TaskReturnType.Task:
                        {
                            await ((Task)callContext.RpcMethod.Invoke(rpcServer, ps)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                        break;

                    case TaskReturnType.TaskObject:
                        {
                            invokeResult.Result = await callContext.RpcMethod.InvokeObjectAsync(rpcServer, ps)
                                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                        break;

                    default:
                    case TaskReturnType.None:
                        {
                            if (callContext.RpcMethod.HasReturn)
                            {
                                invokeResult.Result = callContext.RpcMethod.Invoke(rpcServer, ps);
                            }
                            else
                            {
                                callContext.RpcMethod.Invoke(rpcServer, ps);
                            }
                        }
                        break;
                }

                invokeResult.Status = InvokeStatus.Success;
            }
        }
        catch (TargetInvocationException ex)
        {
            invokeResult.Status = InvokeStatus.InvocationException;
            invokeResult.Message = ex.InnerException != null ? "函数内部发生异常，信息：" + ex.InnerException.Message : "函数内部发生异常，信息：未知";
            invokeResult.Exception = ex.InnerException;
        }
        catch (Exception ex)
        {
            invokeResult.Status = InvokeStatus.Exception;
            invokeResult.Message = ex.Message;
            invokeResult.Exception = ex;
        }
        finally
        {
            for (var i = 0; i < filters.Count; i++)
            {
                invokeResult = await filters[i].ExecutedAsync(callContext, ps, invokeResult, invokeResult.Exception)
                    .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }

            if (rpcCallContextAccessor is not null)
            {
                rpcCallContextAccessor.CallContext = default;
            }
        }

        return invokeResult;
    }

    /// <inheritdoc/>
    public RpcMethod[] GetMethods()
    {
        return this.m_rpcStore.GetAllMethods();
    }

    private object GetRpcServer(ICallContext callContext)
    {
        try
        {
            var rpcServer = callContext.Resolver.Resolve(callContext.RpcMethod.ServerFromType);
            if (rpcServer is ITransientRpcServer transientRpcServer)
            {
                transientRpcServer.CallContext = callContext;
            }
            else if (rpcServer is IScopedRpcServer scopedRpcServer)
            {
                scopedRpcServer.CallContext = callContext;
            }
            return rpcServer;
        }
        catch (Exception ex)
        {
            this.m_logger?.Exception(ex);
            throw;
        }
    }
}