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
using RRQMCore.Pool;
using RRQMCore.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 集群RPC客户端
    /// </summary>
    public  class RPCClient : IRPCClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RPCClient()
        {
            this.rpcJunctorPool = new ObjectPool<RpcJunctor>();
            this.Capacity = 10;
            this.BytePool = new BytePool();
            BinarySerializeConverter serializeConverter = new BinarySerializeConverter();
            this.SerializeConverter = serializeConverter;
            this.methodStore = new MethodStore();
            this.Logger = new Log();
            this.ServerProviders = new ServerProviderCollection();
        }

        private string verifyToken;

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        public event RRQMBytesEventHandler ReceivedBytesThenReturn;

        /// <summary>
        /// 收到ByteBlock时触发
        /// </summary>
        public event RRQMByteBlockEventHandler ReceivedByteBlock;

        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int Capacity { get { return this.rpcJunctorPool.Capacity; } set { this.rpcJunctorPool.Capacity = value; } }

        private ILog logger;

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger
        {
            get { return logger; }
            set
            {
                logger = value;
            }
        }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get; private set; }

        /// <summary>
        /// 获取即将在下一次通信的客户端单体
        /// </summary>
        public RpcJunctor NextJunctor { get { return this.rpcJunctorPool.PreviewGetObject(); } }

        /// <summary>
        /// 获取即将在下一次通信的客户端单体的ID
        /// </summary>
        public string ID
        {
            get
            {
                RpcJunctor rpcJunctor = this.rpcJunctorPool.PreviewGetObject();
                if (rpcJunctor != null)
                {
                    return rpcJunctor.ID;
                }
                return null;
            }
        }

        /// <summary>
        /// RPC连接池
        /// </summary>
        public ObjectPool<RpcJunctor> RpcJunctorPool { get { return rpcJunctorPool; } }

        /// <summary>
        /// 获取RPC快捷调用实例字典
        /// </summary>
        public static ConcurrentDictionary<string, RPCClient> RPCCacheDic { get { return rpcDic; } }

        /// <summary>
        /// 获取反向RPC服务实例
        /// </summary>
        public ServerProviderCollection ServerProviders { get; private set; }

        /// <summary>
        /// 获取反向RPC映射图
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        private static ConcurrentDictionary<string, RPCClient> rpcDic = new ConcurrentDictionary<string, RPCClient>();
        private IPHost iPHost;
        private bool _disposed;
        private MethodStore methodStore;
        private RPCProxyInfo proxyFile;
        private ObjectPool<RpcJunctor> rpcJunctorPool;

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <param name="ipHost">IP和端口</param>
        /// <param name="verifyToken">连接验证</param>
        /// <param name="proxyToken">代理令箭</param>
        /// <returns></returns>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public RPCProxyInfo GetProxyInfo(IPHost ipHost, string verifyToken = null, string proxyToken = null)
        {
            this.iPHost = ipHost;
            this.verifyToken = verifyToken;
            lock (this)
            {
                RpcJunctor rpcJunctor = this.GetRpcJunctor();
                this.proxyFile = rpcJunctor.GetProxyInfo(proxyToken);
                this.rpcJunctorPool.DestroyObject(rpcJunctor);
                return this.proxyFile;
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverProvider"></param>
        public void RegistService(ServerProvider serverProvider)
        {
            this.ServerProviders.Add(serverProvider);
        }

        /// <summary>
        /// 开启反向RPC服务
        /// </summary>
        public void OpenCallBackRPCServer()
        {
            if (this.ServerProviders.Count == 0)
            {
                throw new RRQMRPCException("已注册服务数量为0");
            }


            this.MethodMap = new MethodMap();

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
        /// 初始化RPC
        /// </summary>
        /// <param name="ipHost"></param>
        /// <param name="verifyToken"></param>
        /// <param name="typeDic"></param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void InitializedRPC(IPHost ipHost, string verifyToken = null, TypeInitializeDic typeDic = null)
        {
            this.iPHost = ipHost;
            this.verifyToken = verifyToken;
            RpcJunctor rpcJunctor = this.GetRpcJunctor();
            this.methodStore = rpcJunctor.GetMethodStore();

            if (this.methodStore != null)
            {
                rpcJunctor.methodStore = this.methodStore;
                this.methodStore.InitializedType(typeDic);
                this.rpcJunctorPool.DestroyObject(rpcJunctor);
            }
            else
            {
                throw new ArgumentNullException("函数映射为空");
            }
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns>服务器返回结果</returns>
        public T RPCInvoke<T>(string method, InvokeOption invokeOption = null, params object[] parameters)
        {
            RpcJunctor rpcJunctor = this.GetRpcJunctor();
            try
            {
                return rpcJunctor.RPCInvoke<T>(method, invokeOption, parameters);
            }
            finally
            {
                this.rpcJunctorPool.DestroyObject(rpcJunctor);
            }
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void RPCInvoke(string method, InvokeOption invokeOption = null, params object[] parameters)
        {
            RpcJunctor rpcJunctor = this.GetRpcJunctor();
            try
            {
                rpcJunctor.RPCInvoke(method, invokeOption, parameters);
            }
            finally
            {
                this.rpcJunctorPool.DestroyObject(rpcJunctor);
            }
        }
        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns>服务器返回结果</returns>
        public object Invoke(string method, InvokeOption invokeOption, ref object[] parameters)
        {
            RpcJunctor rpcJunctor = this.GetRpcJunctor();
            try
            {
                return rpcJunctor.Invoke(method,  invokeOption,ref parameters);
            }
            finally
            {
                this.rpcJunctorPool.DestroyObject(rpcJunctor);
            }
        }

        private int tryCount;

        private RpcJunctor GetRpcJunctor()
        {
            if (this._disposed)
            {
                throw new RRQMRPCException("无法利用已释放资源");
            }
            RpcJunctor rpcJunctor = this.rpcJunctorPool.PreviewGetObject();
            if (rpcJunctor == null)
            {
                if (this.rpcJunctorPool.FreeSize < this.Capacity)
                {
                    rpcJunctor = new RpcJunctor(this.BytePool);
                    rpcJunctor.VerifyToken = this.verifyToken;
                    try
                    {
                        rpcJunctor.Connect(this.iPHost.AddressFamily, this.iPHost.EndPoint);
                        rpcJunctor.ReceivedBytesThenReturn = this.OnReceivedBytesThenReturn;
                        rpcJunctor.ReceivedByteBlock = this.OnReceivedByteBlock;
                        rpcJunctor.ExecuteCallBack = this.OnExecuteCallBack;
                        rpcJunctor.Logger = this.logger;
                        rpcJunctor.SerializeConverter = this.SerializeConverter;
                        rpcJunctor.methodStore = this.methodStore;
                        this.tryCount = 0;
                        return rpcJunctor;
                    }
                    catch (Exception ex)
                    {
                        this.logger.Debug(LogType.Error, rpcJunctor, $"连接异常：{ex.Message}");
                        rpcJunctor.Dispose();
                        if (++tryCount >= this.Capacity)
                        {
                            throw new RRQMRPCException("重试次数达到上线");
                        }
                        return GetRpcJunctor();
                    }
                }
                else
                {
                    throw new RRQMRPCNoFreeException("无空闲RPC连接器，请稍后重试");
                }
            }
            else
            {
                rpcJunctor = this.rpcJunctorPool.GetObject();
                if (!rpcJunctor.Online)
                {
                    rpcJunctor.Dispose();
                    if (++tryCount >= this.Capacity)
                    {
                        throw new RRQMRPCException("重试次数达到上线");
                    }
                    return GetRpcJunctor();
                }
                tryCount = 0;
                return rpcJunctor;
            }
        }

        private void OnReceivedBytesThenReturn(object sender, BytesEventArgs e)
        {
            this.ReceivedBytesThenReturn?.Invoke(sender, e);
        }

        private void OnReceivedByteBlock(object sender, ByteBlock e)
        {
            this.ReceivedByteBlock?.Invoke(sender, e);
        }

        private RpcContext OnExecuteCallBack(RpcContext rpcContext)
        {
            if (this.MethodMap != null)
            {
                if (this.MethodMap.TryGet(rpcContext.MethodToken, out MethodInstance methodInstance))
                {
                    try
                    {
                        object[] ps = new object[rpcContext.ParametersBytes.Count];
                        for (int i = 0; i < rpcContext.ParametersBytes.Count; i++)
                        {
                            ps[i] = SerializeConvert.RRQMBinaryDeserialize(rpcContext.ParametersBytes[i], 0, methodInstance.ParameterTypes[i]);
                        }
                        object result = methodInstance.Method.Invoke(methodInstance.Provider, ps);
                        if (result != null)
                        {
                            rpcContext.ReturnParameterBytes = SerializeConvert.RRQMBinarySerialize(result, true);
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

        /// <summary>
        /// 快捷调用RPC
        /// </summary>
        /// <param name="iPHost">IP及端口</param>
        /// <param name="methodKey">函数键</param>
        /// <param name="verifyToken">验证Token</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        public static void CallRPC(string iPHost, string methodKey, string verifyToken = null, InvokeOption invokeOption = null, params object[] parameters)
        {
            RPCClient client;
            if (rpcDic.TryGetValue(iPHost, out client))
            {
                client.RPCInvoke(methodKey,  invokeOption,parameters);
                return;
            }
            client = new RPCClient();
            client.InitializedRPC(new IPHost(iPHost), verifyToken);
            rpcDic.TryAdd(iPHost, client);
            client.RPCInvoke(methodKey, invokeOption, parameters);
        }

        /// <summary>
        /// 快捷调用RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iPHost">IP及端口</param>
        /// <param name="methodKey">函数键</param>
        /// <param name="verifyToken">验证Token</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static T CallRPC<T>(string iPHost, string methodKey, string verifyToken = null, InvokeOption invokeOption = null, params object[] parameters)
        {
            RPCClient client;
            if (rpcDic.TryGetValue(iPHost, out client))
            {
                return client.RPCInvoke<T>(methodKey, invokeOption, parameters);
            }
            client = new RPCClient();
            client.InitializedRPC(new IPHost(iPHost), verifyToken);
            rpcDic.TryAdd(iPHost, client);
            return client.RPCInvoke<T>(methodKey, invokeOption, parameters);
        }

        /// <summary>
        /// 释放所占资源
        /// </summary>
        public void Dispose()
        {
            this._disposed = true;
            while (true)
            {
                RpcJunctor rpcJunctor = this.rpcJunctorPool.GetObject();
                if (rpcJunctor == null)
                {
                    break;
                }

                rpcJunctor.Dispose();
            }
        }
    }
}