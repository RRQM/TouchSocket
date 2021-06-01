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
using System.Net.Sockets;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UDP协议客户端
    /// </summary>
    public class UdpRPCClient : IRPCClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpRPCClient() : this(new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20))
        {
        }

        private EndPoint remoteService;
        private MethodStore methodStore;
        private WaitData<WaitResult> singleWaitData;
        private RRQMUdpSession udpSession;
        private RPCProxyInfo proxyFile;
        private WaitResult waitResult;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool"></param>
        public UdpRPCClient(BytePool bytePool)
        {
            this.SerializeConverter = new BinarySerializeConverter();
            this.waitResult = new WaitResult();
            this.Logger = new Log();
            this.singleWaitData = new WaitData<WaitResult>();
            this.BytePool = bytePool;
            this.udpSession = new RRQMUdpSession();
            this.udpSession.OnReceivedData += this.UdpSession_OnReceivedData;
        }

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        public event RRQMBytesEventHandler ReceivedBytesThenReturn;

        /// <summary>
        /// 收到ByteBlock时触发
        /// </summary>
        public event RRQMByteBlockEventHandler ReceivedByteBlock;

        /// <summary>
        /// 数据交互缓存池限制，Min:1k Byte，Max:10Mb Byte
        /// </summary>
        public int BufferLength { get => this.udpSession.BufferLength; set { this.udpSession.BufferLength = value; } }

        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get; private set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// 返回ID
        /// </summary>
        public string ID => null;

       

        /// <summary>
        /// 获取远程服务器RPC服务文件
        /// </summary>
        /// <param name="ipHost"></param>
        /// <param name="verifyToken"></param>
        /// <param name="proxyToken">代理令箭</param>
        /// <returns></returns>
        /// <exception cref="RRQMRPCException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public RPCProxyInfo GetProxyInfo(IPHost ipHost, string verifyToken = null, string proxyToken = null)
        {
            this.remoteService = ipHost.EndPoint;
            int count = 0;
            while (count < 3)
            {
                lock (this)
                {
                    byte[] datas;
                    if (proxyToken == null)
                    {
                        datas = new byte[0];
                    }
                    else
                    {
                        datas = Encoding.UTF8.GetBytes(proxyToken);
                    }
                    this.UDPSend(100, datas, 0, datas.Length);
                    this.singleWaitData.Wait(3);
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

        private void Agreement_110(byte[] buffer, int r)
        {
            WaitBytes waitBytes = SerializeConvert.RRQMBinaryDeserialize<WaitBytes>(buffer, 4);
            BytesEventArgs args = new BytesEventArgs();
            args.ReceivedDataBytes = waitBytes.Bytes;
            this.ReceivedBytesThenReturn?.Invoke(this, args);
            waitBytes.Bytes = args.ReturnDataBytes;
            byte[] data = SerializeConvert.RRQMBinarySerialize(waitBytes, true);
            UDPSend(110, data, 0, data.Length);
        }

        private void UdpSession_OnReceivedData(EndPoint remoteEndpoint, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
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

        private void UDPSend(int agreement, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length + 4);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(agreement));
                byteBlock.Write(buffer, offset, length);
                this.udpSession.SendTo(byteBlock.Buffer, 0, (int)byteBlock.Length, remoteService);
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

        private void UDPSend(int agreement, ByteBlock block)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(block.Length + 4);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(agreement));
                byteBlock.Write(block.Buffer, 0, (int)block.Length);
                this.udpSession.SendTo(byteBlock.Buffer, 0, (int)byteBlock.Length, remoteService);
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

        private void UDPSend(int agreement)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(agreement));
                this.udpSession.SendTo(byteBlock.Buffer, 0, (int)byteBlock.Length, remoteService);
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
            lock (this)
            {
                this.singleWaitData.WaitResult = null;
                RpcContext context = new RpcContext();
                MethodItem methodItem = this.methodStore.GetMethodItem(method);
                context.MethodToken = methodItem.MethodToken;
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                if (invokeOption == null)
                {
                    invokeOption = InvokeOption.NoFeedback;
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

                    UDPSend(101, byteBlock);
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
                    this.singleWaitData.Wait(invokeOption.WaitTime * 1000);
                    if (this.singleWaitData.WaitResult == null)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                }
                return default(T);
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
            lock (this)
            {
                this.singleWaitData.WaitResult = null;
                RpcContext context = new RpcContext();
                MethodItem methodItem = this.methodStore.GetMethodItem(method);
                context.MethodToken = methodItem.MethodToken;
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                if (invokeOption == null)
                {
                    invokeOption = InvokeOption.NoFeedback;
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

                    UDPSend(101, byteBlock);
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
                    this.singleWaitData.Wait(invokeOption.WaitTime * 1000);
                    if (this.singleWaitData.WaitResult == null)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                }
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
        /// <returns>服务器返回结果,此处永久返回null</returns>
        public object Invoke(string method, InvokeOption invokeOption, ref object[] parameters)
        {
            lock (this)
            {
                this.singleWaitData.WaitResult = null;
                RpcContext context = new RpcContext();
                MethodItem methodItem = this.methodStore.GetMethodItem(method);
                context.MethodToken = methodItem.MethodToken;
                ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                if (invokeOption == null)
                {
                    invokeOption = InvokeOption.NoFeedback;
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

                    UDPSend(101, byteBlock);
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
                    this.singleWaitData.Wait(invokeOption.WaitTime * 1000);
                    if (this.singleWaitData.WaitResult == null)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 释放所占资源
        /// </summary>
        public void Dispose()
        {
            this.udpSession.Dispose();
        }
    }
}