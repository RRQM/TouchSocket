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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// DmtpRpcActor
    /// </summary>
    public class DmtpRpcActor : ConcurrentDictionary<long, DmtpRpcCallContext>, IDmtpRpcActor
    {
        /// <summary>
        /// 创建一个DmtpRpcActor
        /// </summary>
        /// <param name="dmtpActor"></param>
        /// <param name="rpcServerProvider"></param>
        /// <param name="m_resolver"></param>
        public DmtpRpcActor(IDmtpActor dmtpActor, IRpcServerProvider rpcServerProvider, IResolver m_resolver)
        {
            this.DmtpActor = dmtpActor;
            this.m_rpcServerProvider = rpcServerProvider;
            this.m_resolver = m_resolver;
        }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get; }

        /// <summary>
        /// 获取调用的函数
        /// </summary>
        public Func<string, RpcMethod> GetInvokeMethod { get; set; }

        /// <inheritdoc/>
        public ISerializationSelector SerializationSelector { get => m_serializationSelector; set => m_serializationSelector = value; }

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
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> InputReceivedData(DmtpMessage message)
        {
            var byteBlock = message.BodyByteBlock;

            if (message.ProtocolFlags == this.m_invoke_Request)
            {
                try
                {
                    var rpcPackage = new DmtpRpcRequestPackage();

                    rpcPackage.UnpackageRouter(ref byteBlock);
                    if (rpcPackage.Route && this.DmtpActor.AllowRoute)
                    {
                        if (await this.DmtpActor.TryRouteAsync(new PackageRouterEventArgs(RouteType.Rpc, rpcPackage)).ConfigureFalseAwait())
                        {
                            if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId).ConfigureFalseAwait() is DmtpActor actor)
                            {
                                await actor.SendAsync(this.m_invoke_Request, byteBlock.Memory).ConfigureFalseAwait();
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

                        byteBlock.Reset();
                        rpcPackage.SwitchId();

                        var block = byteBlock;
                        rpcPackage.Package(ref block);

                        await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory).ConfigureFalseAwait();
                    }
                    else
                    {
                        var rpcMethod = this.GetInvokeMethod.Invoke(rpcPackage.InvokeKey);
                        if (rpcMethod != null)
                        {
                            if (rpcMethod.IsEnable)
                            {
                                var callContext = new DmtpRpcCallContext(this.DmtpActor.Client, rpcMethod, rpcPackage, this.m_resolver);

                                rpcPackage.LoadInfo(callContext, this.m_serializationSelector);
                            }
                            else
                            {
                                rpcPackage.LoadInfo(rpcMethod);
                            }

                            rpcPackage.UnpackageBody(ref byteBlock);
                        }

                        //await this.InvokeThis(rpcPackage).ConfigureFalseAwait();
                         _ = Task.Factory.StartNew(this.InvokeThis, rpcPackage);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_invoke_Response)
            {
                try
                {
                    var rpcPackage = new DmtpRpcResponsePackage();

                    rpcPackage.UnpackageRouter(ref byteBlock);
                    if (this.DmtpActor.AllowRoute && rpcPackage.Route)
                    {
                        if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId).ConfigureFalseAwait() is DmtpActor actor)
                        {
                            await actor.SendAsync(this.m_invoke_Response, byteBlock.Memory).ConfigureFalseAwait();
                        }
                    }
                    else
                    {
                        if (this.DmtpActor.WaitHandlePool.TryGetDataAsync(rpcPackage.Sign, out var waitDataAsync))
                        {
                            var sourcePackage = (DmtpRpcRequestPackage)waitDataAsync.WaitResult;
                            rpcPackage.LoadInfo(sourcePackage.ReturnType, this.m_serializationSelector, sourcePackage.SerializationType);
                            rpcPackage.UnpackageBody(ref byteBlock);

                            waitDataAsync.Set(rpcPackage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_cancelInvoke)
            {
                try
                {
                    var canceledPackage = new CanceledPackage();

                    canceledPackage.UnpackageRouter(ref byteBlock);
                    if (this.DmtpActor.AllowRoute && canceledPackage.Route)
                    {
                        if (await this.DmtpActor.TryFindDmtpActor(canceledPackage.TargetId).ConfigureFalseAwait() is DmtpActor actor)
                        {
                            await actor.SendAsync(this.m_cancelInvoke, byteBlock.Memory).ConfigureFalseAwait();
                        }
                    }
                    else
                    {
                        canceledPackage.UnpackageBody(ref byteBlock);
                        if (this.TryGetValue(canceledPackage.Sign, out var context))
                        {
                            context.Cancel();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置处理协议标识的起始标识。
        /// </summary>
        /// <param name="start"></param>
        public void SetProtocolFlags(ushort start)
        {
            this.m_invoke_Request = start++;
            this.m_invoke_Response = start++;
            this.m_cancelInvoke = start;
        }

        private static void CheckWaitDataStatus(WaitDataStatus status)
        {
            switch (status)
            {
                case WaitDataStatus.SetRunning:
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

        private void CanceledInvoke(object obj)
        {
            if (obj is CanceledPackage canceled)
            {
                using (var byteBlock = new ByteBlock())
                {
                    var block = byteBlock;
                    canceled.Package(ref block);

                    this.DmtpActor.SendAsync(this.m_cancelInvoke, byteBlock.Memory).GetFalseAwaitResult();
                }
            }
        }

        private async Task InvokeThis(object o)
        {
            try
            {
                var rpcRequestPackage = (DmtpRpcRequestPackage)o;
                DmtpRpcResponsePackage rpcResponsePackage;

                var parameters = rpcRequestPackage.Parameters;
                var rpcMethod = rpcRequestPackage.RpcMethod;
                var callContext = rpcRequestPackage.CallContext;

                if (rpcRequestPackage.Feedback == FeedbackType.WaitSend)
                {
                    //立即返回

                    var returnByteBlock = new ValueByteBlock(1024);
                    try
                    {
                        rpcResponsePackage = new DmtpRpcResponsePackage(rpcRequestPackage, this.m_serializationSelector, null);

                        rpcResponsePackage.Package(ref returnByteBlock);

                        await this.DmtpActor.SendAsync(this.m_invoke_Response, returnByteBlock.Memory).ConfigureFalseAwait();
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
                    if (rpcMethod.IsEnable)
                    {
                        if (rpcRequestPackage.Feedback == FeedbackType.WaitInvoke && rpcMethod.HasCallContext)
                        {
                            this.TryAdd(rpcRequestPackage.Sign, callContext);
                        }
                    }
                    else
                    {
                        invokeResult.Status = InvokeStatus.UnEnable;
                    }
                }

                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, parameters).ConfigureFalseAwait();
                }

                if (rpcRequestPackage.Feedback != FeedbackType.WaitInvoke)
                {
                    //调用方不关心结果
                    return;
                }
                else if (rpcMethod != null && rpcMethod.HasCallContext)
                {
                    this.TryRemove(rpcRequestPackage.Sign, out _);
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

                    await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory).ConfigureFalseAwait();
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
            catch
            {
            }
        }

        private async Task<DmtpRpcActor> TryFindDmtpRpcActor(string targetId)
        {
            if (targetId == this.DmtpActor.Id)
            {
                return this;
            }
            if (await this.DmtpActor.TryFindDmtpActor(targetId).ConfigureFalseAwait() is DmtpActor dmtpActor)
            {
                if (dmtpActor.GetDmtpRpcActor() is DmtpRpcActor newActor)
                {
                    return newActor;
                }
            }
            return default;
        }

        #region Rpc

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
                if (invokeOption.Token.CanBeCanceled)
                {
                    waitData.SetCancellationToken(invokeOption.Token);
                    invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
                }

                var byteBlock = new ByteBlock();
                try
                {
                    rpcPackage.Package(ref byteBlock);

                    await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory).ConfigureFalseAwait();
                }
                finally
                {
                    byteBlock.Dispose();
                }

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            return returnType.GetDefault();
                        }
                    case FeedbackType.WaitSend:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            return returnType.GetDefault();
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            var resultRpcPackage = (DmtpRpcResponsePackage)waitData.WaitResult;
                            resultRpcPackage.ThrowStatus();
                            return resultRpcPackage.ReturnParameter;
                        }
                    default:
                        throw new Exception();
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

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
                if (invokeOption.Token.CanBeCanceled)
                {
                    waitData.SetCancellationToken(invokeOption.Token);
                    invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
                }

                var byteBlock = new ByteBlock();
                try
                {
                    rpcPackage.Package(ref byteBlock);

                    await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory).ConfigureFalseAwait();
                }
                finally
                {
                    byteBlock.Dispose();
                }

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            return returnType.GetDefault();
                        }
                    case FeedbackType.WaitSend:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            return returnType.GetDefault();
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            var resultRpcPackage = (DmtpRpcResponsePackage)waitData.WaitResult;
                            resultRpcPackage.ThrowStatus();

                            return resultRpcPackage.ReturnParameter;
                        }
                    default:
                        throw new Exception();
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        #endregion Rpc
    }
}