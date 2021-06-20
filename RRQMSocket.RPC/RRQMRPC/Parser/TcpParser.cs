using RRQMCore.ByteManager;
using RRQMCore.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCParser泛型类型
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public class TcpParser<TClient> : ProtocolService<TClient>, IRPCParser, IRRQMRPCParser where TClient : RPCSocketClient, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpParser()
        {
            this.eventBus = new EventBus();
            this.methodStore = new MethodStore();
        }

#pragma warning disable

        /// <summary>
        /// 事务总线
        /// </summary>
        public EventBus EventBus { get => this.eventBus; }

        public MethodMap MethodMap { get; private set; }

        public RPCService RPCService { get; private set; }

        public Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        public CellCode[] Codes { get => codes; }

        public string NameSpace { get; private set; }

        public RPCProxyInfo ProxyInfo { get => proxyInfo; }

        public string ProxyToken { get; private set; }

        public IRPCCompiler RPCCompiler { get; private set; }

        public Version RPCVersion { get; private set; }

        public SerializeConverter SerializeConverter { get; private set; }

        private MethodStore methodStore;
        private RPCProxyInfo proxyInfo;
        private CellCode[] codes;
        private EventBus eventBus;

        public MethodStore MethodStore { get => methodStore; }

        public void RRQMEndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RPCContext context = (RPCContext)methodInvoker.Flag;
            if (context.Feedback == 0)
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
                            break;
                        }
                    case InvokeStatus.Exception:
                        {
                            break;
                        }
                    default:
                        break;
                }

                context.Serialize(byteBlock);
                ((RPCSocketClient)methodInvoker.Caller).InternalSend(101, byteBlock.Buffer, 0, (int)byteBlock.Length);
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

        public void RRQMInitializeServers(ServerProviderCollection providers, MethodInstance[] methodInstances)
        {
            Tools.GetRPCMethod(providers, methodInstances, this.NameSpace, providers.SingleAssembly, out this.methodStore,
           this.RPCVersion, this.RPCCompiler, out this.proxyInfo, out this.codes);
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

        public virtual RPCProxyInfo GetProxyInfo(string proxyToken, object caller)
        {
            RPCProxyInfo proxyInfo = new RPCProxyInfo();
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

        public virtual void ExecuteContext(RPCContext context, object caller)
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

        protected override void LoadConfig(ServerConfig serverConfig)
        {
            base.LoadConfig(serverConfig);
            this.SerializeConverter = (SerializeConverter)serverConfig.GetValue(TcpRPCParserConfig.SerializeConverterProperty);
            this.ProxyToken = (string)serverConfig.GetValue(TcpRPCParserConfig.ProxyTokenProperty);
            this.RPCCompiler = (IRPCCompiler)serverConfig.GetValue(TcpRPCParserConfig.RPCCompilerProperty);
            this.NameSpace = (string)serverConfig.GetValue(TcpRPCParserConfig.NameSpaceProperty);
            this.RPCVersion = (Version)serverConfig.GetValue(TcpRPCParserConfig.RPCVersionProperty);
        }

        public virtual List<MethodItem> GetRegisteredMethodItems(object caller)
        {
            return this.methodStore.GetAllMethodItem();
        }

        private RPCContext IDInvoke(RPCSocketClient socketClient, RPCContext context)
        {
            if (this.TryGetSocketClient(context.ID, out TClient targetsocketClient))
            {
                try
                {
                    context.ReturnParameterBytes = targetsocketClient.CallBack(context, context.Feedback == 1 ? InvokeOption.CanFeedback : InvokeOption.NoFeedback);
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

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="parameterTypes">事件参数类型</param>
        public void PublishEvent<T>(string eventName)
        {
            EventUnit eventUnit = new EventUnit();
            eventUnit.ParameterTypes = typeof(T).Name;
            eventUnit.Publisher = this.ServerName;
            this.eventBus.AddEvent(eventUnit);
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName"></param>
        public void RaiseEvent(string eventName)
        {
            if (!this.eventBus.TryGetEventUnit(eventName, out EventUnit eventUnit))
            {
                throw new RRQMRPCException("没有该事件的注册信息");
            }

        }
    }
}
