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
using System.Net;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UDP协议客户端
    /// </summary>
    public class UdpRPCClient : UdpSession, IRRQMRPCClient
    {
        private MethodStore methodStore;
        private RPCProxyInfo proxyFile;
        private WaitData<WaitResult> singleWaitData;
        private RRQMWaitHandle<RPCContext> waitHandle;
        private WaitResult waitResult;

        /// <summary>
        /// 发现服务后
        /// </summary>
        public event RRQMMessageEventHandler ServiceDiscovered;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpRPCClient()
        {
            this.waitHandle = new RRQMWaitHandle<RPCContext>();
            this.SerializeConverter = new BinarySerializeConverter();
            this.waitResult = new WaitResult();
            this.singleWaitData = new WaitData<WaitResult>();
        }

        /// <summary>
        /// 返回ID
        /// </summary>
        public string ID => null;

        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

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
                    string proxyToken = (string)this.ServiceConfig.GetValue(UdpRPCClientConfig.ProxyTokenProperty);
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

        /// <summary>
        /// 发现服务
        /// </summary>
        /// <param name="isTrigger">是否触发初始化事件</param>
        /// <returns>已发现的服务</returns>
        public MethodItem[] DiscoveryService(bool isTrigger = true)
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
                    try
                    {
                        this.methodStore = null;

                        string proxyToken = (string)this.ServiceConfig.GetValue(UdpRPCClientConfig.ProxyTokenProperty);
                        byte[] data = new byte[0];
                        if (!string.IsNullOrEmpty(proxyToken))
                        {
                            data = Encoding.UTF8.GetBytes(proxyToken);
                        }

                        this.UDPSend(102, data, 0, data.Length);
                        this.singleWaitData.Wait(1000 * 5);
                        if (this.methodStore != null)
                        {
                            if (isTrigger)
                            {
                                this.OnServiceDiscovered(new MesEventArgs("success"));
                            }
                            return this.methodStore.GetAllMethodItem().ToArray();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new RRQMRPCException(e.Message);
                    }
                }
                count++;
            }
            throw new RRQMTimeoutException("初始化超时");
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
                this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
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
                        RPCContext resultContext = waitData.WaitResult;
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
                        RPCContext resultContext = waitData.WaitResult;
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
                this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
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
                        RPCContext resultContext = waitData.WaitResult;
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
                        RPCContext resultContext = waitData.WaitResult;
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
                            catch (Exception e)
                            {
                                throw new RRQMException(e.Message);
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
                this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
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
                        RPCContext resultContext = waitData.WaitResult;
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
                        RPCContext resultContext = waitData.WaitResult;
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
                this.UDPSend(101, byteBlock.Buffer, 0, byteBlock.Len);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
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
                        RPCContext resultContext = waitData.WaitResult;
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
                        RPCContext resultContext = waitData.WaitResult;
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
                        catch (Exception e)
                        {
                            throw new RRQMException(e.Message);
                        }
                    }
                default:
                    return default;
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
                            RPCContext result = RPCContext.Deserialize(buffer, 2);
                            this.waitHandle.SetRun(result.Sign, result);
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
                            List<MethodItem> methodItems = SerializeConvert.RRQMBinaryDeserialize<List<MethodItem>>(buffer, 2);
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
            }
        }

        private void UDPSend(short procotol, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length + 2);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(procotol));
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

        private void UDPSend(short procotol)
        {
            this.UDPSend(procotol, new byte[0], 0, 0);
        }

        /// <summary>
        /// RPC完成初始化后
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnServiceDiscovered(MesEventArgs args)
        {
            this.ServiceDiscovered?.Invoke(this, args);
        }
    }
}