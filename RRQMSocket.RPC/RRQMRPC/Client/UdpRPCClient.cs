using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UDP协议客户端
    /// </summary>
    public class UdpRPCClient : UdpSession, IRPCClient
    {
        private MethodStore methodStore;
        private WaitData<WaitResult> singleWaitData;
        private RPCProxyInfo proxyFile;
        private WaitResult waitResult;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpRPCClient()
        {
            this.SerializeConverter = new BinarySerializeConverter();
            this.waitResult = new WaitResult();
            this.Logger = new Log();
            this.singleWaitData = new WaitData<WaitResult>();
        }

        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 返回ID
        /// </summary>
        public string ID => null;

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public RPCProxyInfo GetProxyInfo()
        {
            int count = 0;
            while (count < 3)
            {
                lock (this)
                {
                    byte[] datas;
                    string proxyToken = (string)this.ServerConfig.GetValue(UdpRPCClientConfig.ProxyTokenProperty);
                    if (proxyToken == null)
                    {
                        datas = new byte[0];
                    }
                    else
                    {
                        datas = Encoding.UTF8.GetBytes(proxyToken);
                    }
                    this.UDPSend(100, datas, 0, datas.Length);
                    this.singleWaitData.Wait(5000);
                    if (this.proxyFile != null)
                    {
                        return this.proxyFile;
                    }
                }
                count++;
            }

            return null;
        }

        private void UDPSend(short procotol, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length + 2);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(procotol));
                byteBlock.Write(buffer, offset, length);
                this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
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

        private void UDPSend(short procotol, ByteBlock block)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(block.Length + 2);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(procotol));
                byteBlock.Write(block.Buffer, 0, (int)block.Length);
                this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
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

        private void UDPSend(short procotol)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(procotol));
                this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
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
        /// 密封数据
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
            int procotol = BitConverter.ToInt16(buffer, 0);
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

                case 101:/*函数调用返回数据对象*/
                    {
                        try
                        {
                            this.singleWaitData.Set(waitResult);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 102:/*连接初始化返回数据对象*/
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
            }
        }

        public RPCProxyInfo GetProxyInfo()
        {

        }

        public void InitializedRPC()
        {
            if (this.udpSession.MainSocket == null)
            {
                throw new RRQMRPCException("UDP端需要先绑定本地监听端口");
            }
            this.remoteService = ipHost.EndPoint;
            int count = 0;
            while (count < 3)
            {
                lock (this)
                {
                    try
                    {
                        this.methodStore = null;
                        this.UDPSend(102);
                        this.singleWaitData.Wait(1000 * 3);
                    }
                    catch (Exception e)
                    {
                        throw new RRQMRPCException(e.Message);
                    }
                    if (this.methodStore != null)
                    {
                        this.methodStore.InitializedType(typeDic);
                        return;
                    }
                }
                count++;
            }
            throw new RRQMTimeoutException("连接初始化超时");
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
            RPCContext context = new RPCContext();
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    context.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, (int)byteBlock.Length);
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
                RPCContext resultContext = waitData.WaitResult;
                waitData.Dispose();

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
            RPCContext context = new RPCContext();
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    context.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, (int)byteBlock.Length);
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
                RPCContext resultContext = waitData.WaitResult;
                waitData.Dispose();

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
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMException($"服务名为{method}的服务未找到注册信息");
            }
            RPCContext context = new RPCContext();
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    context.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, (int)byteBlock.Length);
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
                RPCContext resultContext = waitData.WaitResult;
                waitData.Dispose();

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
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, params object[] parameters)
        {
            if (!this.methodStore.TryGetMethodItem(method, out MethodItem methodItem))
            {
                throw new RRQMException($"服务名为{method}的服务未找到注册信息");
            }
            RPCContext context = new RPCContext();
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodItem.MethodToken;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    context.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(101, byteBlock.Buffer, 0, (int)byteBlock.Length);
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
                RPCContext resultContext = waitData.WaitResult;
                waitData.Dispose();

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
                try
                {
                    return (T)this.SerializeConverter.DeserializeParameter(resultContext.ReturnParameterBytes, typeof(T));
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
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodToken;
            context.ID = id;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    context.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(103, byteBlock.Buffer, 0, (int)byteBlock.Length);
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
                RPCContext resultContext = waitData.WaitResult;
                waitData.Dispose();

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
            else
            {
                waitData.Dispose();
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
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = methodToken;
            context.ID = id;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.CanFeedback;
            }
            try
            {
                if (invokeOption.Feedback)
                {
                    context.Feedback = 1;
                }
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);
                this.InternalSend(103, byteBlock.Buffer, 0, (int)byteBlock.Length);
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
                RPCContext resultContext = waitData.WaitResult;
                waitData.Dispose();

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

    }
}