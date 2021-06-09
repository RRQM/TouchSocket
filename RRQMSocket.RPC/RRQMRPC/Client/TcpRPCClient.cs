//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCClient
    /// </summary>
    public class TcpRPCClient : TokenClient, IRPCClient
    {
        private RRQMAgreementHelper agreementHelper;

        private MethodMap methodMap;

        private MethodStore methodStore;

        private RPCProxyInfo proxyFile;

        private ServerProviderCollection serverProviders;

        private WaitData<WaitResult> singleWaitData;

        private RRQMWaitHandle<RpcContext> waitHandle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpRPCClient()
        {
            this.serverProviders = new ServerProviderCollection();
            this.methodStore = new MethodStore();
            this.singleWaitData = new WaitData<WaitResult>();
            this.singleWaitData.WaitResult = new WaitResult();
            this.waitHandle = new RRQMWaitHandle<RpcContext>();
        }

        /// <summary>
        /// 收到字节
        /// </summary>
        public event RRQMBytesEventHandler Received;

        /// <summary>
        /// 获取反向RPC映射图
        /// </summary>
        public MethodMap MethodMap
        {
            get { return methodMap; }
        }

        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 获取反向RPC服务实例
        /// </summary>
        public ServerProviderCollection ServerProviders
        {
            get { return serverProviders; }
        }

        /// <summary>
        /// 连接
        /// </summary>
        public override void Connect()
        {
            lock (this)
            {
                base.Connect();
                this.agreementHelper = new RRQMAgreementHelper(this);
                try
                {
                    this.methodStore = null;
                    agreementHelper.SocketSend(102);
                }
                catch (Exception e)
                {
                    throw new RRQMRPCException(e.Message);
                }
                this.singleWaitData.Wait(1000 * 10);
                if (this.methodStore == null)
                {
                    throw new RRQMRPCException("初始化超时");
                }
            }
        }

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public RPCProxyInfo GetProxyInfo()
        {
            agreementHelper.SocketSend(100, (string)this.ClientConfig.GetValue(TcpRPCClientConfig.ProxyTokenProperty));
            this.singleWaitData.Wait(1000 * 10);

            if (this.proxyFile == null)
            {
                throw new RRQMTimeoutException("获取引用文件超时");
            }
            else if (this.proxyFile.Status == 2)
            {
                throw new RRQMRPCException(this.proxyFile.Message);
            }
            return this.proxyFile;
        }

        /// <summary>
        /// 初始化RPC
        /// </summary>
        public void InitializedRPC()
        {
            if (!this.Online)
            {
                this.Connect();
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
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMException($"服务名为{method}的服务未找到注册信息");
            }
            WaitData<RpcContext> waitData = this.waitHandle.GetWaitData();
            waitData.WaitResult.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    waitData.WaitResult.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                waitData.WaitResult.ParametersBytes = datas;
                waitData.WaitResult.Serialize(byteBlock);
                agreementHelper.SocketSend(101, byteBlock);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
            if (invokeOption.Feedback)
            {
                waitData.Wait(invokeOption.WaitTime * 1000);
                RpcContext context = waitData.WaitResult;
                waitData.Dispose();

                if (context.Status == 0)
                {
                    throw new RRQMTimeoutException("等待结果超时");
                }
                else if (context.Status == 2)
                {
                    throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCMethod");
                }
                else if (context.Status == 3)
                {
                    throw new RRQMRPCException("该方法已被禁用");
                }
                else if (context.Status == 4)
                {
                    throw new RRQMRPCException($"服务器已阻止本次行为，信息：{context.MethodToken}");
                }
                if (methodItem.IsOutOrRef)
                {
                    try
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            parameters[i] = this.SerializeConverter.DeserializeParameter(context.ParametersBytes[i], types[i]);
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
                try
                {
                    return (T)this.SerializeConverter.DeserializeParameter(context.ReturnParameterBytes, typeof(T));
                }
                catch (Exception e)
                {
                    throw new RRQMException(e.Message);
                }
            }
            else
            {
                waitData.Dispose();
                return default;
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
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMException($"服务名为{method}的服务未找到注册信息");
            }
            WaitData<RpcContext> waitData = this.waitHandle.GetWaitData();
            waitData.WaitResult.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    waitData.WaitResult.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                waitData.WaitResult.ParametersBytes = datas;
                waitData.WaitResult.Serialize(byteBlock);
                agreementHelper.SocketSend(101, byteBlock);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
            if (invokeOption.Feedback)
            {
                waitData.Wait(invokeOption.WaitTime * 1000);
                RpcContext context = waitData.WaitResult;
                waitData.Dispose();

                if (context.Status == 0)
                {
                    throw new RRQMTimeoutException("等待结果超时");
                }
                else if (context.Status == 2)
                {
                    throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCMethod");
                }
                else if (context.Status == 3)
                {
                    throw new RRQMRPCException("该方法已被禁用");
                }
                else if (context.Status == 4)
                {
                    throw new RRQMRPCException($"服务器已阻止本次行为，信息：{context.MethodToken}");
                }
                if (methodItem.IsOutOrRef)
                {
                    try
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            parameters[i] = this.SerializeConverter.DeserializeParameter(context.ParametersBytes[i], types[i]);
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
            }
            else
            {
                waitData.Dispose();
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
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, InvokeOption invokeOption, params object[] parameters)
        {
            Type[] types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].GetType();
            }
            this.Invoke(method, invokeOption, ref parameters, types);
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
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, params object[] parameters)
        {
            Type[] types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].GetType();
            }
            return this.Invoke<T>(method, invokeOption, ref parameters, types);
        }

        /// <summary>
        /// 开启反向RPC服务
        /// </summary>
        public void OpenCallBackServer()
        {
            if (this.ServerProviders.Count == 0)
            {
                throw new RRQMRPCException("已注册服务数量为0");
            }

            this.methodMap = new MethodMap();

            foreach (ServerProvider instance in this.ServerProviders)
            {
                MethodInfo[] methodInfos = instance.GetType().GetMethods();
                foreach (MethodInfo method in methodInfos)
                {
                    if (method.IsGenericMethod)
                    {
                        throw new RRQMRPCException("RPC方法中不支持泛型参数");
                    }
                    RRQMRPCCallBackMethodAttribute attribute = method.GetCustomAttribute<RRQMRPCCallBackMethodAttribute>();

                    if (attribute != null)
                    {
                        MethodInstance methodInstance = new MethodInstance();
                        methodInstance.MethodToken = attribute.MethodToken;
                        methodInstance.Provider = instance;
                        methodInstance.Method = method;
                        methodInstance.RPCAttributes = new RPCMethodAttribute[] { attribute };
                        methodInstance.IsEnable = true;
                        methodInstance.Parameters = method.GetParameters();
                        List<string> names = new List<string>();
                        foreach (var parameterInfo in methodInstance.Parameters)
                        {
                            names.Add(parameterInfo.Name);
                        }
                        methodInstance.ParameterNames = names.ToArray();
                        if (typeof(Task).IsAssignableFrom(method.ReturnType))
                        {
                            methodInstance.Async = true;
                        }

                        ParameterInfo[] parameters = method.GetParameters();
                        List<Type> types = new List<Type>();
                        foreach (var parameter in parameters)
                        {
                            if (parameter.ParameterType.IsByRef)
                            {
                                throw new RRQMRPCException("反向RPC方法不支持out或ref");
                            }
                            types.Add(parameter.ParameterType);
                        }
                        methodInstance.ParameterTypes = types.ToArray();

                        if (method.ReturnType == typeof(void))
                        {
                            methodInstance.ReturnType = null;
                        }
                        else
                        {
                            if (methodInstance.Async)
                            {
                                methodInstance.ReturnType = method.ReturnType.GetGenericArguments()[0];
                            }
                            else
                            {
                                methodInstance.ReturnType = method.ReturnType;
                            }
                        }

                        try
                        {
                            this.MethodMap.Add(methodInstance);
                        }
                        catch
                        {
                            throw new RRQMRPCKeyException("MethodToken必须唯一");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverProvider"></param>
        public void RegistServer(ServerProvider serverProvider)
        {
            this.ServerProviders.Add(serverProvider);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void Send(byte[] buffer, int offset, int length)
        {
            this.agreementHelper.SocketSend(120, buffer, offset, length);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void SendAsync(byte[] buffer, int offset, int length)
        {
            this.agreementHelper.SocketSend(120, buffer, offset, length);
        }

        /// <summary>
        /// 收到信息
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Length;
            int agreement = BitConverter.ToInt32(buffer, 0);
            switch (agreement)
            {
                case 100:/* 100表示获取RPC引用文件上传状态返回*/
                    {
                        try
                        {
                            proxyFile = SerializeConvert.RRQMBinaryDeserialize<RPCProxyInfo>(buffer, 4);
                            this.singleWaitData.Set();
                        }
                        catch
                        {
                            proxyFile = null;
                        }

                        break;
                    }

                case 101:/*函数调用返回数据对象*/
                    {
                        try
                        {
                            RpcContext result = RpcContext.Deserialize(buffer, 4);
                            this.waitHandle.SetRun(result.Sign, result);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 102:/*获取服务*/
                    {
                        try
                        {
                            List<MethodItem> methodItems = SerializeConvert.RRQMBinaryDeserialize<List<MethodItem>>(buffer, 4);
                            this.methodStore = new MethodStore();
                            foreach (var item in methodItems)
                            {
                                this.methodStore.AddMethodItem(item);
                            }

                            this.singleWaitData.Set();
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 112:/*反向函数调用*/
                    {
                        Task.Run(() =>
                        {
                            RpcContext rpcContext = RpcContext.Deserialize(byteBlock.Buffer, 4);
                            ByteBlock block = this.BytePool.GetByteBlock(this.BufferLength);
                            try
                            {
                                rpcContext = this.OnExecuteCallBack(rpcContext);
                                rpcContext.Serialize(block);
                                agreementHelper.SocketSend(112, block);
                            }
                            catch (Exception e)
                            {
                                Logger.Debug(LogType.Error, this, $"错误代码: 112, 错误详情:{e.Message}");
                            }
                            finally
                            {
                                block.Dispose();
                            }
                        });

                        break;
                    }
                case 120:/*接收数据*/
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                byte[] data = new byte[r-4];
                                byteBlock.Position = 4;
                                byteBlock.Read(data);
                                this.Received?.Invoke(this,new BytesEventArgs(data));
                            }
                            catch (Exception e)
                            {
                                Logger.Debug(LogType.Error, this, $"错误代码: 120, 错误详情:{e.Message}");
                            }
                        });

                        break;
                    }
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
            this.SerializeConverter = (SerializeConverter)clientConfig.GetValue(TcpRPCClientConfig.SerializeConverterProperty);
        }

        private RpcContext OnExecuteCallBack(RpcContext rpcContext)
        {
            if (this.methodMap != null)
            {
                if (this.methodMap.TryGet(rpcContext.MethodToken, out MethodInstance methodInstance))
                {
                    try
                    {
                        object[] ps = new object[rpcContext.ParametersBytes.Count];
                        for (int i = 0; i < rpcContext.ParametersBytes.Count; i++)
                        {
                            ps[i] = this.SerializeConverter.DeserializeParameter(rpcContext.ParametersBytes[i], methodInstance.ParameterTypes[i]);
                        }
                        object result = methodInstance.Method.Invoke(methodInstance.Provider, ps);
                        if (result != null)
                        {
                            rpcContext.ReturnParameterBytes = this.SerializeConverter.SerializeParameter(result);
                        }
                        rpcContext.Status = 1;
                    }
                    catch (Exception ex)
                    {
                        rpcContext.Status = 4;
                        rpcContext.Message = ex.Message;
                    }
                }
                else
                {
                    rpcContext.Status = 2;
                }
            }
            else
            {
                rpcContext.Status = 3;
            }

            rpcContext.ParametersBytes = null;
            return rpcContext;
        }
    }
}