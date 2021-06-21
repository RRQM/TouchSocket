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
using RRQMCore.Log;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UDP RPC解释器
    /// </summary>
    public class UdpRPCParser : UdpSession, IRPCParser, IRRQMRPCParser
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpRPCParser()
        {

        }
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
        public MethodStore MethodStore => this.methodStore;


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
            this.SerializeConverter = (SerializeConverter)serverConfig.GetValue(UdpRPCParserConfig.SerializeConverterProperty);
            this.ProxyToken = (string)serverConfig.GetValue(UdpRPCParserConfig.ProxyTokenProperty);
            this.RPCCompiler = (IRPCCompiler)serverConfig.GetValue(UdpRPCParserConfig.RPCCompilerProperty);
            this.NameSpace = (string)serverConfig.GetValue(UdpRPCParserConfig.NameSpaceProperty);
            this.RPCVersion = (Version)serverConfig.GetValue(UdpRPCParserConfig.RPCVersionProperty);
        }

        public virtual List<MethodItem> GetRegisteredMethodItems(object caller)
        {
            return this.methodStore.GetAllMethodItem();
        }
#pragma warning restore


        private void UDPSend(short procotol, EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length + 2);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(procotol));
                byteBlock.Write(buffer, offset, length);
                this.SendTo(byteBlock.Buffer, 0, (int)byteBlock.Length, endPoint);
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

        /// <summary>
        /// 密封处理
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected sealed override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
            short procotol = BitConverter.ToInt16(buffer, 0);

            switch (procotol)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        try
                        {
                            string proxyToken = Encoding.UTF8.GetString(buffer, 2, r - 2);
                            this.UDPSend(100, remoteEndPoint, SerializeConvert.RRQMBinarySerialize(this.GetProxyInfo(proxyToken, remoteEndPoint), true));
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
                            RPCContext content = RPCContext.Deserialize(buffer, 2);
                            this.ExecuteContext(content, remoteEndPoint);
                            if (content.Feedback != 0)
                            {
                                this.UDPSend(101, remoteEndPoint, new byte[0]);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"UDP错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*连接初始化*/
                    {
                        try
                        {
                            UDPSend(102, remoteEndPoint, SerializeConvert.RRQMBinarySerialize(this.GetRegisteredMethodItems(remoteEndPoint), true));
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
        /// 初始化
        /// </summary>
        /// <param name="providers"></param>
        /// <param name="methodInstances"></param>
        public void RRQMInitializeServers(ServerProviderCollection providers, MethodInstance[] methodInstances)
        {
            Tools.GetRPCMethod(providers, methodInstances, this.NameSpace, providers.SingleAssembly, out this.methodStore,
                 this.RPCVersion, this.RPCCompiler, out this.proxyInfo, out this.codes);
        }

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        public void RRQMEndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            return;
        }


    }
}