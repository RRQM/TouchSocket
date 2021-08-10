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
    public class XmlRpcParser : TcpService<SimpleSocketClient>, IRPCParser
    {
        private ActionMap actionMap;

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
        public ActionMap ActionMap { get { return this.actionMap; } }

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RPCService RPCService { get; private set; }

        /// <summary>
        /// 执行函数
        /// </summary>
        public Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

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
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);

            XmlDataTool.CreatResponse(httpResponse, methodInvoker.ReturnParameter);
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
        public void OnRegisterServer(ServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                foreach (var att in methodInstance.RPCAttributes)
                {
                    if (att is XmlRpcAttribute attribute)
                    {
                        if (methodInstance.IsByRef)
                        {
                            throw new RRQMRPCException("XmlRpc服务中不允许有out及ref关键字");
                        }
                        string actionKey = string.IsNullOrEmpty(attribute.ActionKey) ? $"{methodInstance.Method.Name}" : attribute.ActionKey;

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
        public void OnUnregisterServer(ServerProvider provider, MethodInstance[] methodInstances)
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
        /// 初始化
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="createOption"></param>
        protected override void OnCreateSocketCliect(SimpleSocketClient socketClient, CreateOption createOption)
        {
            if (createOption.NewCreate)
            {
                socketClient.OnReceived = this.OnReceived;
            }
            socketClient.SetDataHandlingAdapter(new HttpDataHandlingAdapter(this.BufferLength, HttpType.Server));
        }

        private void OnReceived(SimpleSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            HttpRequest httpRequest = (HttpRequest)obj;
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = socketClient;
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
                }
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
            }

            this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
        }
    }
}