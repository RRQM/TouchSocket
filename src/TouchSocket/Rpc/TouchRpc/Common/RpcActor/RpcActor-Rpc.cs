//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Log;
using TouchSocket.Core.Run;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    public partial class RpcActor
    {
        private readonly ConcurrentDictionary<long, TouchRpcCallContext> m_contextDic;
        private SerializationSelector m_serializationSelector;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcStore RpcStore { get; set; }

        /// <summary>
        /// Rpc主呼方。
        /// </summary>
        public object Caller { get; set; }

        /// <summary>
        /// 序列化选择器
        /// </summary>
        public SerializationSelector SerializationSelector { get => this.m_serializationSelector; set => this.m_serializationSelector = value; }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.isByRef)
                                        {
                                            try
                                            {
                                                for (int i = 0; i < parameters.Length; i++)
                                                {
                                                    parameters[i] = this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                                }
                                            }
                                            catch (System.Exception e)
                                            {
                                                throw new Exception(e.Message);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        return (T)this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    default:
                        return default;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.isByRef)
                                        {
                                            for (int i = 0; i < parameters.Length; i++)
                                            {
                                                parameters[i] = this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="Exception"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    List<byte[]> datas = new List<byte[]>();
                    foreach (object parameter in parameters)
                    {
                        datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                    }
                    context.parametersBytes = datas;
                }

                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    break;

                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcNoRegisterException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledServiceInvoke, context.Sign);
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    List<byte[]> datas = new List<byte[]>();
                    foreach (object parameter in parameters)
                    {
                        datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                    }
                    context.parametersBytes = datas;
                }

                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        return (T)this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }

                    default:
                        return default;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="targetID">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void Invoke(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (string.IsNullOrEmpty(targetID))
            {
                throw new ArgumentException($"“{nameof(targetID)}”不能为 null 或空。", nameof(targetID));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException($"“{nameof(method)}”不能为 null 或空。", nameof(method));
            }

            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    rpcActor.Invoke(method, invokeOption, parameters);
                    return;
                }
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription());
            }

            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            context.id = targetID;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledIDInvoke, new CanceledID() { id = targetID, sign = context.Sign });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    List<byte[]> datas = new List<byte[]>();
                    foreach (object parameter in parameters)
                    {
                        datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                    }
                    context.parametersBytes = datas;
                }

                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                        }
                        break;

                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    break;

                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="targetID">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns></returns>
        public T Invoke<T>(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (string.IsNullOrEmpty(targetID))
            {
                throw new ArgumentException($"“{nameof(targetID)}”不能为 null 或空。", nameof(targetID));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException($"“{nameof(method)}”不能为 null 或空。", nameof(method));
            }

            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    return rpcActor.Invoke<T>(method, invokeOption, parameters);
                }
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription());
            }

            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            context.id = targetID;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledIDInvoke, new CanceledID() { id = targetID, sign = context.Sign });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                if (parameters != null)
                {
                    List<byte[]> datas = new List<byte[]>();
                    foreach (object parameter in parameters)
                    {
                        datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                    }
                    context.parametersBytes = datas;
                }

                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        return (T)this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }

                    default:
                        return default;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <param name="types"></param>
        public void Invoke(string targetID, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (string.IsNullOrEmpty(targetID))
            {
                throw new ArgumentException($"“{nameof(targetID)}”不能为 null 或空。", nameof(targetID));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException($"“{nameof(method)}”不能为 null 或空。", nameof(method));
            }

            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    rpcActor.Invoke(method, invokeOption, ref parameters, types);
                    return;
                }
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription());
            }

            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            context.id = targetID;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledIDInvoke, new CanceledID() { id = targetID, sign = context.Sign });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.isByRef)
                                        {
                                            for (int i = 0; i < parameters.Length; i++)
                                            {
                                                parameters[i] = this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        break;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return;
                        }

                    default:
                        return;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T Invoke<T>(string targetID, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            if (string.IsNullOrEmpty(targetID))
            {
                throw new ArgumentException($"“{nameof(targetID)}”不能为 null 或空。", nameof(targetID));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException($"“{nameof(method)}”不能为 null 或空。", nameof(method));
            }

            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    return rpcActor.Invoke<T>(method, invokeOption, ref parameters, types);
                }
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription());
            }

            TouchRpcPackage context = new TouchRpcPackage();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(context);
            context.methodName = method;
            context.id = targetID;
            ByteBlock byteBlock = new ByteBlock();
            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            if (invokeOption.Token.CanBeCanceled)
            {
                waitData.SetCancellationToken(invokeOption.Token);
                invokeOption.Token.Register(this.CanceledIDInvoke, new CanceledID() { id = targetID, sign = context.Sign });
            }

            try
            {
                context.LoadInvokeOption(invokeOption);
                List<byte[]> datas = new List<byte[]>();
                foreach (object parameter in parameters)
                {
                    datas.Add(this.m_serializationSelector.SerializeParameter(context.SerializationType, parameter));
                }
                context.parametersBytes = datas;
                context.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_201_Invoke2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        TouchRpcPackage resultContext = (TouchRpcPackage)waitData.WaitResult;
                                        resultContext.ThrowStatus();
                                        if (resultContext.isByRef)
                                        {
                                            for (int i = 0; i < parameters.Length; i++)
                                            {
                                                parameters[i] = this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ParametersBytes[i], types[i]);
                                            }
                                        }
                                        else
                                        {
                                            parameters = null;
                                        }
                                        return (T)this.m_serializationSelector.DeserializeParameter(resultContext.SerializationType, resultContext.ReturnParameterBytes, typeof(T));
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }

                    default:
                        return default;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public Task InvokeAsync(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(id, method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        public Task<T> InvokeAsync<T>(string id, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(id, method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(method, invokeOption, parameters);
            });
        }

        private void CanceledIDInvoke(object obj)
        {
            if (obj is CanceledID canceled)
            {
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    this.SocketSend(113, byteBlock.Write(canceled.id).Write(canceled.sign));
                }
            }
        }

        private void CanceledServiceInvoke(object obj)
        {
            this.SocketSend(TouchRpcUtility.P_204_CancelInvoke, TouchSocketBitConverter.Default.GetBytes((long)obj));
        }

        private TouchRpcPackage Invoke(TouchRpcPackage sourceContext, IInvokeOption invokeOption, bool autoSign)
        {
            TouchRpcPackage thisContext = new TouchRpcPackage();
            if (!autoSign)
            {
                thisContext.Sign = sourceContext.Sign;
            }
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(thisContext, autoSign);
            thisContext.methodName = sourceContext.methodName;
            ByteBlock byteBlock = new ByteBlock();

            try
            {
                thisContext.LoadInvokeOption(invokeOption);
                thisContext.parametersBytes = sourceContext.parametersBytes;
                thisContext.Serialize(byteBlock);

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SocketSendAsync(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.SocketSend(TouchRpcUtility.P_200_Invoke_Request, byteBlock.Buffer, 0, byteBlock.Len);
                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        return (TouchRpcPackage)waitData.WaitResult;
                                    }
                                case WaitDataStatus.Overtime:
                                    {
                                        throw new TimeoutException("等待结果超时");
                                    }
                            }
                            return default;
                        }

                    default:
                        return default;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private readonly WaitCallback m_waitCallback_InvokeClientByID;

        private void InvokeClientByID(object o)
        {
            TouchRpcPackage context = (TouchRpcPackage)o;
            using (ByteBlock retuenByteBlock = new ByteBlock())
            {
                if (this.OnFindRpcActor?.Invoke(context.id) is RpcActor rpcActor)
                {
                    InvokeOption invokeOption = new InvokeOption();
                    invokeOption.FeedbackType = (FeedbackType)context.Feedback;
                    invokeOption.SerializationType = context.SerializationType;
                    invokeOption.Timeout = context.Timeout;

                    try
                    {
                        var resultContext = rpcActor.Invoke(context, invokeOption, false);
                        context.Status = 1;
                        if (resultContext == null)
                        {
                            context.parametersBytes = null;
                        }
                        else
                        {
                            context.returnParameterBytes = resultContext.returnParameterBytes;
                            if (resultContext.isByRef)
                            {
                                context.isByRef = true;
                                context.parametersBytes = resultContext.parametersBytes;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        context.parametersBytes = null;
                        context.Status = 6;
                        context.Message = ex.Message;
                    }
                    context.Serialize(retuenByteBlock);
                    this.SocketSend(TouchRpcUtility.P_1201_Invoke2C_Response, retuenByteBlock.Buffer, 0, retuenByteBlock.Len);
                }
                else
                {
                    context.parametersBytes = null;
                    context.Status = 7;
                    context.Serialize(retuenByteBlock);
                    this.SocketSend(TouchRpcUtility.P_1201_Invoke2C_Response, retuenByteBlock.Buffer, 0, retuenByteBlock.Len);
                }
            }
        }

        private readonly WaitCallback m_waitCallback_InvokeThis;

        private void InvokeThis(object o)
        {
            TouchRpcPackage rpcPackage = (TouchRpcPackage)o;
            List<byte[]> psData = rpcPackage.parametersBytes;
            if (rpcPackage.Feedback == 1)
            {
                ByteBlock returnByteBlock = new ByteBlock();
                try
                {
                    rpcPackage.parametersBytes = null;
                    rpcPackage.Status = 1;
                    rpcPackage.Serialize(returnByteBlock);
                    this.SocketSend(TouchRpcUtility.P_1200_Invoke_Response, returnByteBlock.Buffer, 0, returnByteBlock.Len);
                }
                catch
                {
                }
                finally
                {
                    returnByteBlock.Dispose();
                }
            }

            InvokeResult invokeResult = new InvokeResult();
            object[] ps = null;
            MethodInstance methodInstance = this.GetInvokeMethod?.Invoke(rpcPackage.methodName);
            TouchRpcCallContext callContext = null;
            if (methodInstance != null)
            {
                try
                {
                    if (methodInstance.IsEnable)
                    {
                        callContext = new TouchRpcCallContext()
                        {
                            Caller = this.Caller,
                            MethodInstance = methodInstance,
                            TouchRpcPackage = rpcPackage
                        };
                        this.m_contextDic.TryAdd(rpcPackage.Sign, callContext);

                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            ps = new object[methodInstance.ParameterTypes.Length];
                            ps[0] = callContext;
                            for (int i = 0; i < psData.Count; i++)
                            {
                                ps[i + 1] = this.m_serializationSelector.DeserializeParameter(rpcPackage.SerializationType, psData[i], methodInstance.ParameterTypes[i + 1]);
                            }
                        }
                        else
                        {
                            ps = new object[methodInstance.ParameterTypes.Length];
                            for (int i = 0; i < methodInstance.ParameterTypes.Length; i++)
                            {
                                ps[i] = this.m_serializationSelector.DeserializeParameter(rpcPackage.SerializationType, psData[i], methodInstance.ParameterTypes[i]);
                            }
                        }
                    }
                    else
                    {
                        invokeResult.Status = InvokeStatus.UnEnable;
                    }
                }
                catch (Exception ex)
                {
                    invokeResult.Status = InvokeStatus.Exception;
                    invokeResult.Message = ex.Message;
                }
            }
            else
            {
                invokeResult.Status = InvokeStatus.UnFound;
            }

            if (invokeResult.Status == InvokeStatus.Ready)
            {
                IRpcServer rpcServer = methodInstance.ServerFactory.Create(callContext, ps);
                if (rpcServer is ITransientRpcServer transientRpcServer)
                {
                    transientRpcServer.CallContext = callContext;
                }
                invokeResult = this.RpcStore.Execute(rpcServer, ps, callContext);
            }

            if (rpcPackage.Feedback != 2)
            {
                return;
            }

            switch (invokeResult.Status)
            {
                case InvokeStatus.UnFound:
                    {
                        rpcPackage.Status = 2;
                        break;
                    }
                case InvokeStatus.Success:
                    {
                        if (methodInstance.HasReturn)
                        {
                            rpcPackage.returnParameterBytes = this.m_serializationSelector.SerializeParameter(rpcPackage.SerializationType, invokeResult.Result);
                        }
                        else
                        {
                            rpcPackage.returnParameterBytes = null;
                        }

                        if (methodInstance.IsByRef)
                        {
                            rpcPackage.isByRef = true;
                            rpcPackage.parametersBytes = new List<byte[]>();

                            int i = 0;
                            if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                            {
                                i = 1;
                            }
                            for (; i < ps.Length; i++)
                            {
                                rpcPackage.parametersBytes.Add(this.m_serializationSelector.SerializeParameter(rpcPackage.SerializationType, ps[i]));
                            }
                        }
                        else
                        {
                            rpcPackage.parametersBytes = null;
                        }

                        rpcPackage.Status = 1;
                        break;
                    }
                case InvokeStatus.UnEnable:
                    {
                        rpcPackage.Status = 3;
                        break;
                    }
                case InvokeStatus.InvocationException:
                    {
                        rpcPackage.Status = 5;
                        rpcPackage.Message = invokeResult.Message;
                        break;
                    }
                case InvokeStatus.Exception:
                    {
                        rpcPackage.Status = 6;
                        rpcPackage.Message = invokeResult.Message;
                        break;
                    }
                default:
                    return;
            }

            this.m_contextDic.TryRemove(rpcPackage.Sign, out _);
            ByteBlock byteBlock = new ByteBlock();
            try
            {
                rpcPackage.Serialize(byteBlock);
                this.SocketSend(TouchRpcUtility.P_1200_Invoke_Response, byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger?.Exception(ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}