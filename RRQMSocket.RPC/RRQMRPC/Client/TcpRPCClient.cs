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
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCClient
    /// </summary>
    public class TcpRPCClient : ProtocolClient, IRRQMRPCClient
    {
        private MethodMap methodMap;

        private MethodStore methodStore;

        private RPCProxyInfo proxyFile;

        private ServerProviderCollection serverProviders;

        private WaitData<WaitResult> singleWaitData;

        private RRQMWaitHandle<WaitResult> waitHandle;

        static TcpRPCClient()
        {
            AddUsedProtocol(100, "请求RPC代理文件");
            AddUsedProtocol(101, "RPC调用");
            AddUsedProtocol(102, "获取注册服务");
            AddUsedProtocol(103, "ID调用客户端");
            AddUsedProtocol(104, "RPC回调");
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpRPCClient()
        {
            this.methodMap = new MethodMap();
            this.serverProviders = new ServerProviderCollection();
            this.methodStore = new MethodStore();
            this.singleWaitData = new WaitData<WaitResult>();
            this.waitHandle = new RRQMWaitHandle<WaitResult>();
        }

        /// <summary>
        /// 收到协议数据
        /// </summary>
        public event RRQMReceivedProcotolEventHandler Received;

        /// <summary>
        /// RPC初始化后
        /// </summary>
        public event RRQMMessageEventHandler ServiceDiscovered;

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
        public SerializeConverter SerializeConverter { get; private set; }

        /// <summary>
        /// 获取反向RPC服务实例
        /// </summary>
        public ServerProviderCollection ServerProviders
        {
            get { return serverProviders; }
        }

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public RPCProxyInfo GetProxyInfo()
        {
            string proxyToken = (string)this.ClientConfig.GetValue(TcpRPCClientConfig.ProxyTokenProperty);
            byte[] data = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(proxyToken) ? string.Empty : proxyToken);
            this.InternalSend(100, data, 0, data.Length);
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
        /// 发现服务
        /// </summary>
        /// <param name="isTrigger">是否触发初始化事件</param>
        /// <returns>已发现的服务</returns>
        public MethodItem[] DiscoveryService(bool isTrigger = true)
        {
            lock (locker)
            {
                if (!this.Online)
                {
                    base.Connect();
                }
                try
                {
                    this.methodStore = null;
                    string proxyToken = (string)this.clientConfig.GetValue(TcpRPCClientConfig.ProxyTokenProperty);
                    byte[] data = new byte[0];
                    if (!string.IsNullOrEmpty(proxyToken))
                    {
                        data = Encoding.UTF8.GetBytes(proxyToken);
                    }
                    this.InternalSend(102, data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                this.singleWaitData.Wait(1000 * 10);
                if (this.methodStore == null)
                {
                    throw new RRQMRPCException("初始化超时");
                }

                if (isTrigger)
                {
                    this.OnServiceDiscovered(new MesEventArgs("success"));
                }
                return this.methodStore.GetAllMethodItem().ToArray();
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
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RPCContext context = new RPCContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                context.Feedback = (byte)invokeOption.FeedbackType;

                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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
                        this.waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        else
                        {
                            return default;
                        }
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
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
                            throw new RRQMRPCInvokeException("函数执行异常，详细信息："+resultContext.Message);
                        }
                        else if (resultContext.Status == 6)
                        {
                            throw new RRQMRPCException($"函数异常，信息：{resultContext.Message}");
                        }
                        if (methodItem.IsOutOrRef)
                        {
                            try
                            {
                                for (int i = 0; i < parameters.Length; i++)
                                {
                                    parameters[i] = this.SerializeConverter.DeserializeParameter(resultContext.ParametersBytes[i], types[i]);
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
                            return (T)this.SerializeConverter.DeserializeParameter(resultContext.ReturnParameterBytes, typeof(T));
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                default:
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
        /// <exception cref="RRQMRPCNoRegisterException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RPCContext context = new RPCContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                context.Feedback = (byte)invokeOption.FeedbackType;
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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
                        this.waitHandle.Destroy(waitData);
                        break;
                    }
                case FeedbackType.WaitSend:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        break;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
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
                        if (methodItem.IsOutOrRef)
                        {
                            try
                            {
                                for (int i = 0; i < parameters.Length; i++)
                                {
                                    parameters[i] = this.SerializeConverter.DeserializeParameter(resultContext.ParametersBytes[i], types[i]);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        else
                        {
                            parameters = null;
                        }
                        break;
                    }
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
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RPCContext context = new RPCContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                context.Feedback = (byte)invokeOption.FeedbackType;
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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
                        this.waitHandle.Destroy(waitData);
                        break;
                    }
                case FeedbackType.WaitSend:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        break;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
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
                default:
                    break;
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
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMRPCNoRegisterException($"服务名为{method}的服务未找到注册信息");
            }
            RPCContext context = new RPCContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                context.Feedback = (byte)invokeOption.FeedbackType;
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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
                        this.waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        else
                        {
                            return default;
                        }
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
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

                        try
                        {
                            return (T)this.SerializeConverter.DeserializeParameter(resultContext.ReturnParameterBytes, typeof(T));
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="methodToken">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string id, int methodToken, InvokeOption invokeOption, params object[] parameters)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new RRQMRPCException("目标ID不能为null或empty");
            }
            RPCContext context = new RPCContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodToken;
            context.ID = id;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                context.Feedback = (byte)invokeOption.FeedbackType;
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(103, byteBlock.Buffer, 0, byteBlock.Len);
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
                        this.waitHandle.Destroy(waitData);
                    }
                    break;

                case FeedbackType.WaitSend:
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        else if (resultContext.Status == 2)
                        {
                            throw new RRQMRPCInvokeException("未找到该客户端ID");
                        }
                        else if (resultContext.Status == 3)
                        {
                            throw new RRQMRPCException(resultContext.Message);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="methodToken">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string id, int methodToken, InvokeOption invokeOption, params object[] parameters)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new RRQMRPCException("目标ID不能为null或empty");
            }
            RPCContext context = new RPCContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodToken;
            context.ID = id;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                context.Feedback = (byte)invokeOption.FeedbackType;
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(103, byteBlock.Buffer, 0, byteBlock.Len);
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
                        this.waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        RPCContext resultContext = (RPCContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        else if (resultContext.Status == 2)
                        {
                            throw new RRQMRPCInvokeException("未找到该客户端ID");
                        }
                        else if (resultContext.Status == 3)
                        {
                            throw new RRQMRPCException(resultContext.Message);
                        }

                        try
                        {
                            return (T)this.SerializeConverter.DeserializeParameter(resultContext.ReturnParameterBytes, typeof(T));
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回T实例</returns>
        public ServerProvider RegisterServer<T>() where T : ServerProvider
        {
            ServerProvider serverProvider = (ServerProvider)Activator.CreateInstance(typeof(T));
            this.RegisterServer(serverProvider);
            return serverProvider;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public ServerProvider RegisterServer(Type providerType)
        {
            if (!typeof(ServerProvider).IsAssignableFrom(providerType))
            {
                throw new RRQMRPCException("类型不相符");
            }
            ServerProvider serverProvider = (ServerProvider)Activator.CreateInstance(providerType);
            this.RegisterServer(serverProvider);
            return serverProvider;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverProvider"></param>
        public void RegisterServer(ServerProvider serverProvider)
        {
            this.ServerProviders.Add(serverProvider);
            MethodInfo[] methodInfos = serverProvider.GetType().GetMethods();
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
                    methodInstance.Provider = serverProvider;
                    methodInstance.Method = method;
                    methodInstance.RPCAttributes = new RPCAttribute[] { attribute };
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


        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public int UnregisterServer(ServerProvider provider)
        {
            return this.UnregisterServer(provider.GetType());
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public int UnregisterServer(Type providerType)
        {
            if (!typeof(ServerProvider).IsAssignableFrom(providerType))
            {
                throw new RRQMRPCException("类型不相符");
            }
            this.ServerProviders.Remove(providerType);
            if (this.MethodMap.RemoveServer(providerType, out ServerProvider serverProvider, out MethodInstance[] instances))
            {
                return instances.Length;
            }
            return 0;
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int UnregisterServer<T>() where T : ServerProvider
        {
            return this.UnregisterServer(typeof(T));
        }

        /// <summary>
        /// 协议数据
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected sealed override void HandleProtocolData(short? procotol, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            switch (procotol)
            {
                case 100:/* 100表示获取RPC引用文件上传状态返回*/
                    {
                        try
                        {
                            proxyFile = SerializeConvert.RRQMBinaryDeserialize<RPCProxyInfo>(buffer, 2);
                            this.singleWaitData.Set();
                        }
                        catch
                        {
                            proxyFile = null;
                        }

                        break;
                    }

                case 101:/*函数调用*/
                    {
                        try
                        {
                            RPCContext result = RPCContext.Deserialize(buffer, 2);
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
                            List<MethodItem> methodItems = SerializeConvert.RRQMBinaryDeserialize<List<MethodItem>>(buffer, 2);
                            this.methodStore = new MethodStore();
                            if (methodItems != null)
                            {
                                foreach (var item in methodItems)
                                {
                                    this.methodStore.AddMethodItem(item);
                                }
                            }
                            this.singleWaitData.Set();
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 103:/*ID函数调用*/
                    {
                        try
                        {
                            RPCContext result = RPCContext.Deserialize(buffer, 2);
                            this.waitHandle.SetRun(result.Sign, result);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 103, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 104:/*反向函数调用*/
                    {
                        Task.Run(() =>
                        {
                            RPCContext rpcContext = RPCContext.Deserialize(byteBlock.Buffer, 2);
                            ByteBlock block = this.BytePool.GetByteBlock(this.BufferLength);
                            try
                            {
                                rpcContext = this.OnExecuteCallBack(rpcContext);
                                rpcContext.Serialize(block);
                                this.InternalSend(104, block.Buffer, 0, (int)block.Length);
                            }
                            catch (Exception e)
                            {
                                Logger.Debug(LogType.Error, this, $"错误代码: 104, 错误详情:{e.Message}");
                            }
                            finally
                            {
                                block.Dispose();
                            }
                        });

                        break;
                    }
                default:
                    {
                        RPCHandleDefaultData(procotol, byteBlock);
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
            this.SetDataHandlingAdapter(new FixedHeaderDataHandlingAdapter());
            this.SerializeConverter = (SerializeConverter)clientConfig.GetValue(TcpRPCClientConfig.SerializeConverterProperty);
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
        /// RPC完成初始化后
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnServiceDiscovered(MesEventArgs args)
        {
            this.ServiceDiscovered?.Invoke(this, args);
        }

        /// <summary>
        /// RPC处理其余协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void RPCHandleDefaultData(short? procotol, ByteBlock byteBlock)
        {
            OnHandleDefaultData(procotol, byteBlock);
        }

        private RPCContext OnExecuteCallBack(RPCContext rpcContext)
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