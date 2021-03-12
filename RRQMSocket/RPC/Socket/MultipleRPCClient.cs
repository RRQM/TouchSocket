//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
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
using RRQMCore.Run;
using RRQMCore.Serialization;
using RRQMSocket.Pool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 集群RPC客户端
    /// </summary>
    public sealed class MultipleRPCClient : IRPCClient, ISerialize
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MultipleRPCClient(int capacity)
        {
            this.locker = new object();
            this.BytePool = new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20);
            BinarySerializeConverter serializeConverter = new BinarySerializeConverter();
            this.SerializeConverter = serializeConverter;
            this.methodStore = new MethodStore();
            this.singleWaitData = new WaitData<WaitResult>();
            this.singleWaitData.WaitResult = new WaitResult();
            this.ConnectionPool = TcpConnectionPool<RRQMTokenTcpClient>.CreatConnectionPool(capacity, this.BytePool, this.ConnectionPool_OnClientIni, this.BytePool);
            this.Logger = new Log();
        }

        private void ConnectionPool_OnClientIni(RRQMTokenTcpClient tcpClient)
        {
            tcpClient.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
            tcpClient.OnReceivedData += this.TcpClient_OnReceivedData;
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
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 获取连接池实例
        /// </summary>
        public TcpConnectionPool<RRQMTokenTcpClient> ConnectionPool { get; private set; }

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
                if (this.ConnectionPool != null)
                {
                    this.ConnectionPool.Logger = value;
                }
            }
        }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool { get; private set; }

        /// <summary>
        /// 获取即将在下一次通信的客户端单体
        /// </summary>
        public RRQMTokenTcpClient NextClient { get { return this.ConnectionPool == null ? null : this.ConnectionPool.GetNextClient(); } }

        /// <summary>
        /// 获取即将在下一次通信的客户端单体的IDToken
        /// </summary>
        public string IDToken { get { return this.ConnectionPool == null ? null : this.ConnectionPool.GetNextClient().IDToken; } }

        private WaitData<WaitResult> singleWaitData;
        private RRQMWaitHandle<RPCContext> waitHandles = new RRQMWaitHandle<RPCContext>();
        private MethodStore methodStore;
        private RPCProxyInfo proxyFile;
        private object locker;

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="endPoint"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMRPCException"></exception>
        public void Connect(EndPoint endPoint)
        {
            lock (locker)
            {
                this.ConnectionPool.Connect(endPoint);
                try
                {
                    this.methodStore = null;
                    SendAgreement(102);
                    this.singleWaitData.Wait(1000 * 10);
                }
                catch (Exception e)
                {
                    throw new RRQMRPCException(e.Message);
                }
                if (this.methodStore == null)
                {
                    throw new RRQMTimeoutException("连接初始化超时");
                }

            }
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="setting"></param>
        public void Connect(ConnectSetting setting)
        {
            IPAddress IP = IPAddress.Parse(setting.TargetIP);
            EndPoint endPoint = new IPEndPoint(IP, setting.TargetPort);
            this.Connect(endPoint);
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
                SendAgreement(100, proxyToken);
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
        /// 初始化RPC
        /// </summary>
        public void InitializedRPC()
        {
            if (this.methodStore != null)
            {
                this.methodStore.InitializedType();
            }
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="waitTime">等待时间（秒）</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns>服务器返回结果</returns>
        public T RPCInvoke<T>(string method, ref object[] parameters, int waitTime = 3)
        {
            WaitData<RPCContext> waitData = this.waitHandles.GetWaitData();
            waitData.WaitResult = new RPCContext();
            MethodItem methodItem = methodStore.GetMethodItem(method);

            waitData.WaitResult.Method = methodItem.Method;

            ByteBlock byteBlock = this.BytePool.GetByteBlock(1024);

            try
            {
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                waitData.WaitResult.ParametersBytes = datas;
                SerializeConvert.RRQMBinarySerialize(byteBlock, waitData.WaitResult);

                SendAgreement(101, byteBlock);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }

            waitData.Wait(waitTime * 1000);

            RPCContext context = waitData.WaitResult;
            waitData.Dispose();
            if (context.Status == 0)
            {
                throw new RRQMTimeoutException("等待结果超时");
            }
            else if (context.Status == 2)
            {
                throw new RRQMRPCInvokeException(context.Message);
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

            try
            {
                return (T)this.SerializeConverter.DeserializeParameter(context.ReturnParameterBytes, methodItem.ReturnType);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="waitTime">等待时间</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMSerializationException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void RPCInvoke(string method, ref object[] parameters, int waitTime = 3)
        {
            WaitData<RPCContext> waitData = this.waitHandles.GetWaitData();
            waitData.WaitResult = new RPCContext();
            MethodItem methodItem = this.methodStore.GetMethodItem(method);
            waitData.WaitResult.Method = methodItem.Method;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(1024);

            try
            {
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                }
                waitData.WaitResult.ParametersBytes = datas;
                SerializeConvert.RRQMBinarySerialize(byteBlock, waitData.WaitResult);

                SendAgreement(101, byteBlock);
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
            waitData.Wait(waitTime * 1000);
            RPCContext context = waitData.WaitResult;
            waitData.Dispose();

            if (context.Status == 0)
            {
                throw new RRQMTimeoutException("等待结果超时");
            }
            else if (context.Status == 2)
            {
                throw new RRQMRPCInvokeException(context.Message);
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


        }


        private void Agreement_110(object sender,byte[] buffer, int r)
        {
            WaitBytes waitBytes = SerializeConvert.BinaryDeserialize<WaitBytes>(buffer, 4, r - 4);
            BytesEventArgs args = new BytesEventArgs();
            args.ReceivedDataBytes = waitBytes.Bytes;
            this.ReceivedBytesThenReturn?.Invoke(sender, args);
            waitBytes.Bytes = args.ReturnDataBytes;

            SendAgreement(110, SerializeConvert.BinarySerialize(waitBytes));
        }

        private void TcpClient_OnReceivedData(object sender, ByteBlock e)
        {
            byte[] buffer = e.Buffer;
            int r = (int)e.Length;
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
                            RPCContext result = SerializeConvert.RRQMBinaryDeserialize<RPCContext>(buffer, 4);

                            this.waitHandles.SetRun(result.Sign, result);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{ex.Message}");
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
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{ex.Message}");
                        }
                        break;
                    }
                case 110:/*反向函数调用返回*/
                    {
                        try
                        {
                            Agreement_110(sender,buffer, r);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 110, 错误详情:{ex.Message}");
                        }
                        break;
                    }
                case 111:/*收到服务器数据*/
                    {
                        ByteBlock block = this.BytePool.GetByteBlock(r - 4);
                        try
                        {
                            block.Write(e.Buffer, 4, r - 4);
                            ReceivedByteBlock?.Invoke(this, block);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 111, 错误详情:{ex.Message}");
                        }
                        finally
                        {
                            block.Dispose();
                        }
                        break;
                    }

            }
        }

        private Socket GetSocket()
        {
            return this.ConnectionPool.GetNextClient().MainSocket;
        }
        private void SendAgreement(int agreement, byte[] dataBuffer)
        {
            byte[] data = dataBuffer;
            int dataLen = data.Length + 8;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(dataLen);
            byte[] lenBytes = BitConverter.GetBytes(dataLen);
            byte[] agreementBytes = BitConverter.GetBytes(agreement);

            byteBlock.Write(lenBytes);
            byteBlock.Write(agreementBytes);

            byteBlock.Write(data, 0, data.Length);
            try
            {
                this.ConnectionPool.Send(byteBlock.Buffer, 0, (int)byteBlock.Position);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
        private void SendAgreement(int agreement, ByteBlock dataByteBlock)
        {
            int dataLen = (int)dataByteBlock.Length + 4;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(dataLen);
            byte[] agreementBytes = BitConverter.GetBytes(agreement);
            byteBlock.Write(agreementBytes);
            byteBlock.Write(dataByteBlock.Buffer, 0, (int)dataByteBlock.Length);
            try
            {
                this.ConnectionPool.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
        private void SendAgreement(int agreement, string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            int dataLen = data.Length + 8;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(dataLen);
            byte[] lenBytes = BitConverter.GetBytes(dataLen);
            byte[] agreementBytes = BitConverter.GetBytes(agreement);

            byteBlock.Write(lenBytes);
            byteBlock.Write(agreementBytes);

            byteBlock.Write(data, 0, data.Length);
            try
            {
                this.ConnectionPool.Send(byteBlock.Buffer, 0, (int)byteBlock.Position);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
        private void SendAgreement(int agreement)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(8);
            byte[] lenBytes = BitConverter.GetBytes(8);
            byte[] agreementBytes = BitConverter.GetBytes(agreement);

            byteBlock.Write(lenBytes);
            byteBlock.Write(agreementBytes);

            try
            {
                this.GetSocket().Send(byteBlock.Buffer, 0, (int)byteBlock.Position, SocketFlags.None);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}