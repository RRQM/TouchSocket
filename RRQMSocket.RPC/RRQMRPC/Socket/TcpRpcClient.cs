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
using System.Linq;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRpcClient
    /// </summary>
    public class TcpRpcClient : ProtocolClientBase, ITcpRpcClient, IRRQMRPCClient
    {
        private ConcurrentDictionary<string, MethodInstance> callbackMap;
        private ConcurrentDictionary<long, RpcCallContext> contextDic;
        private Action<IRpcParser, MethodInvoker, MethodInstance> executeMethod;
        private MethodMap methodMap;
        private MethodStore methodStore;
        private SerializationSelector serializationSelector;
        private ServerProviderCollection serverProviders;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpRpcClient()
        {
            this.AddUsedProtocol(100, "请求Rpc代理文件(弃用)");
            this.AddUsedProtocol(101, "Rpc调用");
            this.AddUsedProtocol(102, "获取注册服务");
            this.AddUsedProtocol(103, "ID调用客户端");
            this.AddUsedProtocol(104, "Rpc回调");
            this.AddUsedProtocol(105, "取消Rpc调用");
            this.AddUsedProtocol(106, "发布事件");
            this.AddUsedProtocol(107, "取消发布事件");
            this.AddUsedProtocol(108, "订阅事件");
            this.AddUsedProtocol(109, "请求触发事件");
            this.AddUsedProtocol(110, "分发触发");
            this.AddUsedProtocol(111, "获取所有事件");
            this.AddUsedProtocol(112, "请求取消订阅");
            this.AddUsedProtocol(113, "取消Rpc ID回调");

            for (short i = 114; i < 200; i++)
            {
                this.AddUsedProtocol(i, "保留协议");
            }

            this.callbackMap = new ConcurrentDictionary<string, MethodInstance>();
            this.contextDic = new ConcurrentDictionary<long, RpcCallContext>();
            this.methodMap = new MethodMap();
            this.serverProviders = new ServerProviderCollection();
            this.methodStore = new MethodStore();
        }

        #region 事件

        /// <summary>
        /// 收到协议数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<TcpRpcClient> Received;

        /// <summary>
        /// Rpc初始化后
        /// </summary>
        public event RRQMMessageEventHandler<TcpRpcClient> ServiceDiscovered;

        #endregion 事件

        #region 属性

        /// <summary>
        /// 获取反向Rpc映射图
        /// </summary>
        public MethodMap MethodMap => this.methodMap;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcService RpcService { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Action<IRpcParser, MethodInvoker, MethodInstance> RRQMExecuteMethod => this.executeMethod;

        /// <summary>
        /// 序列化选择器
        /// </summary>
        public SerializationSelector SerializationSelector => this.serializationSelector;

        /// <summary>
        /// 获取反向Rpc服务实例
        /// </summary>
        public ServerProviderCollection ServerProviders => this.serverProviders;

        #endregion 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="proxyToken"><inheritdoc/></param>
        /// <param name="token"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public MethodItem[] DiscoveryService(string proxyToken, System.Threading.CancellationToken token = default)
        {
            lock (this)
            {
                if (!this.Online)
                {
                    throw new RRQMNotConnectedException("未连接到服务器");
                }

                DiscoveryServiceWaitResult waitResult = new DiscoveryServiceWaitResult();
                WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitResult);

                try
                {
                    this.methodStore = new MethodStore();
                    waitResult.PT = proxyToken;

                    waitData.SetCancellationToken(token);

                    this.InternalSend(102, RRQMCore.Serialization.SerializeConvert.RRQMBinarySerialize(waitResult));
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
                            throw new TimeoutException("操作超时。");
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
                    this.WaitHandlePool.Destroy(waitData);
                }
            }
        }

        #region Rpc

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RRQMRpcInvokeException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RpcNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
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
                            this.InternalSendAsync(101, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
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
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
                            
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        resultContext.ThrowStatus();
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="RRQMRpcInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RpcNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
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
                            this.InternalSendAsync(101, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
                            
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
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
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        RpcContext resultContext = (RpcContext)waitData.WaitResult;
                                        resultContext.ThrowStatus();
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RRQMRpcInvokeException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RpcNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
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
                            this.InternalSendAsync(101, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="RRQMRpcInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RpcNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodToken = methodItem.MethodToken;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
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
                            this.InternalSendAsync(101, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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
                            lock (this)
                            {
                                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException($"“{nameof(method)}”不能为 null 或空。", nameof(method));
            }

            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            context.id = id;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledIDInvoke, new CanceledID() { id = id, sign = context.Sign });
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
                            this.InternalSendAsync(103, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            lock (this)
                            {
                                this.InternalSend(103, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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
                            lock (this)
                            {
                                this.InternalSend(103, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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
        /// <returns></returns>
        public T Invoke<T>(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException($"“{nameof(method)}”不能为 null 或空。", nameof(method));
            }

            RpcContext context = new RpcContext();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            context.id = id;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledIDInvoke, new CanceledID() { id = id, sign = context.Sign });
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
                            this.InternalSendAsync(103, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            lock (this)
                            {
                                this.InternalSend(103, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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
                            lock (this)
                            {
                                this.InternalSend(103, byteBlock.Buffer, 0, byteBlock.Len);
                            }
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

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RRQMRpcInvokeException"></exception>
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

        #endregion Rpc

        #region RPC解析器

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        public virtual void GetProxyInfo(GetProxyInfoArgs args)
        {
            if (args.RpcType.HasFlag(RpcType.RRQMCallbackRpc))
            {
                if (args.ProxyToken != this.Config.GetValue<string>(RpcConfigExtensions.ProxyTokenProperty))
                {
                    args.Message = "在验证RRQMCallbackRpc时令箭不正确。";
                    args.RemoveOperation(Operation.Permit);
                    return;
                }
                foreach (var item in this.RpcService.ServerProviders)
                {
                    var serverCellCode = CodeGenerator.Generator<RRQMRPCCallBackAttribute>(item.GetType());
                    args.Codes.Add(serverCellCode);
                }
            }
        }

        void IRpcParser.OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RpcContext context = (RpcContext)methodInvoker.Flag;

            if (context.Feedback != 2)
            {
                return;
            }

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

            this.contextDic.TryRemove(context.Sign, out _);
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            context.Serialize(byteBlock);
            try
            {
                if (this.Online)
                {
                    this.InternalSend(104, byteBlock.Buffer, 0, byteBlock.Len);
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

        void IRpcParser.OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var item in methodInstances)
            {
                if (item.GetAttribute<RRQMRPCCallBackAttribute>() is RRQMRPCCallBackAttribute attribute)
                {
                    string key = CodeGenerator.GetMethodName<RRQMRPCCallBackAttribute>(item);
                    if (!this.callbackMap.TryAdd(key, item))
                    {
                        throw new RpcKeyException($"函数键为{key}的函数已经注册。");
                    }
                }
            }
        }

        void IRpcParser.OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var item in methodInstances)
            {
                if (item.GetAttribute<RRQMRPCCallBackAttribute>() is RRQMRPCCallBackAttribute attribute)
                {
                    string key = CodeGenerator.GetMethodName<RRQMRPCCallBackAttribute>(item);
                    this.callbackMap.TryRemove(key, out _);
                }
            }
        }

        void IRpcParser.SetExecuteMethod(Action<IRpcParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.executeMethod = executeMethod;
        }

        void IRpcParser.SetMethodMap(MethodMap methodMap)
        {
            this.methodMap = methodMap;
        }

        void IRpcParser.SetRpcService(RpcService service)
        {
            this.RpcService = service;
        }

        #endregion RPC解析器

        /// <summary>
        /// 协议数据
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected override sealed void HandleProtocolData(short protocol, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            switch (protocol)
            {
                case 100:/* 100表示获取Rpc引用文件上传状态返回*/
                    {
                        break;
                    }

                case 101:/*函数调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext result = RpcContext.Deserialize(byteBlock);
                            this.WaitHandlePool.SetRun(result.Sign, result);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*获取服务*/
                    {
                        try
                        {
                            DiscoveryServiceWaitResult result = SerializeConvert.RRQMBinaryDeserialize<DiscoveryServiceWaitResult>(buffer, 2);
                            this.WaitHandlePool.SetRun(result);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 103:/*ID函数调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext result = RpcContext.Deserialize(byteBlock);
                            this.WaitHandlePool.SetRun(result.Sign, result);
                        }
                        catch (Exception e)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: 103, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 104:/*反向函数调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext rpcContext = RpcContext.Deserialize(byteBlock);
                            this.P104_Execute(rpcContext);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, $"错误代码: {protocol}, 错误详情:{ex.Message}");
                        }
                        break;
                    }
                case 106:
                case 107:
                case 108:
                case 109:
                case 111:
                    {
                        break;
                    }

                case 110:
                    {
                        break;
                    }
                case 113:
                    {
                        try
                        {
                            long sign = RRQMBitConverter.Default.ToInt64(byteBlock.Buffer, 2);
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

                default:
                    {
                        this.RpcHandleDefaultData(protocol, byteBlock);
                        break;
                    }
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected override void LoadConfig(RRQMConfig config)
        {
            this.serializationSelector = (SerializationSelector)config.GetValue(RRQMRPCConfigExtensions.SerializationSelectorProperty);
            base.LoadConfig(config);
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
        /// Rpc完成初始化后
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

        /// <summary>
        /// Rpc处理其余协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void RpcHandleDefaultData(short protocol, ByteBlock byteBlock)
        {
            this.OnHandleDefaultData(protocol, byteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDisconnected(ClientDisconnectedEventArgs e)
        {
           var keys= contextDic.Keys.ToArray();
            foreach (var item in keys)
            {
                if (contextDic.TryRemove(item, out RpcCallContext rpcCallContext))
                {
                    rpcCallContext.TryCancel();
                }
            }
            base.OnDisconnected(e);
        }

        private void CanceledIDInvoke(object obj)
        {
            if (obj is CanceledID canceled)
            {
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    this.InternalSend(113, byteBlock.Write(canceled.id).Write(canceled.sign));
                }
            }
        }

        private void CanceledServiceInvoke(object obj)
        {
            if (obj is int sign)
            {
                this.InternalSend(105, RRQMBitConverter.Default.GetBytes(sign));
            }
        }

        private void ExecuteContext(RpcContext context)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = this;
            methodInvoker.Flag = context;
            if (this.callbackMap.TryGetValue(context.methodName, out MethodInstance methodInstance))
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

                this.executeMethod.Invoke(this, methodInvoker, methodInstance);
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
                this.executeMethod.Invoke(this, methodInvoker, null);
            }
        }

        private void P104_Execute(RpcContext context)
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
                    this.InternalSend(104, returnByteBlock.Buffer, 0, returnByteBlock.Len);
                }
                finally
                {
                    context.parametersBytes = ps;
                    returnByteBlock.Dispose();
                }
            }

            this.ExecuteContext(context);
        }
    }
}