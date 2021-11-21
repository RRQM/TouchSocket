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
using RRQMCore.ByteManager;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC服务器辅助类
    /// </summary>
    public class RpcSocketClient : ProtocolSocketClient, ICaller
    {
        /// <summary>
        /// 预处理流
        /// </summary>
        public event RRQMStreamOperationEventHandler<RpcSocketClient> BeforeReceiveStream;

        /// <summary>
        /// 收到流数据
        /// </summary>
        public event RRQMStreamStatusEventHandler<RpcSocketClient> ReceivedStream;

        /// <summary>
        /// 收到数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<RpcSocketClient> Received;

        internal Action<MethodInvoker, MethodInstance> executeMethod;

        internal Func<RpcSocketClient, RpcContext, RpcContext> IDAction;

        internal MethodMap methodMap;

        internal SerializationSelector serializationSelector;

        internal RRQMWaitHandlePool<IWaitResult> waitPool;

        private ConcurrentDictionary<int, RpcServerCallContext> contextDic;

        private ConcurrentDictionary<Type, IServerProvider> serverProviderDic;

        static RpcSocketClient()
        {
            AddUsedProtocol(100, "请求RPC代理文件");
            AddUsedProtocol(101, "RPC调用");
            AddUsedProtocol(102, "获取注册服务");
            AddUsedProtocol(103, "ID调用客户端");
            AddUsedProtocol(104, "RPC回调");
            AddUsedProtocol(105, "取消RPC调用");

            for (short i = 106; i < 110; i++)
            {
                AddUsedProtocol(i, "保留协议");
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcSocketClient()
        {
            this.contextDic = new ConcurrentDictionary<int, RpcServerCallContext>();
            this.waitPool = new RRQMWaitHandlePool<IWaitResult>();
            this.serverProviderDic = new ConcurrentDictionary<Type, IServerProvider>();
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodToken"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T CallBack<T>(int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.waitPool.GetWaitData(context);
            context.methodToken = methodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
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
                    case FeedbackType.WaitInvoke:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        this.waitPool.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                    {
                        switch (waitData.Wait(invokeOption.Timeout))
                        {
                            case WaitDataStatus.SetRunning:
                                {
                                    this.waitPool.Destroy(waitData);
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
                        switch (waitData.Wait(invokeOption.Timeout))
                        {
                            case WaitDataStatus.SetRunning:
                                {
                                    RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                    this.waitPool.Destroy(waitData);
                                    if (resultContext.Status == 1)
                                    {
                                        try
                                        {
                                            return (T)this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
                                        }
                                        catch (Exception ex)
                                        {
                                            throw ex;
                                        }
                                    }
                                    else if (resultContext.Status == 2)
                                    {
                                        throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCMethod");
                                    }
                                    else if (resultContext.Status == 3)
                                    {
                                        throw new RRQMRPCException("该方法已被禁用");
                                    }
                                    else if (resultContext.Status == 4)
                                    {
                                        throw new RRQMRPCException($"服务器已阻止本次行为，信息：{resultContext.Message}");
                                    }
                                    else if (resultContext.Status == 5)
                                    {
                                        throw new RRQMRPCInvokeException("函数执行异常，详细信息：" + resultContext.Message);
                                    }
                                    else if (resultContext.Status == 6)
                                    {
                                        throw new RRQMRPCException($"函数异常，信息：{resultContext.Message}");
                                    }
                                    break;
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

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <param name="methodToken"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        public void CallBack(int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.waitPool.GetWaitData(context);
            context.methodToken = methodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
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
                    case FeedbackType.WaitInvoke:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        this.waitPool.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitSend:
                    {
                        switch (waitData.Wait(invokeOption.Timeout))
                        {
                            case WaitDataStatus.SetRunning:
                                {
                                    this.waitPool.Destroy(waitData);
                                }
                                break;
                            case WaitDataStatus.Overtime:
                                {
                                    throw new RRQMTimeoutException("等待结果超时");
                                }
                        }
                        return;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        switch (waitData.Wait(invokeOption.Timeout))
                        {
                            case WaitDataStatus.SetRunning:
                                {
                                    RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                    this.waitPool.Destroy(waitData);
                                    if (resultContext.Status == 1)
                                    {
                                        return;
                                    }
                                    else if (resultContext.Status == 2)
                                    {
                                        throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCMethod");
                                    }
                                    else if (resultContext.Status == 3)
                                    {
                                        throw new RRQMRPCException("该方法已被禁用");
                                    }
                                    else if (resultContext.Status == 4)
                                    {
                                        throw new RRQMRPCException($"服务器已阻止本次行为，信息：{resultContext.Message}");
                                    }
                                    else if (resultContext.Status == 5)
                                    {
                                        throw new RRQMRPCInvokeException("函数执行异常，详细信息：" + resultContext.Message);
                                    }
                                    else if (resultContext.Status == 6)
                                    {
                                        throw new RRQMRPCException($"函数异常，信息：{resultContext.Message}");
                                    }
                                    break;
                                }
                            case WaitDataStatus.Overtime:
                                {
                                    throw new RRQMTimeoutException("等待结果超时");
                                }
                        }
                        return;
                    }
                default:
                    return;
            }
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <param name="invokeContext"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public byte[] CallBack(RpcContext invokeContext, int timeout)
        {
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.waitPool.GetWaitData(context);
            context.methodToken = invokeContext.methodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);

            InvokeOption invokeOption = new InvokeOption();
            try
            {
                invokeOption.FeedbackType = (FeedbackType)invokeContext.Feedback;
                invokeOption.SerializationType = (SerializationType)invokeContext.SerializationType;
                context.LoadInvokeOption(invokeOption);
                context.parametersBytes = invokeContext.parametersBytes;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.InternalSendAsync(104, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                    case FeedbackType.WaitInvoke:
                        {
                            this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        this.waitPool.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                    {
                        switch (waitData.Wait(invokeOption.Timeout))
                        {
                            case WaitDataStatus.SetRunning:
                                {
                                    this.waitPool.Destroy(waitData);
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
                        switch (waitData.Wait(invokeOption.Timeout))
                        {
                            case WaitDataStatus.SetRunning:
                                {
                                    RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                    this.waitPool.Destroy(waitData);
                                    if (resultContext.Status == 1)
                                    {
                                        try
                                        {
                                            return resultContext.ReturnParameterBytes;
                                        }
                                        catch (Exception ex)
                                        {
                                            throw ex;
                                        }
                                    }
                                    else if (resultContext.Status == 2)
                                    {
                                        throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCMethod");
                                    }
                                    else if (resultContext.Status == 3)
                                    {
                                        throw new RRQMRPCException("该方法已被禁用");
                                    }
                                    else if (resultContext.Status == 4)
                                    {
                                        throw new RRQMRPCException($"服务器已阻止本次行为，信息：{resultContext.Message}");
                                    }
                                    else if (resultContext.Status == 5)
                                    {
                                        throw new RRQMRPCInvokeException("函数执行异常，详细信息：" + resultContext.Message);
                                    }
                                    else if (resultContext.Status == 6)
                                    {
                                        throw new RRQMRPCException($"函数异常，信息：{resultContext.Message}");
                                    }
                                    break;
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

        internal void EndInvoke(RpcContext context)
        {
            this.contextDic.TryRemove(context.Sign, out _);
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            context.Serialize(byteBlock);
            try
            {
                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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
        protected sealed override void HandleProtocolData(short? procotol, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
            switch (procotol)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        try
                        {
                            string proxyToken = Encoding.UTF8.GetString(buffer, 2, r - 2);
                            this.P100_RequestProxy(proxyToken);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 100, 错误详情:{e.Message}");
                        }
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
                            Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*获取注册服务*/
                    {
                        try
                        {
                            string proxyToken = Encoding.UTF8.GetString(buffer, 2, r - 2);
                            byte[] data = SerializeConvert.RRQMBinarySerialize(((IRRQMRpcParser)this.Service).GetRegisteredMethodItems(proxyToken, this), true);
                            this.InternalSend(102, data, 0, data.Length);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 103:/*ID调用客户端*/
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                byteBlock.Pos = 2;
                                RpcContext content = RpcContext.Deserialize(byteBlock);
                                content = this.IDAction(this, content);

                                ByteBlock retuenByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                                try
                                {
                                    content.Serialize(retuenByteBlock);
                                    this.InternalSend(103, retuenByteBlock.Buffer, 0, (int)retuenByteBlock.Length);
                                }
                                finally
                                {
                                    byteBlock.Dispose();
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Debug(LogType.Error, this, $"错误代码: 103, 错误详情:{e.Message}");
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
                            this.waitPool.SetRun(content);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 104, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 105:/*取消函数调用*/
                    {
                        try
                        {
                            int sign = BitConverter.ToInt32(byteBlock.Buffer, 2);
                            if (this.contextDic.TryGetValue(sign, out RpcServerCallContext context))
                            {
                                context.tokenSource.Cancel();
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 105, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case < 110:
                    {
                        break;
                    }
                default:
                    RPCHandleDefaultData(procotol, byteBlock);
                    break;
            }
        }

        /// <summary>
        /// 处理其余协议的事件触发
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected void OnHandleDefaultData(short? procotol, ByteBlock byteBlock)
        {
            Received?.Invoke(this, procotol, byteBlock);
        }

        /// <summary>
        /// RPC处理其余协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void RPCHandleDefaultData(short? procotol, ByteBlock byteBlock)
        {
            this.OnHandleDefaultData(procotol, byteBlock);
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
                            RpcServerCallContext serverCallContext = new RpcServerCallContext();
                            serverCallContext.tokenSource = new System.Threading.CancellationTokenSource();
                            serverCallContext.caller = this;
                            serverCallContext.methodInvoker = methodInvoker;
                            serverCallContext.methodInstance = methodInstance;
                            serverCallContext.context = context;

                            this.contextDic.TryAdd(context.Sign, serverCallContext);

                            ps[0] = serverCallContext;
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

        private void P100_RequestProxy(string proxyToken)
        {
            byte[] data = SerializeConvert.RRQMBinarySerialize(((IRRQMRpcParser)this.Service).GetProxyInfo(proxyToken, this), true);
            this.InternalSend(100, data, 0, data.Length);
        }

        private void P101_Execute(RpcContext context)
        {
            if (context.Feedback == 1)
            {
                List<byte[]> ps = context.parametersBytes;

                ByteBlock returnByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                try
                {
                    context.parametersBytes = null;
                    context.Status = 1;
                    context.Serialize(returnByteBlock);
                    this.InternalSend(101, returnByteBlock.Buffer, 0, returnByteBlock.Len);
                }
                finally
                {
                    context.parametersBytes = ps;
                    returnByteBlock.Dispose();
                }
            }

            this.ExecuteContext(context);
        }

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
                item.tokenSource.Cancel();
            }
            this.contextDic.Clear();
            this.serverProviderDic.Clear();
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        protected override void PreviewHandleStream(StreamOperationEventArgs args)
        {
            this.BeforeReceiveStream.Invoke(this, args);
        }
    }
}