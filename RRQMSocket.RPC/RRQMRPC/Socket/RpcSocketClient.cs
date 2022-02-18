//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC服务器辅助类
    /// </summary>
    public class RpcSocketClient : ProtocolSocketClient, ICaller, IRpcClient, IIDInvoke
    {
        internal Action<MethodInvoker, MethodInstance> executeMethod;
        internal Action<RpcContext> IDAction;
        internal MethodMap methodMap;
        internal SerializationSelector serializationSelector;
        private ConcurrentDictionary<int, RpcCallContext> contextDic;
        private ConcurrentDictionary<Type, IServerProvider> serverProviderDic;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcSocketClient()
        {
            AddUsedProtocol(100, "请求RPC代理文件(弃用)");
            AddUsedProtocol(101, "RPC调用");
            AddUsedProtocol(102, "获取注册服务");
            AddUsedProtocol(103, "ID调用客户端");
            AddUsedProtocol(104, "RPC回调");
            AddUsedProtocol(105, "取消RPC调用");
            AddUsedProtocol(106, "发布事件");
            AddUsedProtocol(107, "取消发布事件");
            AddUsedProtocol(108, "订阅事件");
            AddUsedProtocol(109, "请求触发事件");
            AddUsedProtocol(110, "分发触发");
            AddUsedProtocol(111, "获取所有事件");
            AddUsedProtocol(112, "请求取消订阅");
            AddUsedProtocol(113, "取消RPC回调");

            for (short i = 114; i < 200; i++)
            {
                AddUsedProtocol(i, "保留协议");
            }
            this.contextDic = new ConcurrentDictionary<int, RpcCallContext>();
            this.serverProviderDic = new ConcurrentDictionary<Type, IServerProvider>();
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
                Logger.Debug(LogType.Error, this, ex.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 处理协议数据
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected override sealed void HandleProtocolData(short procotol, ByteBlock byteBlock)
        {
            switch (procotol)
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
                            Logger.Debug(LogType.Error, this, $"错误代码: {procotol}, 错误详情:{e.Message}");
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
                            Logger.Debug(LogType.Error, this, $"错误代码: {procotol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 103:/*ID调用客户端*/
                    {
                        byteBlock.Pos = 2;
                        RpcContext content = RpcContext.Deserialize(byteBlock);

                        EasyAction.TaskRun(content, (con) =>
                         {
                             ByteBlock retuenByteBlock = BytePool.GetByteBlock(this.BufferLength);
                             try
                             {
                                 this.IDAction.Invoke(con);
                                 con.Serialize(retuenByteBlock);
                                 this.InternalSend(103, retuenByteBlock.Buffer, 0, retuenByteBlock.Len);
                             }
                             finally
                             {
                                 retuenByteBlock.Dispose();
                             }
                         });
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
                            Logger.Debug(LogType.Error, this, $"错误代码: {procotol}, 错误详情:{e.Message}");
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
                            Logger.Debug(LogType.Error, this, $"错误代码: {procotol}, 错误详情:{e.Message}");
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
                    RPCHandleDefaultData(procotol, byteBlock);
                    break;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        protected override void HandleStream(StreamStatusEventArgs args)
        {
            this.ReceivedStream.Invoke(this, args);
        }

        /// <summary>
        /// 处理其余协议的事件触发
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected void OnHandleDefaultData(short procotol, ByteBlock byteBlock)
        {
            Received?.Invoke(this, procotol, byteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        protected override void PreviewHandleStream(StreamOperationEventArgs args)
        {
            this.BeforeReceiveStream.Invoke(this, args);
        }

        /// <summary>
        /// RPC处理其余协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void RPCHandleDefaultData(short procotol, ByteBlock byteBlock)
        {
            this.OnHandleDefaultData(procotol, byteBlock);
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
                                serverProviderDic.TryAdd(methodInstance.ProviderType, instance);
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
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
                                        throw new RRQMTimeoutException("等待结果超时");
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
                                        RRQMRPCTools.ThrowRPCStatus(resultContext);
                                        return (T)this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new RRQMTimeoutException("等待结果超时");
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
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
                                        throw new RRQMTimeoutException("等待结果超时");
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
                                        RRQMRPCTools.ThrowRPCStatus(resultContext);
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new RRQMTimeoutException("等待结果超时");
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, InvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
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
                                        throw new RRQMTimeoutException("等待结果超时");
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
                                        RRQMRPCTools.ThrowRPCStatus(resultContext);
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new RRQMTimeoutException("等待结果超时");
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
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
                                        throw new RRQMTimeoutException("等待结果超时");
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
                                        RRQMRPCTools.ThrowRPCStatus(resultContext);
                                        return (T)this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new RRQMTimeoutException("等待结果超时");
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
        /// 选择ID然后调用反向RPC
        /// </summary>
        /// <param name="id"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        public void Invoke(string id, string method, InvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            context.methodName = method;
            if (invokeOption == null)
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
            this.IDAction.Invoke(context);
        }

        /// <summary>
        /// 选择ID然后调用反向RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Invoke<T>(string id, string method, InvokeOption invokeOption, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            context.methodName = method;
            if (invokeOption == null)
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
            this.IDAction.Invoke(context);
            return (T)this.serializationSelector.DeserializeParameter(context.SerializationType, context.ReturnParameterBytes, typeof(T));
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <param name="invokeContext"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal RpcContext Invoke(RpcContext invokeContext, int timeout)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = invokeContext.methodName;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);

            InvokeOption invokeOption = new InvokeOption();
            try
            {
                invokeOption.FeedbackType = (FeedbackType)invokeContext.Feedback;
                invokeOption.SerializationType = invokeContext.SerializationType;
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
                            switch (waitData.Wait(timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new RRQMTimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        return (RpcContext)waitData.WaitResult;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new RRQMTimeoutException("等待结果超时");
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