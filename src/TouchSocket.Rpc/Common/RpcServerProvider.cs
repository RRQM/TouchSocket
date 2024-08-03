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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcServerProvider
    /// </summary>
    public sealed class RpcServerProvider : IRpcServerProvider
    {
        private readonly IResolver m_containerProvider;
        private readonly ILog m_logger;
        private readonly RpcStore m_rpcStore;

        /// <summary>
        /// RpcServerProvider
        /// </summary>
        /// <param name="containerProvider"></param>
        /// <param name="logger"></param>
        /// <param name="rpcStore"></param>
        public RpcServerProvider(IResolver containerProvider, ILog logger, RpcStore rpcStore)
        {
            this.m_containerProvider = containerProvider;
            this.m_logger = logger;
            this.m_rpcStore = rpcStore;
        }

        /// <summary>
        /// 异步执行Rpc
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="callContext"></param>
        /// <returns></returns>
        public async Task<InvokeResult> ExecuteAsync(ICallContext callContext, object[] ps)
        {
            var invokeResult = new InvokeResult();
            var filters = callContext.RpcMethod.GetFilters();
            try
            {
                for (var i = 0; i < filters.Count; i++)
                {
                    invokeResult = await filters[i].ExecutingAsync(callContext, ps, invokeResult)
                        .ConfigureAwait(false);
                }

                if (invokeResult.Status != InvokeStatus.Ready)
                {
                    return invokeResult;
                }

                var rpcServer = this.GetRpcServer(callContext);

                //调用
                switch (callContext.RpcMethod.TaskType)
                {
                    case TaskReturnType.Task:
                        {
                            await ((Task)callContext.RpcMethod.Invoke(rpcServer, ps)).ConfigureAwait(false);
                        }
                        break;

                    case TaskReturnType.TaskObject:
                        {
                            invokeResult.Result = await callContext.RpcMethod.InvokeObjectAsync(rpcServer, ps)
                                .ConfigureAwait(false);
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
                for (var i = 0; i < filters.Count; i++)
                {
                    invokeResult = await filters[i].ExecutedAsync(callContext, ps, invokeResult,default)
                        .ConfigureAwait(false);
                }
            }
            catch (TargetInvocationException ex)
            {
                invokeResult.Status = InvokeStatus.InvocationException;
                invokeResult.Message = ex.InnerException != null ? "函数内部发生异常，信息：" + ex.InnerException.Message : "函数内部发生异常，信息：未知";
                for (var i = 0; i < filters.Count; i++)
                {
                    invokeResult = await filters[i].ExecutedAsync(callContext, ps, invokeResult, ex).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
                for (var i = 0; i < filters.Count; i++)
                {
                    invokeResult = await filters[i].ExecutedAsync(callContext, ps, invokeResult, ex).ConfigureAwait(false);
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
                var rpcServer = (IRpcServer)this.m_containerProvider.Resolve(callContext.RpcMethod.ServerFromType);
                if (rpcServer is ITransientRpcServer transientRpcServer)
                {
                    transientRpcServer.CallContext = callContext;
                }
                return rpcServer;
            }
            catch (Exception ex)
            {
                this.m_logger.Exception(ex);
                throw;
            }
        }
    }
}