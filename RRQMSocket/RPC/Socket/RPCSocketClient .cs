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
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC服务器辅助类
    /// </summary>
    public sealed class RPCSocketClient : TcpSocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RPCSocketClient()
        {
            this.waitHandles = new RRQMWaitHandle<WaitBytes>();
        }

        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        internal MethodStore serverMethodStore;
        internal MethodStore clientMethodStore;

        private RRQMWaitHandle<WaitBytes> waitHandles;
        internal RRQMAgreementHelper agreementHelper;

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="buffer"></param>
        private void Agreement_101(byte[] buffer)
        {
            RPCContext content = RPCContext.Deserialize(buffer, 4);
            InstanceMethod instanceMethod = this.serverMethodStore.GetInstanceMethod(content.Method);
            if (instanceMethod.async)
            {
                Task.Factory.StartNew(() =>
                {
                    ExecuteMethod(content, instanceMethod);
                });
            }
            else
            {
                ExecuteMethod(content, instanceMethod);
            }
        }

        private void ExecuteMethod(RPCContext content, InstanceMethod instanceMethod)
        {
            if (instanceMethod != null)
            {
                ServerProvider instance = instanceMethod.instance;
                try
                {
                    MethodItem methodItem = instanceMethod.methodItem;
                    object[] parameters = null;
                    if (content.ParametersBytes != null)
                    {
                        parameters = new object[content.ParametersBytes.Count];
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            parameters[i] = this.SerializeConverter.DeserializeParameter(content.ParametersBytes[i], methodItem.ParameterTypes[i]);
                        }
                    }

                    instance.RPCEnter(methodItem);
                    MethodInfo method = instanceMethod.method;
                    content.ReturnParameterBytes = this.SerializeConverter.SerializeParameter(method.Invoke(instance, parameters));
                    content.Status = 1;
                    content.Message = null;
                    if (!instanceMethod.methodItem.IsOutOrRef)
                    {
                        content.ParametersBytes = null;
                    }
                    else
                    {
                        List<byte[]> datas = new List<byte[]>();
                        foreach (object parameter in parameters)
                        {
                            datas.Add(this.SerializeConverter.SerializeParameter(parameter));
                        }
                        content.ParametersBytes = datas;
                    }
                    instance.RPCLeave(instanceMethod.methodItem);
                }
                catch (TargetInvocationException e)
                {
                    content.Status = 2;
                    if (e.InnerException != null)
                    {
                        content.Message = "函数内部发生异常，信息：" + e.InnerException.Message;
                    }
                    else
                    {
                        content.Message = "函数内部发生异常，信息：未知";
                    }
                    instance.RPCError(instanceMethod.methodItem);
                }
                catch (Exception e)
                {
                    content.Status = 2;
                    content.Message = e.Message;
                    instance.RPCError(instanceMethod.methodItem);
                }
            }
            else
            {
                content.Message = "未找到该公共方法，或该方法未标记RRQMRPCMethod";
                content.Status = 2;
            }
            ByteBlock byteBlock = this.BytePool.GetByteBlock(1024);
            try
            {
                content.Serialize(byteBlock);
                agreementHelper.SocketSend(101, byteBlock);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, ex.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Agreement_102()
        {
            agreementHelper.SocketSend(102, SerializeConvert.BinarySerialize(this.clientMethodStore.GetAllMethodItem()));
        }

        /// <summary>
        /// 等待字节返回
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="r"></param>
        private void Agreement_110(byte[] buffer, int r)
        {
            WaitBytes waitBytes = SerializeConvert.BinaryDeserialize<WaitBytes>(buffer, 4, r - 4);
            this.waitHandles.SetRun(waitBytes.Sign, waitBytes);
        }

        /// <summary>
        /// 发送字节并返回
        /// </summary>
        /// <param name="data"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public byte[] SendBytesWaitReturn(byte[] data, int waitTime = 3)
        {
            WaitData<WaitBytes> waitData = this.waitHandles.GetWaitData();
            waitData.WaitResult.Bytes = data;
            try
            {
                byte[] buffer = SerializeConvert.BinarySerialize(waitData.WaitResult);
                agreementHelper.SocketSend(110, buffer);
            }
            catch (Exception e)
            {
                throw new RRQMRPCException(e.Message);
            }

            waitData.Wait(waitTime * 1000);

            waitData.Wait(waitTime * 1000);
            WaitBytes waitBytes = waitData.WaitResult;
            waitData.Dispose();

            if (waitBytes.Status == 0)
            {
                throw new RRQMTimeoutException("等待结果超时");
            }
            else if (waitBytes.Status == 2)
            {
                throw new RRQMRPCException(waitBytes.Message);
            }

            return waitBytes.Bytes;
        }

        /// <summary>
        /// 向RPC发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Send(byte[] buffer, int offset, int length)
        {
            agreementHelper.SocketSend(111, buffer, offset, length);
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
            int agreement = BitConverter.ToInt32(buffer, 0);

            switch (agreement)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        try
                        {
                            string proxyToken = null;
                            if (r - 4 > 0)
                            {
                                proxyToken = Encoding.UTF8.GetString(buffer, 4, r - 4);
                            }
                            agreementHelper.SocketSend(100, SerializeConvert.BinarySerialize(this.serverMethodStore.GetProxyInfo(proxyToken)));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 100, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 101:/*函数式调用*/
                    {
                        try
                        {
                            Agreement_101(buffer);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*连接初始化*/
                    {
                        try
                        {
                            Agreement_102();
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 110:/*函数式调用返回*/
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
            }
        }
    }
}