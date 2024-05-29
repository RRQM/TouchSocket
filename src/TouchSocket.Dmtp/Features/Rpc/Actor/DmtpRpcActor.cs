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
        public SerializationSelector SerializationSelector { get; set; }

        #region 字段

        private readonly IResolver m_resolver;
        private readonly IRpcServerProvider m_rpcServerProvider;
        private ushort m_cancelInvoke;
        private ushort m_invoke_Request;
        private ushort m_invoke_Response;

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
                    var rpcPackage = new DmtpRpcPackage();

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
                        rpcPackage.UnpackageBody(ref byteBlock);
                        //await this.InvokeThis(rpcPackage);
                        //ThreadPool.QueueUserWorkItem(this.InvokeThis, rpcPackage);

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
                    var rpcPackage = new DmtpRpcPackage();
                   
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
                        rpcPackage.UnpackageBody(ref byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(rpcPackage);
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
                var rpcPackage = (DmtpRpcPackage)o;

                var psData = rpcPackage.ParametersBytes;
                if (rpcPackage.Feedback == FeedbackType.WaitSend)
                {
                    using (var returnByteBlock = new ByteBlock())
                    {
                        var methodName = rpcPackage.InvokeKey;
                        var parametersBytes = rpcPackage.ParametersBytes;

                        rpcPackage.SwitchId();
                        rpcPackage.InvokeKey = default;
                        rpcPackage.ParametersBytes = default;
                        rpcPackage.Status = TouchSocketDmtpStatus.Success.ToValue();

                        var block = returnByteBlock;
                        rpcPackage.Package(ref block);
                        
                        await this.DmtpActor.SendAsync(this.m_invoke_Response, returnByteBlock.Memory).ConfigureFalseAwait();

                        rpcPackage.SwitchId();
                        rpcPackage.InvokeKey = methodName;
                        rpcPackage.ParametersBytes = parametersBytes;
                    }
                }

                var invokeResult = new InvokeResult();
                object[] ps = null;
                var rpcMethod = this.GetInvokeMethod.Invoke(rpcPackage.InvokeKey);
                DmtpRpcCallContext callContext = null;
                if (rpcMethod != null)
                {
                    try
                    {
                        if (rpcMethod.IsEnable)
                        {
                            ps = new object[rpcMethod.Parameters.Length];

                            callContext = new DmtpRpcCallContext(this.DmtpActor.Client, rpcMethod, rpcPackage, this.m_resolver);

                            if (rpcPackage.Feedback == FeedbackType.WaitInvoke && rpcMethod.HasCallContext)
                            {
                                this.TryAdd(rpcPackage.Sign, callContext);
                            }

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
                                    ps[i] = this.m_resolver.Resolve(parameter.Type);
                                }
                                else if (index < psData.Count)
                                {
                                    ps[i] = this.SerializationSelector.DeserializeParameter(rpcPackage.SerializationType, psData[index++], rpcMethod.ParameterTypes[i]);
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
                            invokeResult.Status = InvokeStatus.UnEnable;
                        }
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.Message;
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnFound;
                }

                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, ps).ConfigureFalseAwait();
                    //invokeResult = this.m_rpcServerProvider.Execute(callContext, ps);
                }

                if (rpcPackage.Feedback == FeedbackType.OnlySend)
                {
                    return;
                }

                switch (invokeResult.Status)
                {
                    case InvokeStatus.UnFound:
                        {
                            rpcPackage.Status = TouchSocketDmtpStatus.RpcMethodNotFind.ToValue();
                            break;
                        }
                    case InvokeStatus.Success:
                        {
                            if (rpcMethod.HasReturn)
                            {
                                rpcPackage.ReturnParameterBytes = this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, invokeResult.Result);
                            }
                            else
                            {
                                rpcPackage.ReturnParameterBytes = null;
                            }

                            if (rpcMethod.HasByRef)
                            {
                                rpcPackage.IsByRef = true;
                                rpcPackage.ParametersBytes = new List<byte[]>();

                                for (var i = 0; i < rpcMethod.Parameters.Length; i++)
                                {
                                    var parameter = rpcMethod.Parameters[i];
                                    if (parameter.IsCallContext)
                                    {
                                        continue;
                                    }
                                    else if (parameter.IsFromServices)
                                    {
                                        continue;
                                    }
                                    else if (parameter.IsByRef)
                                    {
                                        rpcPackage.ParametersBytes.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, ps[i]));
                                    }
                                    else
                                    {
                                        rpcPackage.ParametersBytes.Add(null);
                                    }
                                }
                                //var i = 0;
                                //if (rpcMethod.IncludeCallContext)
                                //{
                                //    i = 1;
                                //}
                                //for (; i < ps.Length; i++)
                                //{
                                //    rpcPackage.ParametersBytes.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, ps[i]));
                                //}
                            }
                            else
                            {
                                rpcPackage.ParametersBytes = null;
                            }

                            rpcPackage.Status = TouchSocketDmtpStatus.Success.ToValue();
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            rpcPackage.Status = TouchSocketDmtpStatus.RpcMethodDisable.ToValue();
                            break;
                        }
                    case InvokeStatus.InvocationException:
                        {
                            rpcPackage.Status = TouchSocketDmtpStatus.RpcInvokeException.ToValue();
                            rpcPackage.Message = invokeResult.Message;
                            break;
                        }
                    case InvokeStatus.Exception:
                        {
                            rpcPackage.Status = TouchSocketDmtpStatus.Exception.ToValue();
                            rpcPackage.Message = invokeResult.Message;
                            break;
                        }
                    default:
                        return;
                }

                if (rpcPackage.Feedback == FeedbackType.WaitInvoke && rpcMethod.HasCallContext)
                {
                    this.TryRemove(rpcPackage.Sign, out _);
                }

                using (var byteBlock = new ByteBlock())
                {
                    rpcPackage.InvokeKey = default;
                    rpcPackage.SwitchId();

                    var block = byteBlock;
                    rpcPackage.Package(ref block);
                   
                    await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory).ConfigureFalseAwait();
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

        //#region Rpc

        ///// <inheritdoc/>
        //public object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        //{
        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        SourceId = this.DmtpActor.Id
        //    };
        //    var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
        //            }
        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            var datas = new List<byte[]>();
        //            foreach (var parameter in parameters)
        //            {
        //                datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //            }
        //            rpcPackage.ParametersBytes = datas;
        //            rpcPackage.Package(byteBlock);

        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                {
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    if (resultContext.IsByRef)
        //                    {
        //                        try
        //                        {
        //                            for (var i = 0; i < parameters.Length; i++)
        //                            {
        //                                parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
        //                            }
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            throw new Exception(e.Message);
        //                        }
        //                    }
        //                    return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
        //                }
        //            default:
        //                return returnType.GetDefault();
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public void Invoke(string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        //{
        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        SourceId = this.DmtpActor.Id
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            var datas = new List<byte[]>();
        //            foreach (var parameter in parameters)
        //            {
        //                datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //            }
        //            rpcPackage.ParametersBytes = datas;
        //            rpcPackage.Package(byteBlock);
        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                break;

        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    break;
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    if (resultContext.IsByRef)
        //                    {
        //                        for (var i = 0; i < parameters.Length; i++)
        //                        {
        //                            parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
        //                        }
        //                    }
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public void Invoke(string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        SourceId = this.DmtpActor.Id
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);
        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                break;

        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    break;
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        SourceId = this.DmtpActor.Id
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);

        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                {
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
        //                }

        //            default:
        //                return returnType.GetDefault();
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public async Task InvokeAsync(string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        SourceId = this.DmtpActor.Id
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);
        //            await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                break;

        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    break;
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public async Task<object> InvokeAsync(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        SourceId = this.DmtpActor.Id
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
        //            }
        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);

        //            await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                {
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
        //                }

        //            default:
        //                return returnType.GetDefault();
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        //#endregion Rpc

        //#region IdRpc

        ///// <inheritdoc/>
        //public void Invoke(string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }

        //    if (string.IsNullOrEmpty(invokeKey))
        //    {
        //        throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
        //    }

        //    if (this.DmtpActor.AllowRoute && this.TryFindDmtpRpcActor(targetId).GetFalseAwaitResult() is DmtpRpcActor actor)
        //    {
        //        actor.Invoke(invokeKey, invokeOption, parameters);
        //        return;
        //    }

        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        Route = true,
        //        TargetId = targetId,
        //        SourceId = this.DmtpActor.Id,
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);

        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                break;

        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    break;
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public object Invoke(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }

        //    if (string.IsNullOrEmpty(invokeKey))
        //    {
        //        throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
        //    }

        //    if (this.DmtpActor.AllowRoute && this.TryFindDmtpRpcActor(targetId).GetFalseAwaitResult() is DmtpRpcActor actor)
        //    {
        //        return actor.Invoke(returnType, invokeKey, invokeOption, parameters);
        //    }

        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        TargetId = targetId,
        //        SourceId = this.DmtpActor.Id,
        //        Route = true
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);
        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                {
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
        //                }

        //            default:
        //                return returnType.GetDefault();
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public void Invoke(string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }

        //    if (string.IsNullOrEmpty(invokeKey))
        //    {
        //        throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
        //    }

        //    if (this.DmtpActor.AllowRoute && this.TryFindDmtpRpcActor(targetId).GetFalseAwaitResult() is DmtpRpcActor actor)
        //    {
        //        actor.Invoke(invokeKey, invokeOption, ref parameters, types);
        //        return;
        //    }

        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        TargetId = targetId,
        //        SourceId = this.DmtpActor.Id,
        //        Route = true
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            var datas = new List<byte[]>();
        //            foreach (var parameter in parameters)
        //            {
        //                datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //            }
        //            rpcPackage.ParametersBytes = datas;
        //            rpcPackage.Package(byteBlock);

        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                {
        //                    return;
        //                }
        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    return;
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    if (resultContext.IsByRef)
        //                    {
        //                        for (var i = 0; i < parameters.Length; i++)
        //                        {
        //                            parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
        //                        }
        //                    }
        //                    return;
        //                }

        //            default:
        //                return;
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public object Invoke(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }

        //    if (string.IsNullOrEmpty(invokeKey))
        //    {
        //        throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
        //    }

        //    if (this.DmtpActor.AllowRoute && this.TryFindDmtpRpcActor(targetId).GetFalseAwaitResult() is DmtpRpcActor actor)
        //    {
        //        return actor.Invoke(returnType, invokeKey, invokeOption, ref parameters, types);
        //    }

        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        TargetId = targetId,
        //        SourceId = this.DmtpActor.Id,
        //        Route = true
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
        //            }
        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            var datas = new List<byte[]>();
        //            foreach (var parameter in parameters)
        //            {
        //                datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //            }
        //            rpcPackage.ParametersBytes = datas;
        //            rpcPackage.Package(byteBlock);
        //            this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len).GetFalseAwaitResult();
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                {
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(waitData.Wait(invokeOption.Timeout));
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    if (resultContext.IsByRef)
        //                    {
        //                        for (var i = 0; i < parameters.Length; i++)
        //                        {
        //                            parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
        //                        }
        //                    }
        //                    return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
        //                }

        //            default:
        //                return returnType.GetDefault();
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public async Task InvokeAsync(string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }

        //    if (string.IsNullOrEmpty(invokeKey))
        //    {
        //        throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
        //    }

        //    if (this.DmtpActor.AllowRoute && await this.TryFindDmtpRpcActor(targetId).ConfigureFalseAwait() is DmtpRpcActor actor)
        //    {
        //        await actor.InvokeAsync(invokeKey, invokeOption, parameters).ConfigureFalseAwait();
        //        return;
        //    }

        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        Route = true,
        //        TargetId = targetId,
        //        SourceId = this.DmtpActor.Id,
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetReverseWaitDataAsync(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
        //            }

        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);

        //            await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                break;

        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    break;
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}

        ///// <inheritdoc/>
        //public async Task<object> InvokeAsync(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }

        //    if (string.IsNullOrEmpty(invokeKey))
        //    {
        //        throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
        //    }

        //    if (this.DmtpActor.AllowRoute && await this.TryFindDmtpRpcActor(targetId).ConfigureFalseAwait() is DmtpRpcActor actor)
        //    {
        //        return await actor.InvokeAsync(returnType, invokeKey, invokeOption, parameters).ConfigureFalseAwait();
        //    }

        //    var rpcPackage = new DmtpRpcPackage
        //    {
        //        InvokeKey = invokeKey,
        //        TargetId = targetId,
        //        SourceId = this.DmtpActor.Id,
        //        Route = true
        //    };

        //    var waitData = this.DmtpActor.WaitHandlePool.GetReverseWaitDataAsync(rpcPackage);

        //    try
        //    {
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            if (invokeOption == default)
        //            {
        //                invokeOption = DmtpInvokeOption.WaitInvoke;
        //            }

        //            if (invokeOption.Token.CanBeCanceled)
        //            {
        //                waitData.SetCancellationToken(invokeOption.Token);
        //                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
        //            }
        //            rpcPackage.LoadInvokeOption(invokeOption);
        //            if (parameters != null)
        //            {
        //                var datas = new List<byte[]>();
        //                foreach (var parameter in parameters)
        //                {
        //                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
        //                }
        //                rpcPackage.ParametersBytes = datas;
        //            }

        //            rpcPackage.Package(byteBlock);

        //            await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
        //        }

        //        switch (invokeOption.FeedbackType)
        //        {
        //            case FeedbackType.OnlySend:
        //                {
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitSend:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    return returnType.GetDefault();
        //                }
        //            case FeedbackType.WaitInvoke:
        //                {
        //                    CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
        //                    var resultContext = (DmtpRpcPackage)waitData.WaitResult;
        //                    resultContext.ThrowStatus();
        //                    return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
        //                }

        //            default:
        //                return returnType.GetDefault();
        //        }
        //    }
        //    finally
        //    {
        //        this.DmtpActor.WaitHandlePool.Destroy(waitData);
        //    }
        //}
        //#endregion IdRpc


        #region Rpc
        public async Task<RpcResponse> InvokeAsync(RpcRequest request)
        {
            var rpcPackage = new DmtpRpcPackage
            {
                InvokeKey = request.InvokeKey,
                SourceId = this.DmtpActor.Id
            };
            var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(rpcPackage);
            var invokeOption = request.InvokeOption ?? InvokeOption.WaitInvoke;
            var returnType = request.ReturnType;

            try
            {
                using (var byteBlock = new ByteBlock())
                {
                    if (invokeOption.Token.CanBeCanceled)
                    {
                        waitData.SetCancellationToken(invokeOption.Token);
                        invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
                    }
                    rpcPackage.LoadInvokeOption(invokeOption);

                    var parameters = request.Parameters;
                    if (parameters != null && parameters.Length > 0)
                    {
                        var datas = new byte[parameters.Length][];
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            datas[i] = this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameters[i]);
                        }
                        rpcPackage.ParametersBytes = datas;
                    }

                    var block = byteBlock;
                    rpcPackage.Package(ref block);

                    await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory).ConfigureFalseAwait();
                }

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            return new RpcResponse(returnType.GetDefault(), default);
                        }
                    case FeedbackType.WaitSend:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            return new RpcResponse(returnType.GetDefault(), default);
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            var resultContext = (DmtpRpcPackage)waitData.WaitResult;
                            resultContext.ThrowStatus();

                            object[] parameters;
                            if (resultContext.IsByRef)
                            {
                                parameters = new object[resultContext.ParametersBytes.Count];
                                for (var i = 0; i < parameters.Length; i++)
                                {
                                    parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], request.ParameterTypes[i]);
                                }
                            }
                            else
                            {
                                parameters = default;
                            }
                            object returnValue;
                            if (returnType == null)
                            {
                                returnValue = default;
                            }
                            else
                            {
                                returnValue = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
                            }
                            return new RpcResponse(returnValue, parameters);

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

        public async Task<RpcResponse> InvokeAsync(string targetId, RpcRequest request)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (this.DmtpActor.AllowRoute && this.TryFindDmtpRpcActor(targetId).GetFalseAwaitResult() is DmtpRpcActor actor)
            {
                return await actor.InvokeAsync(request);
            }

            var rpcPackage = new DmtpRpcPackage
            {
                InvokeKey = request.InvokeKey,
                TargetId = targetId,
                SourceId = this.DmtpActor.Id,
                Route = true
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(rpcPackage);
            var invokeOption = request.InvokeOption ?? InvokeOption.WaitInvoke;
            var returnType = request.ReturnType;

            try
            {
                using (var byteBlock = new ByteBlock())
                {
                    if (invokeOption.Token.CanBeCanceled)
                    {
                        waitData.SetCancellationToken(invokeOption.Token);
                        invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.DmtpActor.Id, Sign = rpcPackage.Sign });
                    }
                    rpcPackage.LoadInvokeOption(invokeOption);
                    var parameters = request.Parameters;
                    if (parameters != null && parameters.Length > 0)
                    {
                        var datas = new byte[parameters.Length][];
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            datas[i] = this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameters[i]);
                        }
                        rpcPackage.ParametersBytes = datas;
                    }

                    var block = byteBlock;
                    rpcPackage.Package(ref block);

                    await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory).ConfigureFalseAwait();
                }

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            return new RpcResponse(returnType.GetDefault(), default);
                        }
                    case FeedbackType.WaitSend:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            return new RpcResponse(returnType.GetDefault(), default);
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            CheckWaitDataStatus(await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait());
                            var resultContext = (DmtpRpcPackage)waitData.WaitResult;
                            resultContext.ThrowStatus();

                            object[] parameters;
                            if (resultContext.IsByRef)
                            {
                                parameters = new object[resultContext.ParametersBytes.Count];
                                for (var i = 0; i < parameters.Length; i++)
                                {
                                    parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], request.ParameterTypes[i]);
                                }
                            }
                            else
                            {
                                parameters = default;
                            }
                            object returnValue;
                            if (returnType == null)
                            {
                                returnValue = default;
                            }
                            else
                            {
                                returnValue = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
                            }
                            return new RpcResponse(returnValue, parameters);

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

        #endregion
    }
}