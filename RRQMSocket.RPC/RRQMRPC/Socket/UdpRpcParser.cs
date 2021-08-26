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
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UDP RPC解释器
    /// </summary>
    public class UdpRpcParser : UdpSession, IRPCParser, IRRQMRpcParser
    {
        private MethodStore methodStore;

        private RpcProxyInfo proxyInfo;

        private SerializationSelector serializationSelector;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpRpcParser()
        {
            this.methodStore = new MethodStore();
            this.proxyInfo = new RpcProxyInfo();
        }

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
        public MethodStore MethodStore => this.methodStore;

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
        /// 序列化选择器
        /// </summary>
        public SerializationSelector SerializationSelector
        {
            get { return serializationSelector; }
        }

#if NET45_OR_GREATER

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetDic"><inheritdoc/></param>
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


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void ExecuteContext(RpcContext context, ICaller caller)
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
                        object[] ps;
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            ps = new object[methodInstance.ParameterTypes.Length];
                            RpcServerCallContext serverCallContext = new RpcServerCallContext();
                            serverCallContext.caller = caller;
                            serverCallContext.methodInstance = methodInstance;
                            serverCallContext.methodInvoker = methodInvoker;
                            serverCallContext.context = context;
                            ps[0] = serverCallContext;
                            for (int i = 0; i < context.parametersBytes.Count; i++)
                            {
                                ps[i + 1] = this.serializationSelector.DeserializeParameter(context.SerializationType, context.ParametersBytes[i], methodInstance.ParameterTypes[i + 1]);
                            }
                        }
                        else
                        {
                            ps = new object[methodInstance.ParameterTypes.Length];
                            for (int i = 0; i < methodInstance.ParameterTypes.Length; i++)
                            {
                                ps[i] = this.serializationSelector.DeserializeParameter(context.SerializationType, context.ParametersBytes[i], methodInstance.ParameterTypes[i]);
                            }
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
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
        public virtual List<MethodItem> GetRegisteredMethodItems(string proxyToken, ICaller caller)
        {
            return this.methodStore.GetAllMethodItem();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
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

                context.Serialize(byteBlock);
                this.UDPSend(101, ((UdpCaller)methodInvoker.Caller).CallerEndPoint, byteBlock.Buffer, 0, byteBlock.Len);
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
        /// <inheritdoc/>
        /// </summary>
        public void OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            RRQMRPCTools.GetRPCMethod(methodInstances, this.NameSpace, ref this.methodStore, this.RPCVersion, ref this.proxyInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
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
        public void SetExecuteMethod(Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.RRQMExecuteMethod = executeMethod;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetMethodMap(MethodMap methodMap)
        {
            this.MethodMap = methodMap;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetRPCService(RPCService service)
        {
            this.RPCService = service;
        }

        /// <summary>
        /// 密封处理
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected sealed override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = byteBlock.Len;
            short procotol = BitConverter.ToInt16(buffer, 0);

            switch (procotol)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        try
                        {
                            string proxyToken = Encoding.UTF8.GetString(buffer, 2, r - 2);
                            this.UDPSend(100, remoteEndPoint,
                                SerializeConvert.RRQMBinarySerialize(this.GetProxyInfo(proxyToken, new UdpCaller(this, remoteEndPoint)), true));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"UDP错误代码: 100, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 101:/*函数式调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            RpcContext content = RpcContext.Deserialize(byteBlock);
                            if (content.Feedback == 1)
                            {
                                List<byte[]> ps = content.parametersBytes;

                                ByteBlock returnByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                                try
                                {
                                    content.parametersBytes = null;
                                    content.Status = 1;
                                    content.Serialize(returnByteBlock);
                                    this.UDPSend(101, remoteEndPoint, returnByteBlock.Buffer, 0, returnByteBlock.Len);
                                }
                                finally
                                {
                                    content.parametersBytes = ps;
                                    returnByteBlock.Dispose();
                                }
                            }
                            this.ExecuteContext(content, new UdpCaller(this, remoteEndPoint));
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
                            string proxyToken = Encoding.UTF8.GetString(buffer, 2, r - 2);
                            UDPSend(102, remoteEndPoint, SerializeConvert.RRQMBinarySerialize(
                                this.GetRegisteredMethodItems(proxyToken, new UdpCaller(this, remoteEndPoint)), true));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void LoadConfig(ServiceConfig ServiceConfig)
        {
            base.LoadConfig(ServiceConfig);
            this.serializationSelector = (SerializationSelector)ServiceConfig.GetValue(UdpRpcParserConfig.SerializationSelectorProperty);
            this.ProxyToken = (string)ServiceConfig.GetValue(UdpRpcParserConfig.ProxyTokenProperty);
            this.NameSpace = (string)ServiceConfig.GetValue(UdpRpcParserConfig.NameSpaceProperty);
            this.RPCVersion = (Version)ServiceConfig.GetValue(UdpRpcParserConfig.RPCVersionProperty);
        }

        private void UDPSend(short procotol, EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length + 2);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(procotol));
                byteBlock.Write(buffer, offset, length);
                this.SendTo(byteBlock.Buffer, 0, byteBlock.Len, endPoint);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void UDPSend(short procotol, EndPoint endPoint, byte[] buffer)
        {
            this.UDPSend(procotol, endPoint, buffer, 0, buffer.Length);
        }
    }
}