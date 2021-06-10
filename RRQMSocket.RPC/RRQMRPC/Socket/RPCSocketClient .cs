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
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC服务器辅助类
    /// </summary>
    public sealed class RPCSocketClient : SocketClient
    {
        internal RRQMAgreementHelper agreementHelper;

        internal Action<RPCSocketClient, ByteBlock> Received;

        internal SerializeConverter serializeConverter;

        internal RRQMWaitHandle<RPCContext> waitHandle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RPCSocketClient()
        {
            waitHandle = new RRQMWaitHandle<RPCContext>();
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodToken"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T CallBack<T>(int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            RPCContext context = new RPCContext();
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            
            context.MethodToken = methodToken;

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
                    datas.Add(SerializeConvert.RRQMBinarySerialize(parameter, true));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);

                agreementHelper.SocketSend(112, byteBlock);
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
                    throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCCallBackMethod");
                }
                else if (resultContext.Status == 3)
                {
                    throw new RRQMRPCException("客户端未开启反向RPC");
                }
                else if (resultContext.Status == 4)
                {
                    throw new RRQMRPCException($"调用异常，信息：{resultContext.Message}");
                }

                try
                {
                    return (T)this.serializeConverter.DeserializeParameter(resultContext.ReturnParameterBytes, typeof(T));
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

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <param name="methodToken"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        public void CallBack(int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            RPCContext context = new RPCContext();
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
           
            context.MethodToken = methodToken;

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
                    datas.Add(SerializeConvert.RRQMBinarySerialize(parameter, true));
                }
                context.ParametersBytes = datas;
                context.Serialize(byteBlock);

                agreementHelper.SocketSend(112, byteBlock);
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
                    throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCCallBackMethod");
                }
                else if (resultContext.Status == 3)
                {
                    throw new RRQMRPCException("客户端未开启反向RPC");
                }
                else if (resultContext.Status == 4)
                {
                    throw new RRQMRPCException($"调用异常，信息：{resultContext.Message}");
                }
            }
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <param name="invokeContext"></param>
        /// <param name="invokeOption"></param>
        /// <returns></returns>
        public byte[] CallBack(RPCContext invokeContext, InvokeOption invokeOption = null)
        {
            RPCContext context = new RPCContext();
            WaitData<RPCContext> waitData = this.waitHandle.GetWaitData(context);
            context.MethodToken = invokeContext.MethodToken;

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
                
                context.ParametersBytes = invokeContext.ParametersBytes;
                context.Serialize(byteBlock);

                agreementHelper.SocketSend(112, byteBlock);
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
                    throw new RRQMRPCInvokeException("未找到该公共方法，或该方法未标记RRQMRPCCallBackMethod");
                }
                else if (resultContext.Status == 3)
                {
                    throw new RRQMRPCException("客户端未开启反向RPC");
                }
                else if (resultContext.Status == 4)
                {
                    throw new RRQMRPCException($"调用异常，信息：{context.Message}");
                }

                    return resultContext.ReturnParameterBytes;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 向RPC发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Send(byte[] buffer, int offset, int length)
        {
            agreementHelper.SocketSend(120, buffer, offset, length);
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            Received.Invoke(this, byteBlock);
        }
    }
}