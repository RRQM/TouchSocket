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
using System.Net;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UDP RPC解释器
    /// </summary>
    public class UdpRpc : UdpSession, IRPCParser, IRRQMRpcParser, IRRQMRpcClient
    {
        private ConcurrentDictionary<int, RpcCallContext> contextDic;
        private Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod;
        private MethodMap methodMap;
        private MethodStore methodStore;
        private SerializationSelector serializationSelector;
        private RRQMWaitHandlePool<IWaitResult> waitHandlePool;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpRpc()
        {
            this.contextDic = new ConcurrentDictionary<int, RpcCallContext>();
            this.waitHandlePool = new RRQMWaitHandlePool<IWaitResult>();
            this.methodStore = new MethodStore();
        }

        /// <summary>
        /// 发现服务后
        /// </summary>
        public event RRQMMessageEventHandler<UdpRpc> ServiceDiscovered;

        /// <summary>
        /// 返回ID
        /// </summary>
        public string ID => null;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodMap MethodMap => this.methodMap;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodStore MethodStore => this.methodStore;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ProxyToken { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RPCService RPCService { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod => this.executeMethod;

        /// <summary>
        /// 序列化选择器
        /// </summary>
        public SerializationSelector SerializationSelector
        {
            get { return this.serializationSelector; }
        }

        /// <summary>
        /// 等待返回池
        /// </summary>
        public RRQMWaitHandlePool<IWaitResult> WaitHandlePool { get => this.waitHandlePool; }

        /// <summary>
        /// 发现服务
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="token"></param>
        /// <returns>已发现的服务</returns>
        public MethodItem[] DiscoveryService(string proxyToken, System.Threading.CancellationToken token = default)
        {
            if (this.ServerState != ServerState.Running)
            {
                throw new RRQMRPCException("UDP端需要先绑定本地监听端口");
            }

            int count = 0;
            while (count < 3)
            {
                lock (this)
                {
                    if (this.RemoteIPHost == null)
                    {
                        throw new RRQMNotConnectedException("未设置远程服务器地址");
                    }

                    DiscoveryServiceWaitResult waitResult = new DiscoveryServiceWaitResult();
                    WaitData<IWaitResult> waitData = this.waitHandlePool.GetWaitData(waitResult);

                    try
                    {
                        this.methodStore = new MethodStore();
                        waitResult.PT = proxyToken;

                        waitData.SetCancellationToken(token);

                        this.UDPSend(100, SerializeConvert.RRQMBinarySerialize(waitResult));
                        switch (waitData.Wait(10 * 1000))
                        {
                            case WaitDataStatus.SetRunning:
                                {
                                    DiscoveryServiceWaitResult result = (DiscoveryServiceWaitResult)waitData.WaitResult;
                                    if (result.Methods == null)
                                    {
                                        throw new RRQMException("发现的服务为空。");
                                    }
                                    foreach (var item in result.Methods)
                                    {
                                        this.methodStore.AddMethodItem(item);
                                    }
                                    this.OnServiceDiscovered(new MesEventArgs("success"));
                                    return result.Methods;
                                }
                            case WaitDataStatus.Overtime:
                                break;

                            case WaitDataStatus.Canceled:
                                return null;

                            case WaitDataStatus.Default:
                            case WaitDataStatus.Disposed:
                            default:
                                throw new RRQMException("未知错误。");
                        }
                    }
                    finally
                    {
                        this.waitHandlePool.Destroy(waitData);
                    }
                }
                count++;
            }
            throw new TimeoutException("初始化超时");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void ExecuteContext(RpcContext context, ICaller caller)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = caller;
            methodInvoker.Flag = context;
            if (this.MethodMap.TryGet(context.MethodToken, out MethodInstance methodInstance))
            {
                try
                {
                    if (methodInstance.IsEnable)
                    {
                        object[] ps;
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            ps = new object[methodInstance.ParameterTypes.Length];
                            RpcCallContext callContext = new RpcCallContext(caller, context, methodInstance, methodInvoker);
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

                this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
                this.RRQMExecuteMethod.Invoke(this, methodInvoker, null);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        public virtual void GetProxyInfo(GetProxyInfoArgs args)
        {
            if (args.RpcType.HasFlag(RpcType.RRQMRPC))
            {
                if (args.ProxyToken != this.ProxyToken)
                {
                    args.ErrorMessage = "在验证RRQMRPC时令箭不正确。";
                    args.IsSuccess = false;
                    return;
                }
                foreach (var item in this.RPCService.ServerProviders)
                {
                    var serverCellCode = CodeGenerator.Generator<RRQMRPCAttribute>(item.GetType());
                    args.Codes.Add(serverCellCode);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual MethodItem[] GetRegisteredMethodItems(string proxyToken, ICaller caller)
        {
            return this.methodStore.GetAllMethodItem();
        }

        #region RPC
        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 函数式调用
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        this.WaitHandlePool.Destroy(waitData);
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        this.WaitHandlePool.Destroy(waitData);
                                        if (resultContext.Status == 1)
                                        {
                                            if (methodItem.IsOutOrRef)
                                            {
                                                try
                                                {
                                                    for (int i = 0; i < parameters.Length; i++)
                                                    {
                                                        parameters[i] = this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    throw new RRQMException(e.Message);
                                                }
                                            }
                                            else
                                            {
                                                parameters = null;
                                            }
                                            return (T)this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        this.WaitHandlePool.Destroy(waitData);
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        if (resultContext.Status == 1)
                                        {
                                            if (methodItem.IsOutOrRef)
                                            {
                                                for (int i = 0; i < parameters.Length; i++)
                                                {
                                                    parameters[i] = this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                                }
                                            }
                                            else
                                            {
                                                parameters = null;
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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
                            this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        if (resultContext.Status == 1)
                                        {
                                            return (T)this.serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RpcContext context = (RpcContext)methodInvoker.Flag;
            if (context.Feedback != 2)
            {
                return;
            }
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            try
            {
                switch (methodInvoker.Status)
                {
                    case InvokeStatus.Ready:
                        {
                            break;
                        }

                    case InvokeStatus.UnFound:
                        {
                            context.Status = 2;
                            break;
                        }
                    case InvokeStatus.Success:
                        {
                            if (methodInstance.MethodToken > 50000000)
                            {
                                context.returnParameterBytes = this.serializationSelector.SerializeParameter(context.SerializationType, methodInvoker.ReturnParameter);
                            }
                            else
                            {
                                context.returnParameterBytes = null;
                            }

                            if (methodInstance.IsByRef)
                            {
                                context.parametersBytes = new List<byte[]>();
                                int i = 0;
                                if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                                {
                                    i = 1;
                                }
                                for (; i < methodInvoker.Parameters.Length; i++)
                                {
                                    context.parametersBytes.Add(this.serializationSelector.SerializeParameter(context.SerializationType, methodInvoker.Parameters[i]));
                                }
                            }
                            else
                            {
                                context.parametersBytes = null;
                            }

                            context.Status = 1;
                            break;
                        }
                    case InvokeStatus.Abort:
                        {
                            context.Status = 4;
                            context.Message = methodInvoker.StatusMessage;
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            context.Status = 3;
                            break;
                        }
                    case InvokeStatus.InvocationException:
                        {
                            context.Status = 5;
                            context.Message = methodInvoker.StatusMessage;
                            break;
                        }
                    case InvokeStatus.Exception:
                        {
                            context.Status = 6;
                            context.Message = methodInvoker.StatusMessage;
                            break;
                        }
                    default:
                        break;
                }

                context.Serialize(byteBlock);
                this.UDPSend(102, ((UdpCaller)methodInvoker.Caller).CallerEndPoint, byteBlock.Buffer, 0, byteBlock.Len);
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
        public void OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var item in methodInstances)
            {
                if (item.GetAttribute<RRQMRPCAttribute>() is RRQMRPCAttribute attribute)
                {
                    MethodItem methodItem = new MethodItem();
                    methodItem.ServerName = provider.GetType().Name;
                    methodItem.IsOutOrRef = item.IsByRef;
                    methodItem.MethodToken = item.MethodToken;

                    if (string.IsNullOrEmpty(attribute.MethodName))
                    {
                        methodItem.Method = item.Method.Name;
                    }
                    else
                    {
                        methodItem.Method = attribute.MethodName;
                    }

                    this.methodStore.AddMethodItem(methodItem);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var item in methodInstances)
            {
                this.methodStore.RemoveMethodItem(item.MethodToken);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetExecuteMethod(Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.executeMethod = executeMethod;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetMethodMap(MethodMap methodMap)
        {
            this.methodMap = methodMap;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetRPCService(RPCService service)
        {
            this.RPCService = service;
        }

        /// <summary>
        /// 密封处理
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override sealed void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            short protocol = RRQMBitConverter.Default.ToInt16(buffer, 0);

            switch (protocol)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        try
                        {
                            DiscoveryServiceWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<DiscoveryServiceWaitResult>(buffer, 2);
                            waitResult.Methods = ((IRRQMRpcParser)this).GetRegisteredMethodItems(waitResult.PT, new UdpCaller(this, remoteEndPoint));
                            byte[] data = SerializeConvert.RRQMBinarySerialize(waitResult);
                            this.UDPSend(104, remoteEndPoint, data, 0, data.Length);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 101:/*函数式调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext content = RpcContext.Deserialize(byteBlock);
                            this.P101_Execute(content, remoteEndPoint);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*调用返回*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext result = RpcContext.Deserialize(byteBlock);
                            this.WaitHandlePool.SetRun(result.Sign, result);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 103:/*取消调用*/
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
                case 104:/*104，请求RPC文件返回*/
                    {
                        try
                        {
                            DiscoveryServiceWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<DiscoveryServiceWaitResult>(buffer, 2);
                            this.waitHandlePool.SetRun(waitResult);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void LoadConfig(ServiceConfig serviceConfig)
        {
            this.serializationSelector = (SerializationSelector)serviceConfig.GetValue(UdpRpcParserConfig.SerializationSelectorProperty);
            this.ProxyToken = (string)serviceConfig.GetValue(UdpRpcParserConfig.ProxyTokenProperty);
            base.LoadConfig(serviceConfig);
        }

        /// <summary>
        /// RPC完成初始化后
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnServiceDiscovered(MesEventArgs args)
        {
            try
            {
                this.ServiceDiscovered?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(ServiceDiscovered)}中发生异常", ex);
            }
        }

        private void CanceledInvoke(int sign)
        {
            this.UDPSend(103, RRQMBitConverter.Default.GetBytes(sign));
        }

        private void ExecuteContext(RpcContext context, EndPoint endPoint)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = new UdpCaller(this, endPoint);
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
                            RpcCallContext callContext = new RpcCallContext(new UdpCaller(this, endPoint), context, methodInstance, methodInvoker);
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

                this.executeMethod.Invoke(this, methodInvoker, methodInstance);
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
                this.executeMethod.Invoke(this, methodInvoker, null);
            }
        }

        private void P101_Execute(RpcContext context, EndPoint endPoint)
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
                    this.UDPSend(102, returnByteBlock.Buffer, 0, returnByteBlock.Len);
                }
                finally
                {
                    context.parametersBytes = ps;
                    returnByteBlock.Dispose();
                }
            }

            this.ExecuteContext(context, endPoint);
        }

        #region UDP发送

        private void UDPSend(short protocol, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = BytePool.GetByteBlock(length + 2);
            try
            {
                byteBlock.Write(RRQMBitConverter.Default.GetBytes(protocol));
                byteBlock.Write(buffer, offset, length);
                this.Send(byteBlock.Buffer, 0, byteBlock.Len);
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

        private void UDPSend(short protocol)
        {
            this.UDPSend(protocol, new byte[0], 0, 0);
        }

        private void UDPSend(short protocol, byte[] buffer)
        {
            this.UDPSend(protocol, buffer, 0, buffer.Length);
        }

        private void UDPSend(short protocol, EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = BytePool.GetByteBlock(length + 2);
            try
            {
                byteBlock.Write(RRQMBitConverter.Default.GetBytes(protocol));
                byteBlock.Write(buffer, offset, length);
                this.Send(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void UDPSend(short protocol, EndPoint endPoint, byte[] buffer)
        {
            this.UDPSend(protocol, endPoint, buffer, 0, buffer.Length);
        }

        #endregion UDP发送
    }
}