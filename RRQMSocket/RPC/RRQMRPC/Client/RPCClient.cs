//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Pool;
using System;
using System.Collections.Concurrent;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 集群RPC客户端
    /// </summary>
    public sealed class RPCClient : IRPCClient
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
        public RPCProxyInfo GetProxyInfo(string ipHost, string verifyToken = null, string proxyToken = null)
        {
            this.iPHost = new IPHost(ipHost);
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
        /// 初始化RPC
        /// </summary>
        /// <param name="ipHost"></param>
        /// <param name="verifyToken"></param>
        /// <param name="typeDic"></param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void InitializedRPC(string ipHost, string verifyToken = null, TypeInitializeDic typeDic = null)
        {
            this.iPHost = new IPHost(ipHost);
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
        public T RPCInvoke<T>(string method, ref object[] parameters, InvokeOption invokeOption)
        {
            RpcJunctor rpcJunctor = this.GetRpcJunctor();
            try
            {
                return rpcJunctor.RPCInvoke<T>(method, ref parameters, invokeOption);
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
        public void RPCInvoke(string method, ref object[] parameters, InvokeOption invokeOption)
        {
            RpcJunctor rpcJunctor = this.GetRpcJunctor();
            try
            {
                rpcJunctor.RPCInvoke(method, ref parameters, invokeOption);
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

        /// <summary>
        /// 快捷调用RPC
        /// </summary>
        /// <param name="host">IP及端口</param>
        /// <param name="methodKey">函数键</param>
        /// <param name="verifyToken">验证Token</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        public static void CallRPC(string host, string methodKey, string verifyToken = null, InvokeOption invokeOption = null, params object[] parameters)
        {
            RPCClient client;
            if (rpcDic.TryGetValue(host, out client))
            {
                client.RPCInvoke(methodKey, ref parameters, invokeOption);
                return;
            }
            client = new RPCClient();
            client.InitializedRPC(host, verifyToken);
            rpcDic.TryAdd(host, client);
            client.RPCInvoke(methodKey, ref parameters, invokeOption);
        }

        /// <summary>
        /// 快捷调用RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="host">IP及端口</param>
        /// <param name="methodKey">函数键</param>
        /// <param name="verifyToken">验证Token</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public static T CallRPC<T>(string host, string methodKey, string verifyToken = null, InvokeOption invokeOption = null, params object[] parameters)
        {
            RPCClient client;
            if (rpcDic.TryGetValue(host, out client))
            {
                return client.RPCInvoke<T>(methodKey, ref parameters, invokeOption);
            }
            client = new RPCClient();
            client.InitializedRPC(host, verifyToken);
            rpcDic.TryAdd(host, client);
            return client.RPCInvoke<T>(methodKey, ref parameters, invokeOption);
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