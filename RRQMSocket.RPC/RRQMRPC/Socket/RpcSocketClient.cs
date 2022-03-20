//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;

using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC服务器辅助类
    /// </summary>
    public class RpcSocketClient : ProtocolSocketClient, ITcpRpcClientBase
    {
        internal Action<MethodInvoker, MethodInstance> executeMethod;
        internal MethodMap methodMap;
        internal SerializationSelector serializationSelector;
        private ConcurrentDictionary<int, RpcCallContext> contextDic;
        private ConcurrentDictionary<Type, IServerProvider> serverProviderDic;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcSocketClient()
        {
            this.contextDic = new ConcurrentDictionary<int, RpcCallContext>();
            this.serverProviderDic = new ConcurrentDictionary<Type, IServerProvider>(); BeforeReceiveStream = null;
        }

        /// <summary>
        /// 预处理流
        /// </summary>
        public event RRQMStreamOperationEventHandler<RpcSocketClient> BeforeReceiveStream;

        /// <summary>
        /// 收到数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<RpcSocketClient> Received;

        /// <summary>
        /// 收到流数据
        /// </summary>
        public event RRQMStreamStatusEventHandler<RpcSocketClient> ReceivedStream;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Dispose()
        {
            if (this.disposable)
            {
                return;
            }

            base.Dispose();

            foreach (var item in this.contextDic.Values)
            {
                item.TokenSource.Cancel();
            }
            this.contextDic.Clear();
            this.serverProviderDic.Clear();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnBreakOut()
        {
            this.BeforeReceiveStream = null;
            this.Received = null;
            this.ReceivedStream = null;
            base.OnBreakOut();
        }

        internal void EndInvoke(RpcContext context)
        {
            this.contextDic.TryRemove(context.Sign, out _);
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            context.Serialize(byteBlock);
            try
            {
                if (this.Online)
                {
                    this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, ex.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }


        /// <summary>
        /// 处理协议数据
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected override sealed void HandleProtocolData(short protocol, ByteBlock byteBlock)
        {
            switch (protocol)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        break;
                    }
                case 101:/*函数调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext content = RpcContext.Deserialize(byteBlock);
                            this.P101_Execute(content);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*获取注册服务*/
                    {
                        try
                        {
                            DiscoveryServiceWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<DiscoveryServiceWaitResult>(byteBlock.Buffer, 2);
                            waitResult.Methods = ((IRRQMRpcParser)this.Service).GetRegisteredMethodItems(waitResult.PT, this);
                            byte[] data = SerializeConvert.RRQMBinarySerialize(waitResult);
                            this.InternalSend(102, data, 0, data.Length);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 103:/*ID调用客户端*/
                    {
                        byteBlock.Pos = 2;
                        RpcContext content = RpcContext.Deserialize(byteBlock);
                        this.P103_OnInvokeClientByID(content.id, content);
                        break;
                    }
                case 104:/*回调函数调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext content = RpcContext.Deserialize(byteBlock);
                            this.WaitHandlePool.SetRun(content);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 105:/*取消函数调用*/
                    {
                        try
                        {
                            int sign = RRQMBitConverter.Default.ToInt32(byteBlock.Buffer, 2);
                            if (this.contextDic.TryGetValue(sign, out RpcCallContext context))
                            {
                                context.TokenSource.Cancel();
                            }
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 106:/*发布事件*/
                    {
                        
                        break;
                    }
                case 107:/*取消发布事件*/
                    {
                        
                        break;
                    }
                case 108:/*订阅事件*/
                    {
                        
                        break;
                    }
                case 109:/*触发事件*/
                    {
                        
                        break;
                    }
                case 111:/*获取所有事件*/
                    {
                       
                        break;
                    }

                case 112:/*取消订阅*/
                    {
                       
                        break;
                    }
                default:
                    this.RPCHandleDefaultData(protocol, byteBlock);
                    break;
            }
        }

        private void P103_OnInvokeClientByID(string id, RpcContext context)
        {
            Task.Run(() =>
            {
                using (ByteBlock retuenByteBlock = BytePool.GetByteBlock(this.BufferLength))
                {
                    if (this.Service.SocketClients.TryGetSocketClient(id, out RpcSocketClient socketClient))
                    {
                        InvokeOption invokeOption = new InvokeOption();
                        invokeOption.FeedbackType = (FeedbackType)context.Feedback;
                        invokeOption.InvokeType = context.InvokeType;
                        invokeOption.SerializationType = context.SerializationType;
                        invokeOption.Timeout = context.Timeout;

                        try
                        {
                            var resultContext = this.Invoke(context, invokeOption);
                            context.Status = 1;
                            if (resultContext == null)
                            {
                                context.parametersBytes = null;
                            }
                            else
                            {
                                context.returnParameterBytes = resultContext.returnParameterBytes;
                            }
                        }
                        catch (Exception ex)
                        {
                            context.parametersBytes = null;
                            context.Status = 6;
                            context.Message = ex.Message;
                        }
                        context.Serialize(retuenByteBlock);
                        this.InternalSend(103, retuenByteBlock.Buffer, 0, retuenByteBlock.Len);
                    }
                    else
                    {
                        context.parametersBytes = null;
                        context.Status = 7;
                        context.Serialize(retuenByteBlock);
                        this.InternalSend(103, retuenByteBlock.Buffer, 0, retuenByteBlock.Len);
                    }
                }
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        protected override void HandleStream(StreamStatusEventArgs args)
        {
            this.ReceivedStream?.Invoke(this, args);
        }

        /// <summary>
        /// 处理其余协议的事件触发
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected void OnHandleDefaultData(short protocol, ByteBlock byteBlock)
        {
            Received?.Invoke(this, protocol, byteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        protected override void PreviewHandleStream(StreamOperationEventArgs args)
        {
            this.BeforeReceiveStream?.Invoke(this, args);
        }

        /// <summary>
        /// RPC处理其余协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void RPCHandleDefaultData(short protocol, ByteBlock byteBlock)
        {
            this.OnHandleDefaultData(protocol, byteBlock);
        }

        private void CanceledInvoke(int sign)
        {
            this.InternalSend(113, RRQMBitConverter.Default.GetBytes(sign));
        }

        private void ExecuteContext(RpcContext context)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = this;
            methodInvoker.Flag = context;
            methodInvoker.InvokeType = context.InvokeType;
            if (this.methodMap.TryGet(context.MethodToken, out MethodInstance methodInstance))
            {
                try
                {
                    if (methodInstance.IsEnable)
                    {
                        object[] ps;
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            methodInvoker.AsyncRun = true;

                            ps = new object[methodInstance.ParameterTypes.Length];

                            RpcCallContext callContext = new RpcCallContext(this, context, methodInstance, methodInvoker);
                            this.contextDic.TryAdd(context.Sign, callContext);

                            ps[0] = callContext;
                            for (int i = 0; i < context.parametersBytes.Count; i++)
                            {
                                ps[i + 1] = this.serializationSelector.DeserializeParameter(context.SerializationType, context.ParametersBytes[i], methodInstance.ParameterTypes[i + 1]);
                            }
                        }
                        else
                        {
                            ps = new object[methodInstance.ParameterTypes.Length];
                            for (int i = 0; i < methodInstance.ParameterTypes.Length; i++)
                            {
                                ps[i] = this.serializationSelector.DeserializeParameter(context.SerializationType, context.ParametersBytes[i], methodInstance.ParameterTypes[i]);
                            }
                        }

                        methodInvoker.Parameters = ps;

                        if (context.InvokeType == InvokeType.CustomInstance)
                        {
                            IServerProvider instance;
                            if (!this.serverProviderDic.TryGetValue(methodInstance.ProviderType, out instance))
                            {
                                instance = (IServerProvider)Activator.CreateInstance(methodInstance.ProviderType);
                                this.serverProviderDic.TryAdd(methodInstance.ProviderType, instance);
                            }
                            methodInvoker.CustomServerProvider = instance;
                        }
                    }
                    else
                    {
                        methodInvoker.Status = InvokeStatus.UnEnable;
                    }
                }
                catch (Exception ex)
                {
                    methodInvoker.Status = InvokeStatus.Exception;
                    methodInvoker.StatusMessage = ex.Message;
                }

                this.executeMethod.Invoke(methodInvoker, methodInstance);
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
                this.executeMethod.Invoke(methodInvoker, null);
            }
        }

        private void P101_Execute(RpcContext context)
        {
            if (context.Feedback == 1)
            {
                List<byte[]> ps = context.parametersBytes;

                ByteBlock returnByteBlock = BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    context.parametersBytes = null;
                    context.Status = 1;
                    context.Serialize(returnByteBlock);
                    this.InternalSendAsync(101, returnByteBlock.Buffer, 0, returnByteBlock.Len);
                }
                finally
                {
                    context.parametersBytes = ps;
                    returnByteBlock.Dispose();
                }
            }

            this.ExecuteContext(context);
        }

        #region RPC

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="types">对应类型集合</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回值</returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.CancellationToken.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.CancellationToken);
                invokeOption.CancellationToken.Register(() =>
                {
                    this.CanceledInvoke(context.Sign);
                });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.InternalSendAsync(104, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                    }
                                    break;

                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        return (T)this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
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
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="types">对应类型集合</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.CancellationToken.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.CancellationToken);
                invokeOption.CancellationToken.Register(() =>
                {
                    this.CanceledInvoke(context.Sign);
                });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.InternalSendAsync(104, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                    }
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
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
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
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.CancellationToken.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.CancellationToken);
                invokeOption.CancellationToken.Register(() =>
                {
                    this.CanceledInvoke(context.Sign);
                });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.InternalSendAsync(104, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
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
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
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
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.CancellationToken.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.CancellationToken);
                invokeOption.CancellationToken.Register(() =>
                {
                    this.CanceledInvoke(context.Sign);
                });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.InternalSendAsync(104, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
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
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        return (T)this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
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
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 反向调用客户端RPC
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public void Invoke(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            context.methodName = method;
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            context.LoadInvokeOption(invokeOption);
            List<byte[]> datas = new List<byte[]>();
            foreach (object parameter in parameters)
            {
                datas.Add(this.serializationSelector.SerializeParameter(context.SerializationType, parameter));
            }
            context.parametersBytes = datas;
            this.Invoke(context, invokeOption);
        }

        /// <summary>
        /// 反向调用客户端RPC
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回值</returns>
        public T Invoke<T>(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            context.methodName = method;
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            context.LoadInvokeOption(invokeOption);
            List<byte[]> datas = new List<byte[]>();
            foreach (object parameter in parameters)
            {
                datas.Add(this.serializationSelector.SerializeParameter(context.SerializationType, parameter));
            }
            context.parametersBytes = datas;
            var resultContext = this.Invoke(context, invokeOption);
            if (resultContext == null)
            {
                return default;
            }
            else
            {
                return (T)this.serializationSelector.DeserializeParameter(context.SerializationType, resultContext.ReturnParameterBytes, typeof(T));

            }
        }

        /// <summary>
        /// 反向调用客户端RPC
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public Task InvokeAsync(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(id, method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 反向调用客户端RPC
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回值</returns>
        public Task<T> InvokeAsync<T>(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(id, method, invokeOption, parameters);
            });
        }

        private RpcContext Invoke(RpcContext invokeContext, IInvokeOption invokeOption)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = invokeContext.methodName;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);

            try
            {
                context.LoadInvokeOption(invokeOption);
                context.parametersBytes = invokeContext.parametersBytes;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.InternalSendAsync(104, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
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
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        return (RpcContext)waitData.WaitResult;
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
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        #endregion RPC
    }
}