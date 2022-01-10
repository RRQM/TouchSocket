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
using RRQMSocket.Http;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;

namespace RRQMSocket.RPC.XmlRpc
{
    /// <summary>
    /// XmlRpc解析器
    /// </summary>
    public class XmlRpcParser : TcpService<XmlRpcSocketClient>, IRPCParser
    {
        private ActionMap actionMap;

        private int maxPackageSize;

        private string proxyToken;

        /// <summary>
        /// 构造函数
        /// </summary>
        public XmlRpcParser()
        {
            this.actionMap = new ActionMap();
        }

        /// <summary>
        /// 服务键映射图
        /// </summary>
        public ActionMap ActionMap
        { get { return this.actionMap; } }

        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public int MaxPackageSize
        {
            get { return maxPackageSize; }
        }

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 代理令箭，当获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken
        {
            get { return proxyToken; }
        }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RPCService RPCService { get; private set; }
        /// <summary>
        /// 执行函数
        /// </summary>
        public Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        public void GetProxyInfo(GetProxyInfoArgs args)
        {
            if (args.RpcType.HasFlag(RpcType.XmlRpc))
            {
                if (args.ProxyToken != this.ProxyToken)
                {
                    args.ErrorMessage = "在验证RRQMRPC时令箭不正确。";
                    args.IsSuccess = false;
                    return;
                }
                foreach (var item in this.RPCService.ServerProviders)
                {
                    var serverCellCode = CodeGenerator.Generator<XmlRpcAttribute>(item.GetType());
                    args.Codes.Add(serverCellCode);
                }
            }
        }

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        public void OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            HttpRequest httpRequest = (HttpRequest)methodInvoker.Flag;
            SimpleSocketClient socketClient = (SimpleSocketClient)methodInvoker.Caller;

            HttpResponse httpResponse = new HttpResponse();

            httpResponse.ProtocolVersion = httpRequest.ProtocolVersion;
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);

            if (methodInvoker.Status == InvokeStatus.Success)
            {
                XmlDataTool.CreatResponse(httpResponse, methodInvoker.ReturnParameter);
            }
            else
            {
                httpResponse.StatusCode = "201";
                httpResponse.StatusMessage = methodInvoker.StatusMessage;
            }
            try
            {
                httpResponse.Build(byteBlock);
                socketClient.Send(byteBlock);
            }
            finally
            {
                byteBlock.Dispose();
            }

            if (!httpRequest.KeepAlive)
            {
                socketClient.Shutdown(SocketShutdown.Both);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        public void OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                foreach (var att in methodInstance.RPCAttributes)
                {
                    if (att is XmlRpcAttribute attribute)
                    {
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            throw new RRQMRPCException("XmlRpc不支持上下文调用");
                        }
                        if (methodInstance.IsByRef)
                        {
                            throw new RRQMRPCException("XmlRpc服务中不允许有out及ref关键字");
                        }
                        string actionKey = CodeGenerator.GetMethodName<XmlRpcAttribute>(methodInstance);

                        this.actionMap.Add(actionKey, methodInstance);
                    }
                }
            }
        }

        /// <summary>
        /// 取消注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        public void OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
        }

        /// <summary>
        /// 设置执行委托
        /// </summary>
        /// <param name="executeMethod"></param>
        public void SetExecuteMethod(Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.RRQMExecuteMethod = executeMethod;
        }

        /// <summary>
        /// 设置地图映射
        /// </summary>
        /// <param name="methodMap"></param>
        public void SetMethodMap(MethodMap methodMap)
        {
            this.MethodMap = methodMap;
        }

        /// <summary>
        /// 设置服务
        /// </summary>
        /// <param name="service"></param>
        public void SetRPCService(RPCService service)
        {
            this.RPCService = service;
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(ServiceConfig serviceConfig)
        {
            this.maxPackageSize = (int)serviceConfig.GetValue(XmlRpcParserConfig.MaxPackageSizeProperty);
            this.proxyToken = serviceConfig.GetValue<string>(XmlRpcParserConfig.ProxyTokenProperty);
            base.LoadConfig(serviceConfig);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(XmlRpcSocketClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.Received += this.OnReceived;
            socketClient.InternalSetAdapter(new HttpDataHandlingAdapter(this.maxPackageSize, HttpType.Server));
            base.OnConnecting(socketClient, e);
        }

        private void OnReceived(SimpleSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            HttpRequest httpRequest = (HttpRequest)obj;
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = (XmlRpcSocketClient)socketClient;
            methodInvoker.Flag = httpRequest;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(httpRequest.Body);
            XmlNode methodName = xml.SelectSingleNode("methodCall/methodName");
            string actionKey = methodName.InnerText;

            if (this.actionMap.TryGet(actionKey, out MethodInstance methodInstance))
            {
                if (methodInstance.IsEnable)
                {
                    try
                    {
                        List<object> ps = new List<object>();
                        XmlNode paramsNode = xml.SelectSingleNode("methodCall/params");
                        int index = 0;
                        foreach (XmlNode paramNode in paramsNode.ChildNodes)
                        {
                            XmlNode valueNode = paramNode.FirstChild.FirstChild;
                            ps.Add(XmlDataTool.GetValue(valueNode, methodInstance.ParameterTypes[index]));
                            index++;
                        }

                        methodInvoker.Parameters = ps.ToArray();
                    }
                    catch (Exception ex)
                    {
                        methodInvoker.Status = InvokeStatus.Exception;
                        methodInvoker.StatusMessage = ex.Message;
                        this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                    }
                }
                else
                {
                    methodInvoker.Status = InvokeStatus.UnEnable;
                    methodInvoker.StatusMessage = "服务不可用";
                }
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
                methodInvoker.StatusMessage = "没有找到这个服务。";
            }

            this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
        }
    }
}