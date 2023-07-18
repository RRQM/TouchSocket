//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.Smtp.Rpc
{
    /// <summary>
    /// SmtpRpcActor
    /// </summary>
    public class SmtpRpcActor : ConcurrentDictionary<long, SmtpRpcCallContext>, ISmtpRpcActor
    {
       
        /// <summary>
        /// 创建一个SmtpRpcActor
        /// </summary>
        /// <param name="smtpActor"></param>
        public SmtpRpcActor(ISmtpActor smtpActor)
        {
            this.SmtpActor = smtpActor;
        }

        /// <summary>
        /// 获取调用的函数
        /// </summary>
        public Func<string, MethodInstance> GetInvokeMethod { get; set; }

        /// <inheritdoc/>
        public RpcStore RpcStore { get; set; }

        /// <inheritdoc/>
        public SerializationSelector SerializationSelector { get; set; }

        /// <inheritdoc/>
        public ISmtpActor SmtpActor { get; }
        #region 字段

        private ushort m_cancelInvoke;
        private ushort m_invoke_Request;
        private ushort m_invoke_Response;
        
        #endregion 字段

        /// <summary>
        /// 处理收到的消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool InputReceivedData(SmtpMessage message)
        {
            var byteBlock = message.BodyByteBlock;
            
            if (message.ProtocolFlags == this.m_invoke_Request)
            {
                try
                {
                    var rpcPackage = new SmtpRpcPackage();
                    rpcPackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && rpcPackage.Route)
                    {
                        if (this.SmtpActor.TryRoute(RouteType.Rpc, rpcPackage))
                        {
                            if (this.SmtpActor.TryFindSmtpActor(rpcPackage.TargetId, out var actor))
                            {
                                actor.Send(this.m_invoke_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                rpcPackage.UnpackageBody(byteBlock);
                                rpcPackage.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            rpcPackage.UnpackageBody(byteBlock);
                            rpcPackage.Status = TouchSocketSmtpStatus.RoutingNotAllowed.ToValue();
                        }

                        byteBlock.Reset();
                        rpcPackage.SwitchId();

                        rpcPackage.Package(byteBlock);
                        this.SmtpActor.Send(this.m_invoke_Response, byteBlock);
                    }
                    else
                    {
                        rpcPackage.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.InvokeThis, rpcPackage);
                        //this.InvokeThis(rpcPackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_invoke_Response)
            {
                try
                {
                    var rpcPackage = new SmtpRpcPackage();
                    rpcPackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && rpcPackage.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(rpcPackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_invoke_Response, byteBlock);
                        }
                    }
                    else
                    {
                        rpcPackage.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(rpcPackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_cancelInvoke)
            {
                try
                {
                    var canceledPackage = new CanceledPackage();
                    canceledPackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && canceledPackage.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(canceledPackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_cancelInvoke, byteBlock);
                        }
                    }
                    else
                    {
                        canceledPackage.UnpackageBody(byteBlock);
                        if (this.TryGetValue(canceledPackage.Sign, out var context))
                        {
                            context.TokenSource.Cancel();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
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

        private void CanceledInvoke(object obj)
        {
            if (obj is CanceledPackage canceled)
            {
                using (var byteBlock = new ByteBlock())
                {
                    canceled.Package(byteBlock);
                    this.SmtpActor.Send(this.m_cancelInvoke, byteBlock);
                }
            }
        }

        private void InvokeThis(object o)
        {
            try
            {
                var rpcPackage = (SmtpRpcPackage)o;

                var psData = rpcPackage.ParametersBytes;
                if (rpcPackage.Feedback == FeedbackType.WaitSend)
                {
                    using (var returnByteBlock = new ByteBlock())
                    {
                        var methodName = rpcPackage.MethodName;
                        var parametersBytes = rpcPackage.ParametersBytes;

                        rpcPackage.SwitchId();
                        rpcPackage.MethodName = default;
                        rpcPackage.ParametersBytes = default;
                        rpcPackage.Status = TouchSocketSmtpStatus.Success.ToValue();
                        rpcPackage.Package(returnByteBlock);
                        this.SmtpActor.Send(this.m_invoke_Response, returnByteBlock);

                        rpcPackage.SwitchId();
                        rpcPackage.MethodName = methodName;
                        rpcPackage.ParametersBytes = parametersBytes;
                    }
                }

                var invokeResult = new InvokeResult();
                object[] ps = null;
                var methodInstance = this.GetInvokeMethod?.Invoke(rpcPackage.MethodName);
                SmtpRpcCallContext callContext = null;
                if (methodInstance != null)
                {
                    try
                    {
                        if (methodInstance.IsEnable)
                        {
                            callContext = new SmtpRpcCallContext()
                            {
                                Caller = this.SmtpActor.Client,
                                MethodInstance = methodInstance,
                                SmtpRpcPackage = rpcPackage
                            };

                            this.TryAdd(rpcPackage.Sign, callContext);

                            if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                            {
                                ps = new object[methodInstance.ParameterTypes.Length];
                                ps[0] = callContext;
                                for (var i = 0; i < psData.Count; i++)
                                {
                                    ps[i + 1] = this.SerializationSelector.DeserializeParameter(rpcPackage.SerializationType, psData[i], methodInstance.ParameterTypes[i + 1]);
                                }
                            }
                            else
                            {
                                ps = new object[methodInstance.ParameterTypes.Length];
                                for (var i = 0; i < methodInstance.ParameterTypes.Length; i++)
                                {
                                    ps[i] = this.SerializationSelector.DeserializeParameter(rpcPackage.SerializationType, psData[i], methodInstance.ParameterTypes[i]);
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
                    var rpcServer = methodInstance.ServerFactory.Create(callContext, ps);
                    if (rpcServer is ITransientRpcServer transientRpcServer)
                    {
                        transientRpcServer.CallContext = callContext;
                    }
                    invokeResult =RpcStore.Execute(rpcServer, ps, callContext);
                }

                if (rpcPackage.Feedback == FeedbackType.OnlySend)
                {
                    return;
                }

                switch (invokeResult.Status)
                {
                    case InvokeStatus.UnFound:
                        {
                            rpcPackage.Status = TouchSocketSmtpStatus.RpcMethodNotFind.ToValue();
                            break;
                        }
                    case InvokeStatus.Success:
                        {
                            rpcPackage.ReturnParameterBytes = methodInstance.HasReturn
                                ? this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, invokeResult.Result)
                                : null;

                            if (methodInstance.IsByRef)
                            {
                                rpcPackage.IsByRef = true;
                                rpcPackage.ParametersBytes = new List<byte[]>();

                                var i = 0;
                                if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                                {
                                    i = 1;
                                }
                                for (; i < ps.Length; i++)
                                {
                                    rpcPackage.ParametersBytes.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, ps[i]));
                                }
                            }
                            else
                            {
                                rpcPackage.ParametersBytes = null;
                            }

                            rpcPackage.Status = TouchSocketSmtpStatus.Success.ToValue();
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            rpcPackage.Status = TouchSocketSmtpStatus.RpcMethodDisable.ToValue();
                            break;
                        }
                    case InvokeStatus.InvocationException:
                        {
                            rpcPackage.Status = TouchSocketSmtpStatus.RpcInvokeException.ToValue();
                            rpcPackage.Message = invokeResult.Message;
                            break;
                        }
                    case InvokeStatus.Exception:
                        {
                            rpcPackage.Status = TouchSocketSmtpStatus.Exception.ToValue();
                            rpcPackage.Message = invokeResult.Message;
                            break;
                        }
                    default:
                        return;
                }

                this.TryRemove(rpcPackage.Sign, out _);

                using (var byteBlock = new ByteBlock())
                {
                    rpcPackage.MethodName = default;
                    rpcPackage.SwitchId();
                    rpcPackage.Package(byteBlock);
                    this.SmtpActor.Send(this.m_invoke_Response, byteBlock);
                }
            }
            catch
            {
            }
        }

        private bool TryFindSmtpRpcActor(string targetId, out SmtpRpcActor rpcActor)
        {
            if (targetId == this.SmtpActor.Id)
            {
                rpcActor = this;
                return true;
            }
            if (this.SmtpActor.TryFindSmtpActor(targetId, out var smtpActor))
            {
                if (smtpActor.GetSmtpRpcActor() is SmtpRpcActor newActor)
                {
                    rpcActor = newActor;
                    return true;
                }
            }

            rpcActor = default;
            return false;
        }

        #region Rpc

        /// <inheritdoc/>
        public object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                SourceId = this.SmtpActor.Id
            };
            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, Sign = rpcPackage.Sign });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                var datas = new List<byte[]>();
                foreach (var parameter in parameters)
                {
                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                }
                rpcPackage.ParametersBytes = datas;
                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.IsByRef)
                                        {
                                            try
                                            {
                                                for (var i = 0; i < parameters.Length; i++)
                                                {
                                                    parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                throw new Exception(e.Message);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    default:
                        return default;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Invoke(string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                SourceId = this.SmtpActor.Id
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, Sign = rpcPackage.Sign });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                var datas = new List<byte[]>();
                foreach (var parameter in parameters)
                {
                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                }
                rpcPackage.ParametersBytes = datas;
                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.IsByRef)
                                        {
                                            for (var i = 0; i < parameters.Length; i++)
                                            {
                                                parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Invoke(string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                SourceId = this.SmtpActor.Id
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, Sign = rpcPackage.Sign });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    var datas = new List<byte[]>();
                    foreach (var parameter in parameters)
                    {
                        datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                    }
                    rpcPackage.ParametersBytes = datas;
                }

                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    break;

                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                SourceId = this.SmtpActor.Id
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, Sign = rpcPackage.Sign });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    var datas = new List<byte[]>();
                    foreach (var parameter in parameters)
                    {
                        datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                    }
                    rpcPackage.ParametersBytes = datas;
                }

                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }

                    default:
                        return default;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public Task InvokeAsync(string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(invokeKey, invokeOption, parameters);
            });
        }

        /// <inheritdoc/>
        public Task<object> InvokeAsync(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke(returnType,invokeKey, invokeOption, parameters);
            });
        }

        #endregion Rpc

        #region IdRpc

        /// <inheritdoc/>
        public void Invoke(string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (string.IsNullOrEmpty(invokeKey))
            {
                throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
            }

            if (this.SmtpActor.AllowRoute&& this.TryFindSmtpRpcActor(targetId, out var actor))
            {
                actor.Invoke(invokeKey, invokeOption, parameters);
                return;
            }

            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                Route = true,
                TargetId = targetId,
                SourceId = this.SmtpActor.Id,
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    var datas = new List<byte[]>();
                    foreach (var parameter in parameters)
                    {
                        datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                    }
                    rpcPackage.ParametersBytes = datas;
                }

                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    break;

                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (string.IsNullOrEmpty(invokeKey))
            {
                throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
            }

            if (this.SmtpActor.AllowRoute&& this.TryFindSmtpRpcActor(targetId, out var rpcActor))
            {
                return rpcActor.Invoke(returnType,invokeKey, invokeOption, parameters);
            }

            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                TargetId = targetId,
                SourceId = this.SmtpActor.Id,
                Route = true
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    var datas = new List<byte[]>();
                    foreach (var parameter in parameters)
                    {
                        datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                    }
                    rpcPackage.ParametersBytes = datas;
                }

                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }

                    default:
                        return default;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Invoke(string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (string.IsNullOrEmpty(invokeKey))
            {
                throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
            }

            if (this.SmtpActor.AllowRoute&& this.TryFindSmtpRpcActor(targetId, out var rpcActor))
            {
                rpcActor.Invoke(invokeKey, invokeOption, ref parameters, types);
                return;
            }

            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                TargetId = targetId,
                SourceId = this.SmtpActor.Id,
                Route = true
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                var datas = new List<byte[]>();
                foreach (var parameter in parameters)
                {
                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                }
                rpcPackage.ParametersBytes = datas;
                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.IsByRef)
                                        {
                                            for (var i = 0; i < parameters.Length; i++)
                                            {
                                                parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return;
                        }

                    default:
                        return;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType,string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (string.IsNullOrEmpty(invokeKey))
            {
                throw new ArgumentException($"“{nameof(invokeKey)}”不能为 null 或空。", nameof(invokeKey));
            }

            if (this.SmtpActor.AllowRoute&& this.TryFindSmtpRpcActor(targetId, out var rpcActor))
            {
                return rpcActor.Invoke(returnType,invokeKey, invokeOption, ref parameters, types);
            }

            var rpcPackage = new SmtpRpcPackage
            {
                MethodName = invokeKey,
                TargetId = targetId,
                SourceId = this.SmtpActor.Id,
                Route = true
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetReverseWaitData(rpcPackage);
            var byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = SmtpInvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledInvoke, new CanceledPackage() { SourceId = this.SmtpActor.Id, TargetId = targetId, Sign = rpcPackage.Sign, Route = true });
            }

            try
            {
                rpcPackage.LoadInvokeOption(invokeOption);
                var datas = new List<byte[]>();
                foreach (var parameter in parameters)
                {
                    datas.Add(this.SerializationSelector.SerializeParameter(rpcPackage.SerializationType, parameter));
                }
                rpcPackage.ParametersBytes = datas;
                rpcPackage.Package(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SmtpActor.Send(this.m_invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (SmtpRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.IsByRef)
                                        {
                                            for (var i = 0; i < parameters.Length; i++)
                                            {
                                                parameters[i] = this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        return this.SerializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, returnType);
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }

                    default:
                        return default;
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        public Task InvokeAsync(string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(targetId, invokeKey, invokeOption, parameters);
            });
        }

        /// <inheritdoc/>
        public Task<object> InvokeAsync(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke(returnType,targetId, invokeKey, invokeOption, parameters);
            });
        }

        #endregion IdRpc
    }
}