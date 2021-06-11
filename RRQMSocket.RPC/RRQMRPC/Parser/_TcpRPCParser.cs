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
    public abstract class _TcpRPCParser<TClient> : ProtocolService<TClient>, IRPCParser, IRRQMRPCParser where TClient : ProtocolSocketClient, new()
    {
#pragma warning disable 
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

        public MethodStore MethodStore
        {
            get { return methodStore; }
            set { methodStore = value; }
        }


        public event RRQMBytesEventHandler Received;

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
                ((RPCSocketClient)methodInvoker.Caller).agreementHelper.SocketSend(101, byteBlock);
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
            Tools.GetRPCMethod(providers, methodInstances, this.NameSpace, this.RPCService.GetType().Assembly, out this.methodStore,
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


#pragma warning restore
    }
}
