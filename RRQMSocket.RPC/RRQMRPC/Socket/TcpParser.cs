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
using RRQMCore.ByteManager;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.IO;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCParser泛型类型
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public class TcpParser<TClient> : ProtocolService<TClient>, IRPCParser, IRRQMRpcParser where TClient : RPCSocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpParser()
        {
            this.methodStore = new MethodStore();
            this.proxyInfo = new RpcProxyInfo();
        }

#pragma warning disable

        public MethodMap MethodMap { get; private set; }

        public RPCService RPCService { get; private set; }

        public Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        public CellCode[] Codes { get => this.proxyInfo == null ? null : this.proxyInfo.Codes.ToArray(); }

        public string NameSpace { get; private set; }

        public RpcProxyInfo ProxyInfo { get => proxyInfo; }

        public string ProxyToken { get; private set; }

        public Version RPCVersion { get; private set; }

        public SerializeConverter SerializeConverter { get; private set; }

        private MethodStore methodStore;
        
        private RpcProxyInfo proxyInfo;

        public MethodStore MethodStore { get => methodStore; }

        public void OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RpcContext context = (RpcContext)methodInvoker.Flag;
            if (context.Feedback != 2)
            {
                return;
            }
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            try
            {
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
                                context.ReturnParameterBytes = this.SerializeConverter.SerializeParameter(methodInvoker.ReturnParameter);
                            }
                            else
                            {
                                context.ReturnParameterBytes = null;
                            }

                            if (methodInstance.IsByRef)
                            {
                                context.ParametersBytes = new List<byte[]>();
                                foreach (var item in methodInvoker.Parameters)
                                {
                                    context.ParametersBytes.Add(this.SerializeConverter.SerializeParameter(item));
                                }
                            }
                            else
                            {
                                context.ParametersBytes = null;
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

                context.Serialize(byteBlock);
                ((RPCSocketClient)methodInvoker.Caller).InternalSend(101, byteBlock.Buffer, 0, byteBlock.Len);
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

        public void OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            Tools.GetRPCMethod(methodInstances, this.NameSpace, ref this.methodStore, this.RPCVersion, ref this.proxyInfo);
        }

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

        public void SetExecuteMethod(Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.RRQMExecuteMethod = executeMethod;
        }

        public void SetMethodMap(MethodMap methodMap)
        {
            this.MethodMap = methodMap;
        }

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
            RpcCompiler.CompileCode(Path.Combine(targetDic, this.proxyInfo.AssemblyName), codesString.ToArray());
        }

#endif

        protected override void OnCreateSocketCliect(TClient socketClient, CreateOption createOption)
        {
            if (createOption.NewCreate)
            {
                socketClient.IDAction = this.IDInvoke;
                socketClient.Received = this.OnReceived;
                socketClient.serializeConverter = this.SerializeConverter;
            }
        }

        private void OnReceived(object sender, short? procotol, ByteBlock byteBlock)
        {
            this.Received?.Invoke(sender, procotol, byteBlock);
        }

        public virtual RpcProxyInfo GetProxyInfo(string proxyToken, object caller)
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

        public virtual void ExecuteContext(RpcContext context, object caller)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = caller;
            methodInvoker.Flag = context;
            if (this.MethodMap.TryGet(context.MethodToken, out MethodInstance methodInstance))
            {
                try
                {
                    if (methodInstance.IsEnable)
                    {
                        object[] ps = new object[methodInstance.ParameterTypes.Length];
                        for (int i = 0; i < methodInstance.ParameterTypes.Length; i++)
                        {
                            ps[i] = this.SerializeConverter.DeserializeParameter(context.ParametersBytes[i], methodInstance.ParameterTypes[i]);
                        }
                        methodInvoker.Parameters = ps;
                    }
                    else
                    {
                        methodInvoker.Status = InvokeStatus.UnEnable;
                    }
                }
                catch (Exception ex)
                {
                    methodInvoker.Status = InvokeStatus.Exception;
                    methodInvoker.StatusMessage = ex.Message;
                }

                this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
                this.RRQMExecuteMethod.Invoke(this, methodInvoker, null);
            }
        }

        protected override void LoadConfig(ServiceConfig ServiceConfig)
        {
            base.LoadConfig(ServiceConfig);
            this.SerializeConverter = (SerializeConverter)ServiceConfig.GetValue(TcpRpcParserConfig.SerializeConverterProperty);
            this.ProxyToken = (string)ServiceConfig.GetValue(TcpRpcParserConfig.ProxyTokenProperty);
            this.NameSpace = (string)ServiceConfig.GetValue(TcpRpcParserConfig.NameSpaceProperty);
            this.RPCVersion = (Version)ServiceConfig.GetValue(TcpRpcParserConfig.RPCVersionProperty);
        }

        public virtual List<MethodItem> GetRegisteredMethodItems(string proxyToken, object caller)
        {
            if (proxyToken == this.ProxyToken)
            {
                return this.methodStore.GetAllMethodItem();
            }
            return null;
        }

        private RpcContext IDInvoke(RPCSocketClient socketClient, RpcContext context)
        {
            if (this.TryGetSocketClient(context.ID, out TClient targetsocketClient))
            {
                try
                {
                    context.ReturnParameterBytes = targetsocketClient.CallBack(context, 5);
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

#pragma warning disable

        /// <summary>
        /// 收到协议数据
        /// </summary>
        public event RRQMReceivedProcotolEventHandler Received;

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

    }
}