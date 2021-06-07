using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCClient
    /// </summary>
    public class TcpRPCClient : IUserClient, IRPCClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpRPCClient()
        {
            this.client = new SimpleTokenClient();
            this.SerializeConverter = new BinarySerializeConverter();
            this.methodStore = new MethodStore();
            this.singleWaitData = new WaitData<WaitResult>();
            this.singleWaitData.WaitResult = new WaitResult();
            this.waitHandle = new RRQMWaitHandle<RpcContext>();
        }

        /// <summary>
        /// 收到ByteBlock时触发
        /// </summary>
        public event RRQMByteBlockEventHandler ReceivedByteBlock;

        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        private MethodMap methodMap;
        /// <summary>
        /// 获取反向RPC映射图
        /// </summary>
        public MethodMap MethodMap
        {
            get { return methodMap; }
        }

        /// <summary>
        /// 获取ID
        /// </summary>
        public string ID => this.client.ID;

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get => this.client.Logger; set => this.client.Logger=value; }

        /// <summary>
        /// 内存池
        /// </summary>
        public BytePool BytePool => this.client.BytePool;

        private RRQMWaitHandle<RpcContext> waitHandle;
        private WaitData<WaitResult> singleWaitData;
        internal MethodStore methodStore;
        private RPCProxyInfo proxyFile;
        private RRQMAgreementHelper agreementHelper;
        private SimpleTokenClient client;

        /// <summary>
        /// 获取函数注册
        /// </summary>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMRPCException"></exception>
        internal MethodStore GetMethodStore()
        {
            this.agreementHelper = new RRQMAgreementHelper(this.client);
            try
            {
                this.methodStore = null;
                agreementHelper.SocketSend(102);
                this.singleWaitData.Wait(1000 * 10);
            }
            catch (Exception e)
            {
                throw new RRQMRPCException(e.Message);
            }
            return this.methodStore;
        }

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <param name="proxyToken">代理令箭</param>
        /// <returns></returns>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public RPCProxyInfo GetProxyInfo(string proxyToken)
        {
            agreementHelper.SocketSend(100, proxyToken);
            this.singleWaitData.Wait(1000 * 100);

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
        /// 连接
        /// </summary>
        public void Connect()
        {
            this.client.Connect();
            this.agreementHelper = new RRQMAgreementHelper(this.client);
        }

        /// <summary>
        /// RPC调用，无返回值时请设置T为<see cref="Nullable"/>。
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public T Invoke<T>(string method, InvokeOption invokeOption, ref object[] parameters)
        {
            MethodItem methodItem = this.methodStore.GetMethodItem(method);
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
                            parameters[i] = this.SerializeConverter.DeserializeParameter(context.ParametersBytes[i], methodItem.ParameterTypes[i]);
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
                    return (T)this.SerializeConverter.DeserializeParameter(context.ReturnParameterBytes, methodItem.ReturnType);
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
        /// 处理已接收到的数据
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
                case 111:/*收到服务器数据*/
                    {
                        ByteBlock block = this.BytePool.GetByteBlock(r - 4);
                        try
                        {
                            block.Write(byteBlock.Buffer, 4, r - 4);
                            ReceivedByteBlock?.Invoke(this, block);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 111, 错误详情:{e.Message}");
                        }
                        finally
                        {
                            block.Dispose();
                        }
                        break;
                    }
                case 112:/*反向函数调用返回*/
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
            }
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

        public void ConnectAsync(Action<AsyncResult> callback = null)
        {
            throw new NotImplementedException();
        }

        public void Setup(TcpClientConfig clientConfig)
        {
            throw new NotImplementedException();
        }

        public RPCProxyInfo GetProxyInfo(IPHost ipHost, string verifyToken = null, string proxyToken = null)
        {
            throw new NotImplementedException();
        }

        public void InitializedRPC(IPHost ipHost, string verifyToken = null, TypeInitializeDic typeDic = null)
        {
            throw new NotImplementedException();
        }

        public void RPCInvoke(string method, InvokeOption invokeOption = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public T RPCInvoke<T>(string method, InvokeOption invokeOption = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public object Invoke(string method, InvokeOption invokeOption, ref object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
