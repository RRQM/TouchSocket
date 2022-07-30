//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Dependency;
using TouchSocket.Http;
using TouchSocket.Http.Plugins;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.XmlRpc
{
    /// <summary>
    /// XmlRpc解析器
    /// </summary>
    public class XmlRpcParserPlugin : HttpPluginBase, IRpcParser
    {
        private RpcStore m_rpcStore;
        private string m_xmlRpcUrl = "/xmlrpc";
        readonly ActionMap m_actionMap;
        /// <summary>
        /// 构造函数
        /// </summary>
        public XmlRpcParserPlugin([DependencyParamterInject(true)] RpcStore rpcStore)
        {
            this.m_actionMap = new ActionMap();
            rpcStore?.AddRpcParser(this.GetType().Name, this);
        }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RpcStore RpcStore => this.m_rpcStore;

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        public string XmlRpcUrl
        {
            get => this.m_xmlRpcUrl;
            set => this.m_xmlRpcUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// XmlRpc调用
        /// </summary>
        public ActionMap ActionMap => this.m_actionMap;

        #region RPC解析器

        void IRpcParser.OnRegisterServer(IRpcServer provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<XmlRpcAttribute>() is XmlRpcAttribute attribute)
                {
                    this.m_actionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        void IRpcParser.OnUnregisterServer(IRpcServer provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<XmlRpcAttribute>() is XmlRpcAttribute attribute)
                {
                    this.m_actionMap.Remove(attribute.GetInvokenKey(methodInstance));
                }
            }
        }

        void IRpcParser.SetRpcStore(RpcStore rpcService)
        {
            this.m_rpcStore = rpcService;
        }
        #endregion RPC解析器

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
            if (this.m_xmlRpcUrl == "/" || e.Context.Request.UrlEquals(this.m_xmlRpcUrl))
            {
                e.Handled = true;

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(e.Context.Request.GetBody());
                XmlNode methodName = xml.SelectSingleNode("methodCall/methodName");
                string actionKey = methodName.InnerText;

                object[] invokePS = null;
                InvokeResult invokeResult = new InvokeResult();
                if (this.m_actionMap.TryGetMethodInstance(actionKey, out MethodInstance methodInstance))
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

                            invokePS = ps.ToArray();
                        }
                        catch (Exception ex)
                        {
                            invokeResult.Status = InvokeStatus.Exception;
                            invokeResult.Message = ex.Message;
                        }
                    }
                    else
                    {
                        invokeResult.Status = InvokeStatus.UnEnable;
                        invokeResult.Message = "服务不可用";
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnFound;
                    invokeResult.Message = "没有找到这个服务。";
                }

                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    invokeResult = this.m_rpcStore.Execute(client, methodInstance, invokePS);
                }


                HttpResponse httpResponse = new HttpResponse();

                ByteBlock byteBlock = new ByteBlock();

                if (invokeResult.Status == InvokeStatus.Success)
                {
                    XmlDataTool.CreatResponse(httpResponse, invokeResult.Result);
                }
                else
                {
                    httpResponse.StatusCode = "201";
                    httpResponse.StatusMessage = invokeResult.Message;
                }
                try
                {
                    httpResponse.Build(byteBlock);
                    client.DefaultSend(byteBlock);
                }
                finally
                {
                    byteBlock.Dispose();
                }

                if (!e.Context.Request.KeepAlive)
                {
                    client.Shutdown(SocketShutdown.Both);
                }
            }
            base.OnPost(client, e);
        }
    }
}