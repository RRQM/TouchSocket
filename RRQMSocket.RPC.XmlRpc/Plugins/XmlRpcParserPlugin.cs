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
using RRQMCore.ByteManager;
using RRQMCore.Log;
using RRQMSocket.Http;
using RRQMSocket.Http.Plugins;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;

namespace RRQMSocket.RPC.XmlRpc
{
    /// <summary>
    /// XmlRpc解析器
    /// </summary>
    public class XmlRpcParserPlugin : HttpPluginBase, IRpcParser
    {
        private ActionMap actionMap;

        private string xmlRpcUrl;

        /// <summary>
        /// 构造函数
        /// </summary>
        public XmlRpcParserPlugin(string xmlRpcUrl = "/xmlrpc")
        {
            this.actionMap = new ActionMap();
            this.XmlRpcUrl = xmlRpcUrl;
        }

        /// <summary>
        /// 服务键映射图
        /// </summary>
        public ActionMap ActionMap => this.actionMap;

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 代理令箭，当获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken { get; set; }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RpcService RpcService { get; private set; }

        /// <summary>
        /// 执行函数
        /// </summary>
        public Action<IRpcParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        public string XmlRpcUrl
        {
            get => this.xmlRpcUrl;
            set => this.xmlRpcUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        public virtual void GetProxyInfo(GetProxyInfoArgs args)
        {
            if (args.RpcType.HasFlag(RpcType.XmlRpc))
            {
                if (args.ProxyToken != this.ProxyToken)
                {
                    args.Message = "在验证XmlRpc时令箭不正确。";
                    args.RemoveOperation(RRQMCore.Operation.Permit);
                    return;
                }
                foreach (var item in this.RpcService.ServerProviders)
                {
                    var serverCellCode = CodeGenerator.Generator<XmlRpcAttribute>(item.GetType());
                    args.Codes.Add(serverCellCode);
                }
            }
        }

        #region RPC解析器
        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        void IRpcParser.OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            HttpRequest httpRequest = (HttpRequest)methodInvoker.Flag;
            SocketClient socketClient = (SocketClient)methodInvoker.Caller;

            HttpResponse httpResponse = new HttpResponse();

            httpResponse.ProtocolVersion = httpRequest.ProtocolVersion;
            ByteBlock byteBlock = BytePool.GetByteBlock();

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
        void IRpcParser.OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                foreach (var att in methodInstance.RpcAttributes)
                {
                    if (att is XmlRpcAttribute attribute)
                    {
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            throw new RpcException("XmlRpc不支持上下文调用");
                        }
                        if (methodInstance.IsByRef)
                        {
                            throw new RpcException("XmlRpc服务中不允许有out及ref关键字");
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
        void IRpcParser.OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
        }

        /// <summary>
        /// 设置执行委托
        /// </summary>
        /// <param name="executeMethod"></param>
        void IRpcParser.SetExecuteMethod(Action<IRpcParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.RRQMExecuteMethod = executeMethod;
        }

        /// <summary>
        /// 设置地图映射
        /// </summary>
        /// <param name="methodMap"></param>
        void IRpcParser.SetMethodMap(MethodMap methodMap)
        {
            this.MethodMap = methodMap;
        }

        /// <summary>
        /// 设置代理令箭，当获取代理文件时需验证令箭
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public XmlRpcParserPlugin SetProxyToken(string value)
        {
            this.ProxyToken = value;
            return this;
        }

        /// <summary>
        /// 设置服务
        /// </summary>
        /// <param name="service"></param>
        void IRpcParser.SetRpcService(RpcService service)
        {
            this.RpcService = service;
        }

        #endregion

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        /// <param name="xmlRpcUrl"></param>
        /// <returns></returns>
        public XmlRpcParserPlugin SetXmlRpcUrl(string xmlRpcUrl)
        {
            this.XmlRpcUrl = xmlRpcUrl;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.xmlRpcUrl == "/" || e.Request.RelativeURL.Equals(this.xmlRpcUrl, StringComparison.OrdinalIgnoreCase))
            {
                MethodInvoker methodInvoker = new MethodInvoker();
                methodInvoker.Caller = client;
                methodInvoker.Flag = e.Request;

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(e.Request.GetBody());
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
                e.Handled = true;
                this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
            }
            base.OnPost(client, e);
        }
    }
}