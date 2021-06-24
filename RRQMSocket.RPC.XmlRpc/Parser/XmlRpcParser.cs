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
using RRQMCore.Log;
using RRQMSocket.Http;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
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
        public void RRQMEndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            HttpRequest httpRequest = (HttpRequest)methodInvoker.Flag;
            SimpleSocketClient socketClient = (SimpleSocketClient)methodInvoker.Caller;

            HttpResponse httpResponse = new HttpResponse();

            httpResponse.ProtocolVersion = httpRequest.ProtocolVersion;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);

            XmlDocument xml = new XmlDocument();

            XmlDeclaration xmlDecl = xml.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            xml.AppendChild(xmlDecl);

            XmlElement xmlElement = xml.CreateElement("methodResponse");

            xml.AppendChild(xmlElement);

            XmlElement paramsElement = xml.CreateElement("params");
            xmlElement.AppendChild(paramsElement);

            XmlElement paramElement = xml.CreateElement("param");
            paramsElement.AppendChild(paramElement);

            XmlElement valueElement = xml.CreateElement("value");
            paramElement.AppendChild(valueElement);

            CreatResponse(xml, valueElement, methodInvoker.ReturnParameter);
            ByteBlock xmlBlock = this.BytePool.GetByteBlock(this.BufferLength);
            xml.Save(xmlBlock);

            string xmlString = Encoding.UTF8.GetString(xmlBlock.Buffer, 0, (int)xmlBlock.Position);

            httpResponse.FromXML(xmlString);
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
        /// <param name="providers"></param>
        /// <param name="methodInstances"></param>
        public void RRQMInitializeServers(ServerProviderCollection providers, MethodInstance[] methodInstances)
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
                        string actionKey = string.IsNullOrEmpty(attribute.ActionKey) ? $"{methodInstance.Provider.GetType().Name}.{methodInstance.Method.Name}" : attribute.ActionKey;

                        this.actionMap.Add(actionKey, methodInstance);
                    }
                }
            }
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
            socketClient.DataHandlingAdapter = new HttpDataHandlingAdapter(this.BufferLength);
        }

        private void CreatResponse(XmlDocument xml, XmlNode xmlNode, object value)
        {
            if (value is int)
            {
                XmlElement valueElement = xml.CreateElement("i4");
                valueElement.InnerText = value.ToString();
                xmlNode.AppendChild(valueElement);
            }
            else if (value is bool)
            {
                XmlElement valueElement = xml.CreateElement("boolean");
                valueElement.InnerText = ((int)value).ToString();
                xmlNode.AppendChild(valueElement);
            }
            else if (value is double)
            {
                XmlElement valueElement = xml.CreateElement("double");
                valueElement.InnerText = ((double)value).ToString();
                xmlNode.AppendChild(valueElement);
            }
            else if (value is string)
            {
                XmlElement valueElement = xml.CreateElement("string");
                valueElement.InnerText = value.ToString();
                xmlNode.AppendChild(valueElement);
            }
            else if (value is DateTime)
            {
                XmlElement valueElement = xml.CreateElement("dateTime.iso8601");
                valueElement.InnerText = ((DateTime)value).ToString();
                xmlNode.AppendChild(valueElement);
            }
            else if (value is byte[])
            {
                XmlElement valueElement = xml.CreateElement("base64");
                string str = Convert.ToBase64String((byte[])value);
                valueElement.InnerText = str;
                xmlNode.AppendChild(valueElement);
            }
            else if (value.GetType().IsArray)
            {
                Array array = (Array)value;
                XmlElement arrayElement;

                arrayElement = xml.CreateElement("array");

                xmlNode.AppendChild(arrayElement);

                XmlElement dataElememt = xml.CreateElement("data");
                arrayElement.AppendChild(dataElememt);

                foreach (var item in array)
                {
                    XmlElement valueElement = xml.CreateElement("value");
                    dataElememt.AppendChild(valueElement);
                    CreatResponse(xml, valueElement, item);
                }
            }
            else
            {
                XmlElement valueElement = xml.CreateElement("struct");
                xmlNode.AppendChild(valueElement);

                PropertyInfo[] propertyInfos = value.GetType().GetProperties();
                foreach (var propertyInfo in propertyInfos)
                {
                    XmlElement memberElement = xml.CreateElement("member");
                    valueElement.AppendChild(memberElement);

                    XmlElement nameElement = xml.CreateElement("name");
                    nameElement.InnerText = propertyInfo.Name;
                    memberElement.AppendChild(nameElement);

                    XmlElement oValueElement = xml.CreateElement("value");
                    memberElement.AppendChild(oValueElement);

                    object oValue = propertyInfo.GetValue(value);
                    this.CreatResponse(xml, oValueElement, oValue);
                }
            }
        }

        private object GetValue(XmlNode valueNode, Type type)
        {
            switch (valueNode.Name)
            {
                case "boolean":
                    {
                        return bool.Parse(valueNode.InnerText);
                    }
                case "i4":
                case "int":
                    {
                        return int.Parse(valueNode.InnerText);
                    }
                case "double":
                    {
                        return double.Parse(valueNode.InnerText);
                    }
                case "dateTime.iso8601":
                    {
                        return DateTime.Parse(valueNode.InnerText);
                    }
                case "base64":
                    {
                        return valueNode.InnerText;
                    }
                case "struct":
                    {
                        object instance = Activator.CreateInstance(type);
                        foreach (XmlNode memberNode in valueNode.ChildNodes)
                        {
                            string name = memberNode.SelectSingleNode("name").InnerText;
                            PropertyInfo property = type.GetProperty(name);
                            property.SetValue(instance, this.GetValue(memberNode.SelectSingleNode("value").FirstChild, property.PropertyType));
                        }
                        return instance;
                    }
                case "arrays":
                case "array":
                    {
                        XmlNode dataNode = valueNode.SelectSingleNode("data");
                        Array array = Array.CreateInstance(type.GetElementType(), dataNode.ChildNodes.Count);

                        int index = 0;
                        foreach (XmlNode arrayValueNode in dataNode.ChildNodes)
                        {
                            array.SetValue(this.GetValue(arrayValueNode.FirstChild, type.GetElementType()), index);
                            index++;
                        }
                        return array;
                    }
                default:
                case "string":
                    {
                        return valueNode.InnerText;
                    }
            }
        }

        private void OnReceived(SimpleSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            HttpRequest httpRequest = (HttpRequest)obj;
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = socketClient;
            methodInvoker.Flag = httpRequest;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(httpRequest.BodyString);
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
                            ps.Add(this.GetValue(valueNode, methodInstance.ParameterTypes[index]));
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