//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCParser泛型类型
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public class TcpParser<TClient> : ProtocolService<TClient>, IRPCParser, IIDInvoke, IRRQMRpcParser where TClient : RpcSocketClient, new()
    {
        private MethodStore methodStore;
        private SerializationSelector serializationSelector;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpParser()
        {
            this.methodStore = new MethodStore();
        }

        /// <summary>
        /// 预处理流
        /// </summary>
        public event RRQMStreamOperationEventHandler<TClient> BeforeReceiveStream;

        /// <summary>
        /// 收到协议数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<TClient> Received;

        /// <summary>
        /// 收到流数据
        /// </summary>
        public event RRQMStreamStatusEventHandler<TClient> ReceivedStream;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodStore MethodStore => this.methodStore;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ProxyToken { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RPCService RPCService { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public SerializationSelector SerializationSelector
        {
            get { return serializationSelector; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"><inheritdoc/></param>
        public virtual void GetProxyInfo(GetProxyInfoArgs args)
        {
            if (args.RpcType.HasFlag(RpcType.RRQMRPC))
            {
                if (args.ProxyToken != this.ProxyToken)
                {
                    args.ErrorMessage = "在验证RRQMRPC时令箭不正确。";
                    args.IsSuccess = false;
                    return;
                }
                foreach (var item in this.RPCService.ServerProviders)
                {
                    var serverCellCode = CodeGenerator.Generator<RRQMRPCAttribute>(item.GetType());
                    args.Codes.Add(serverCellCode);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public virtual MethodItem[] GetRegisteredMethodItems(string proxyToken, ICaller caller)
        {
            if (proxyToken == this.ProxyToken)
            {
                return this.methodStore.GetAllMethodItem();
            }
            return null;
        }

        /// <summary>
        /// 选择ID然后调用反向RPC
        /// </summary>
        /// <param name="id"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        public void Invoke(string id, string method, InvokeOption invokeOption, params object[] parameters)
        {
            if (!this.TryGetSocketClient(id, out TClient client))
            {
                throw new RRQMRPCException("没有找到对应ID的客户端");
            }
            client.Invoke(method, invokeOption, parameters);
        }

        /// <summary>
        /// 选择ID然后调用反向RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Invoke<T>(string id, string method, InvokeOption invokeOption, params object[] parameters)
        {
            if (!this.TryGetSocketClient(id, out TClient client))
            {
                throw new RRQMRPCException("没有找到对应ID的客户端");
            }

            return client.Invoke<T>(method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        public void OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RpcContext context = (RpcContext)methodInvoker.Flag;

            if (context.Feedback != 2)
            {
                return;
            }

            switch (methodInvoker.Status)
            {
                case InvokeStatus.Ready:
                    {
                        break;
                    }

                case InvokeStatus.UnFound:
                    {
                        context.Status = 2;
                        break;
                    }
                case InvokeStatus.Success:
                    {
                        if (methodInstance.MethodToken > 50000000)
                        {
                            context.returnParameterBytes = this.serializationSelector.SerializeParameter(context.SerializationType, methodInvoker.ReturnParameter);
                        }
                        else
                        {
                            context.returnParameterBytes = null;
                        }

                        if (methodInstance.IsByRef)
                        {
                            context.parametersBytes = new List<byte[]>();

                            int i = 0;
                            if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                            {
                                i = 1;
                            }
                            for (; i < methodInvoker.Parameters.Length; i++)
                            {
                                context.parametersBytes.Add(this.serializationSelector.SerializeParameter(context.SerializationType, methodInvoker.Parameters[i]));
                            }
                        }
                        else
                        {
                            context.parametersBytes = null;
                        }

                        context.Status = 1;
                        break;
                    }
                case InvokeStatus.Abort:
                    {
                        context.Status = 4;
                        context.Message = methodInvoker.StatusMessage;
                        break;
                    }
                case InvokeStatus.UnEnable:
                    {
                        context.Status = 3;
                        break;
                    }
                case InvokeStatus.InvocationException:
                    {
                        context.Status = 5;
                        context.Message = methodInvoker.StatusMessage;
                        break;
                    }
                case InvokeStatus.Exception:
                    {
                        context.Status = 6;
                        context.Message = methodInvoker.StatusMessage;
                        break;
                    }
                default:
                    break;
            }

            if (methodInvoker.Caller is RpcSocketClient socketClient)
            {
                socketClient.EndInvoke(context);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        public void OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var item in methodInstances)
            {
                if (item.GetAttribute<RRQMRPCAttribute>() is RRQMRPCAttribute attribute)
                {
                    MethodItem methodItem = new MethodItem();
                    methodItem.ServerName = provider.GetType().Name;
                    methodItem.IsOutOrRef = item.IsByRef;
                    methodItem.MethodToken = item.MethodToken;

                    if (string.IsNullOrEmpty(attribute.MethodName))
                    {
                        methodItem.Method = item.Method.Name;
                    }
                    else
                    {
                        methodItem.Method = attribute.MethodName;
                    }

                    this.methodStore.AddMethodItem(methodItem);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        public void OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var item in methodInstances)
            {
                this.methodStore.RemoveMethodItem(item.MethodToken);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="executeMethod"></param>
        public void SetExecuteMethod(Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.RRQMExecuteMethod = executeMethod;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="methodMap"></param>
        public void SetMethodMap(MethodMap methodMap)
        {
            this.MethodMap = methodMap;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="service"></param>
        public void SetRPCService(RPCService service)
        {
            this.RPCService = service;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="ServiceConfig"></param>
        protected override void LoadConfig(ServiceConfig ServiceConfig)
        {
            this.serializationSelector = ServiceConfig.GetValue<SerializationSelector>(TcpRpcParserConfig.SerializationSelectorProperty);
            this.ProxyToken = ServiceConfig.GetValue<string>(TcpRpcParserConfig.ProxyTokenProperty);
            base.LoadConfig(ServiceConfig);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.IDAction = this.IDInvoke;
            socketClient.Received += this.OnReceived;
            socketClient.BeforeReceiveStream += this.OnBeforeReceiveStream;
            socketClient.ReceivedStream += this.OnReceivedStream;
            socketClient.methodMap = this.MethodMap;
            socketClient.executeMethod = this.Execute;
            socketClient.serializationSelector = this.serializationSelector;
            base.OnConnecting(socketClient, e);
        }

        private void Execute(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
        }

        private void IDInvoke(RpcContext rpcContext)
        {
            if (this.TryGetSocketClient(rpcContext.id, out TClient client))
            {
                var context = client.Invoke(rpcContext, 10 * 1000);
                if (context != null)
                {
                    rpcContext.Status = context.Status;
                    rpcContext.returnParameterBytes = context.returnParameterBytes;
                }
            }
            else
            {
            }
        }

        private void OnBeforeReceiveStream(RpcSocketClient client, StreamOperationEventArgs e)
        {
            this.BeforeReceiveStream?.Invoke((TClient)client, e);
        }

        private void OnReceived(RpcSocketClient client, short procotol, ByteBlock byteBlock)
        {
            this.Received?.Invoke((TClient)client, procotol, byteBlock);
        }

        private void OnReceivedStream(RpcSocketClient client, StreamStatusEventArgs e)
        {
            this.ReceivedStream?.Invoke((TClient)client, e);
        }
    }
}