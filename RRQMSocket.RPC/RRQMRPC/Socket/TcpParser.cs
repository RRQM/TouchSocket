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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCParser泛型类型
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public class TcpParser<TClient> : ProtocolService<TClient>, IRPCParser, IRRQMRpcParser where TClient : RpcSocketClient, new()
    {
        private MethodStore methodStore;

        private RpcProxyInfo proxyInfo;

        private SerializationSelector serializationSelector;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpParser()
        {
            this.methodStore = new MethodStore();
            this.proxyInfo = new RpcProxyInfo();
        }

        /// <summary>
        /// 收到协议数据
        /// </summary>
        public event RRQMReceivedProcotolEventHandler Received;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public CellCode[] Codes { get => this.proxyInfo == null ? null : this.proxyInfo.Codes.ToArray(); }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodStore MethodStore { get => methodStore; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string NameSpace { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcProxyInfo ProxyInfo { get => proxyInfo; }

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
        public Version RPCVersion { get; private set; }

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
        /// 回调RPC
        /// </summary>
        /// <typeparam name="T">返回值</typeparam>
        /// <param name="id">ID</param>
        /// <param name="methodToken">函数唯一标识</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public T CallBack<T>(string id, int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            if (this.SocketClients.TryGetSocketClient(id, out TClient socketClient))
            {
                return socketClient.CallBack<T>(methodToken, invokeOption, parameters);
            }
            else
            {
                throw new RRQMRPCException("未找到该客户端");
            }
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="methodToken">函数唯一标识</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        public void CallBack(string id, int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            if (this.SocketClients.TryGetSocketClient(id, out TClient socketClient))
            {
                socketClient.CallBack(methodToken, invokeOption, parameters);
            }
            else
            {
                throw new RRQMRPCException("未找到该客户端");
            }
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public virtual RpcProxyInfo GetProxyInfo(string proxyToken, ICaller caller)
        {
            RpcProxyInfo proxyInfo = new RpcProxyInfo();
            if (this.ProxyToken == proxyToken)
            {
                proxyInfo.AssemblyData = this.ProxyInfo.AssemblyData;
                proxyInfo.AssemblyName = this.ProxyInfo.AssemblyName;
                proxyInfo.Codes = this.ProxyInfo.Codes;
                proxyInfo.Version = this.ProxyInfo.Version;
                proxyInfo.Status = 1;
            }
            else
            {
                proxyInfo.Status = 2;
                proxyInfo.Message = "令箭不正确";
            }

            return proxyInfo;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public virtual List<MethodItem> GetRegisteredMethodItems(string proxyToken, ICaller caller)
        {
            if (proxyToken == this.ProxyToken)
            {
                return this.methodStore.GetAllMethodItem();
            }
            return null;
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
            RRQMRPCTools.GetRPCMethod(methodInstances, this.NameSpace, ref this.methodStore, this.RPCVersion, ref this.proxyInfo);
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

            CellCode cellCode = null;
            foreach (var item in this.proxyInfo.Codes)
            {
                if (item.Name == provider.GetType().Name)
                {
                    cellCode = item;
                    break;
                }
            }
            if (cellCode != null)
            {
                this.proxyInfo.Codes.Remove(cellCode);
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

        private void Execute(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            this.RRQMExecuteMethod.Invoke(this,methodInvoker,methodInstance);
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

#if NET45_OR_GREATER

        /// <summary>
        /// 编译代理
        /// </summary>
        /// <param name="targetDic">存放目标文件夹</param>
        public void CompilerProxy(string targetDic = "")
        {
            string assemblyInfo = CodeGenerator.GetAssemblyInfo(this.proxyInfo.AssemblyName, this.proxyInfo.Version);
            List<string> codesString = new List<string>();
            codesString.Add(assemblyInfo);
            foreach (var item in this.proxyInfo.Codes)
            {
                codesString.Add(item.Code);
            }
            RpcCompiler.CompileCode(System.IO.Path.Combine(targetDic, this.proxyInfo.AssemblyName), codesString.ToArray());
        }

#endif

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="ServiceConfig"></param>
        protected override void LoadConfig(ServiceConfig ServiceConfig)
        {
            base.LoadConfig(ServiceConfig);
            this.serializationSelector = ServiceConfig.GetValue<SerializationSelector>(TcpRpcParserConfig.SerializationSelectorProperty);
            this.ProxyToken = ServiceConfig.GetValue<string>(TcpRpcParserConfig.ProxyTokenProperty);
            this.NameSpace = ServiceConfig.GetValue<string>(TcpRpcParserConfig.NameSpaceProperty);
            this.RPCVersion = ServiceConfig.GetValue<Version>(TcpRpcParserConfig.RPCVersionProperty);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="createOption"></param>
        protected override void OnCreateSocketClient(TClient socketClient, CreateOption createOption)
        {
            socketClient.IDAction = this.IDInvoke;
            socketClient.Received = this.OnReceived;
            socketClient.methodMap = this.MethodMap;
            socketClient.executeMethod = this.Execute;
            socketClient.serializationSelector = this.serializationSelector;
        }

        private RpcContext IDInvoke(RpcSocketClient socketClient, RpcContext context)
        {
            if (this.TryGetSocketClient(context.ID, out TClient targetsocketClient))
            {
                try
                {
                    context.returnParameterBytes = targetsocketClient.CallBack(context, 5);
                    context.Status = 1;
                }
                catch (Exception ex)
                {
                    context.Status = 3;
                    context.Message = ex.Message;
                }
            }
            else
            {
                context.Status = 2;
            }

            return context;
        }

        private void OnReceived(IClient sender, short? procotol, ByteBlock byteBlock)
        {
            this.Received?.Invoke(sender, procotol, byteBlock);
        }
    }
}