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
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC客户端连接器
    /// </summary>
    public sealed class RpcJunctor : TokenTcpClient, IPoolObject
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool"></param>
        public RpcJunctor(BytePool bytePool) : base(bytePool)
        {
            this.SerializeConverter = new BinarySerializeConverter();
            this.methodStore = new MethodStore();
            this.singleWaitData = new WaitData<WaitResult>();
            this.singleWaitData.WaitResult = new WaitResult();
            this.invokeWaitData = new WaitData<RPCContext>();
            this.invokeWaitData.WaitResult = new RPCContext();
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
        }

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        internal RRQMBytesEventHandler ReceivedBytesThenReturn;

        /// <summary>
        /// 收到ByteBlock时触发
        /// </summary>
        internal RRQMByteBlockEventHandler ReceivedByteBlock;

        /// <summary>
        /// 序列化生成器
        /// </summary>
        internal SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 是否新创建
        /// </summary>
        public bool NewCreat { get; set; }

        private WaitData<WaitResult> singleWaitData;
        private WaitData<RPCContext> invokeWaitData;
        internal MethodStore methodStore;
        private RPCProxyInfo proxyFile;
        private RRQMAgreementHelper agreementHelper;
      

        /// <summary>
        /// 获取函数注册
        /// </summary>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMRPCException"></exception>
        internal MethodStore GetMethodStore()
        {
            this.agreementHelper = new RRQMAgreementHelper(this);
            lock (locker)
            {
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
            lock (locker)
            {
                agreementHelper.SocketSend(100, proxyToken);
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
        }

        /// <summary>
        /// 链接
        /// </summary>
        /// <param name="addressFamily"></param>
        /// <param name="endPoint"></param>
        public override void Connect(AddressFamily addressFamily, EndPoint endPoint)
        {
            base.Connect(addressFamily, endPoint);
            this.agreementHelper = new RRQMAgreementHelper(this);
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
            lock (locker)
            {
                MethodItem methodItem = methodStore.GetMethodItem(method);

                invokeWaitData.WaitResult.Method = methodItem.Method;

                ByteBlock byteBlock = this.BytePool.GetByteBlock(1024);
                if (invokeOption == null)
                {
                    invokeOption = InvokeOption.CanFeedback;
                }
                try
                {
                    if (invokeOption.Feedback)
                    {
                        invokeWaitData.WaitResult.Feedback = 1;
                    }
                    List<byte[]> datas = new List<byte[]>();
                    foreach (object parameter in parameters)
                    {
                        datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                    }
                    invokeWaitData.WaitResult.ParametersBytes = datas;
                    invokeWaitData.WaitResult.Serialize(byteBlock);

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
                    invokeWaitData.Wait(invokeOption.WaitTime * 1000);

                    RPCContext context = invokeWaitData.WaitResult;
                    invokeWaitData.Dispose();
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
                        throw new RRQMRPCException($"服务器已阻止本次行为，信息：{context.Method}");
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
                    return default(T);
                }
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
            lock (locker)
            {


                MethodItem methodItem = this.methodStore.GetMethodItem(method);
                invokeWaitData.WaitResult.Method = methodItem.Method;
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                if (invokeOption == null)
                {
                    invokeOption = InvokeOption.CanFeedback;
                }
                try
                {
                    if (invokeOption.Feedback)
                    {
                        invokeWaitData.WaitResult.Feedback = 1;
                    }
                    List<byte[]> datas = new List<byte[]>();
                    foreach (object parameter in parameters)
                    {
                        datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                    }
                    invokeWaitData.WaitResult.ParametersBytes = datas;
                    invokeWaitData.WaitResult.Serialize(byteBlock);
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
                    invokeWaitData.Wait(invokeOption.WaitTime * 1000);
                    RPCContext context = invokeWaitData.WaitResult;
                    invokeWaitData.Dispose();

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
                        throw new RRQMRPCException($"服务器已阻止本次行为，信息：{context.Method}");
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
                }
                else
                {
                    invokeWaitData.Dispose();
                }
            }
        }

        private void Agreement_110(byte[] buffer, int r)
        {
            WaitBytes waitBytes = SerializeConvert.BinaryDeserialize<WaitBytes>(buffer, 4, r - 4);
            BytesEventArgs args = new BytesEventArgs();
            args.ReceivedDataBytes = waitBytes.Bytes;
            this.ReceivedBytesThenReturn?.Invoke(this, args);
            waitBytes.Bytes = args.ReturnDataBytes;

            agreementHelper.SocketSend(110, SerializeConvert.RRQMBinarySerialize(waitBytes,true));
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
                            proxyFile = SerializeConvert.BinaryDeserialize<RPCProxyInfo>(buffer, 4, r - 4);
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
                            RPCContext result = RPCContext.Deserialize(buffer, 4);
                            this.invokeWaitData.Set(result);
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
                            MethodItem[] methodItems = SerializeConvert.BinaryDeserialize<MethodItem[]>(buffer, 4, r - 4);
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
                case 110:/*反向函数调用返回*/
                    {
                        try
                        {
                            Agreement_110(buffer, r);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 110, 错误详情:{e.Message}");
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

       
        /// <summary>
        /// 初创建
        /// </summary>
        public void Create()
        {

        }

        /// <summary>
        /// 重复创建
        /// </summary>
        public void Recreate()
        {

        }

        /// <summary>
        /// 注销时
        /// </summary>
        public void Destroy()
        {

        }
    }
}