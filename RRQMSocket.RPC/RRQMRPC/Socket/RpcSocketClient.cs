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
    /// Rpc服务器辅助类
    /// </summary>
    public class RpcSocketClient : ProtocolSocketClient, ITcpRpcClientBase
    {
       
        internal Action<MethodInvoker, MethodInstance> executeMethod;
        internal MethodMap methodMap;
        internal SerializationSelector serializationSelector;
        private ConcurrentDictionary<long, RpcCallContext> contextDic;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcSocketClient()
        {
            this.contextDic = new ConcurrentDictionary<long, RpcCallContext>();
        }

        internal void EndInvoke(RpcContext context)
        {
            this.contextDic.TryRemove(context.Sign, out _);
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            try
            {
                context.Serialize(byteBlock);
                if (this.Online)
                {
                    lock (this)
                    {
                        this.InternalSend(101, byteBlock);
                    }
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.disposedValue)
            {
                return;
            }
            foreach (var item in this.contextDic.Values)
            {
                item.TokenSource.Cancel();
            }
            this.contextDic.Clear();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected override void HandleProtocolData(short protocol, ByteBlock byteBlock)
        {
            switch (protocol)
            {
                case 100:/*100，请求Rpc文件*/
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
                            waitResult.Methods = ((IRRQMRPCParser)this.Service).GetRegisteredMethodItems(waitResult.PT, this);
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
                            if (this.contextDic.TryGetValue(RRQMBitConverter.Default.ToInt64(byteBlock.Buffer, 2), out RpcCallContext context))
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
                case 113:/*取消ID调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            string id = byteBlock.ReadString();
                            long sign = byteBlock.ReadInt64();

                            if (this.Service.SocketClients.TryGetSocketClient(id, out RpcSocketClient socketClient))
                            {
                                socketClient.CanceledInvoke(sign);
                            }
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                default:
                    this.HandleRpcDefaultData(protocol, byteBlock);
                    break;
            }
        }

        /// <summary>
        /// Rpc处理其余协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void HandleRpcDefaultData(short protocol, ByteBlock byteBlock)
        {
            this.OnReceived(protocol, byteBlock);
        }

        private void CanceledInvoke(long sign)
        {
            this.InternalSend(113, RRQMBitConverter.Default.GetBytes(sign));
        }

        private void ExecuteContext(RpcContext context)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = this;
            methodInvoker.Flag = context;
            if (this.methodMap.TryGet(context.MethodToken, out MethodInstance methodInstance))
            {
                try
                {
                    if (methodInstance.IsEnable)
                    {
                        object[] ps;
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
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
                        invokeOption.SerializationType = context.SerializationType;
                        invokeOption.Timeout = context.Timeout;

                        try
                        {
                            var resultContext = socketClient.Invoke(context, invokeOption,false);
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

        #region Rpc

        /// <summary>
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="types">对应类型集合</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RpcNoRegisterException">Rpc服务器未注册</exception>
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

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(() =>
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
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="types">对应类型集合</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RpcNoRegisterException">Rpc服务器未注册</exception>
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

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(() =>
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
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
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

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(() =>
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
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
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

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(() =>
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
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public void Invoke(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (!this.Service.SocketClients.TryGetSocketClient(id, out RpcSocketClient socketClient))
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetResString(id));
            }
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
            socketClient.Invoke(context, invokeOption,true);
        }

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回值</returns>
        public T Invoke<T>(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (!this.Service.SocketClients.TryGetSocketClient(id, out RpcSocketClient socketClient))
            {
                throw new ClientNotFindException(ResType.ClientNotFind.GetResString(id));
            }
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
            var resultContext = socketClient.Invoke(context, invokeOption,true);
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
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">Rpc异常</exception>
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
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
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
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRpcInvokeException">调用内部异常</exception>
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

        private RpcContext Invoke(RpcContext sourceContext, IInvokeOption invokeOption,bool autoSign)
        {
            RpcContext thisContext = new RpcContext();
            if (!autoSign)
            {
                thisContext.Sign = sourceContext.Sign;
            }
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(thisContext,autoSign);
            thisContext.methodName = sourceContext.methodName;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);

            try
            {
                thisContext.LoadInvokeOption(invokeOption);
                thisContext.parametersBytes = sourceContext.parametersBytes;
                thisContext.Serialize(byteBlock);

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

        #endregion Rpc
    }
}