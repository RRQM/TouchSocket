//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Text;
using System.Xml;
using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.XmlRpc;

internal static class XmlDataTool
{
    public static object GetValue(XmlNode valueNode, Type type)
    {
        if (valueNode == null)
        {
            return type.GetDefault();
        }
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
                    var instance = Activator.CreateInstance(type);
                    foreach (XmlNode memberNode in valueNode.ChildNodes)
                    {
                        var name = memberNode.SelectSingleNode("name").InnerText;
                        var property = type.GetProperty(name);
                        property.SetValue(instance, GetValue(memberNode.SelectSingleNode("value").FirstChild, property.PropertyType));
                    }
                    return instance;
                }
            case "arrays":
            case "array":
                {
                    if (type.GetElementType() != null)
                    {
                        var dataNode = valueNode.SelectSingleNode("data");
                        var array = Array.CreateInstance(type.GetElementType(), dataNode.ChildNodes.Count);

                        var index = 0;
                        foreach (XmlNode arrayValueNode in dataNode.ChildNodes)
                        {
                            array.SetValue(GetValue(arrayValueNode.FirstChild, type.GetElementType()), index);
                            index++;
                        }
                        return array;
                    }
                    else if (type.GetGenericArguments().Length == 1)
                    {
                        var dataNode = valueNode.SelectSingleNode("data");
                        var array = (IList)Activator.CreateInstance(type);

                        foreach (XmlNode arrayValueNode in dataNode.ChildNodes)
                        {
                            array.Add(GetValue(arrayValueNode.FirstChild, type.GetGenericArguments()[0]));
                        }
                        return array;
                    }
                    return type.GetDefault();
                }
            default:
            case "string":
                {
                    return valueNode.InnerText;
                }
        }
    }

    public static HttpRequest CreateRequest(HttpClientBase httpClientBase, string method, object[] parameters)
    {
        var xml = new XmlDocument();

        var xmlDecl = xml.CreateXmlDeclaration("1.0", string.Empty, string.Empty);
        xml.AppendChild(xmlDecl);

        var xmlElement = xml.CreateElement("methodCall");
        xml.AppendChild(xmlElement);

        var methodNameElement = xml.CreateElement("methodName");
        methodNameElement.InnerText = method;
        xmlElement.AppendChild(methodNameElement);

        var paramsElement = xml.CreateElement("params");
        xmlElement.AppendChild(paramsElement);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var paramElement = xml.CreateElement("param");
                paramsElement.AppendChild(paramElement);

                var valueElement = xml.CreateElement("value");
                paramElement.AppendChild(valueElement);

                CreateParam(xml, valueElement, param);
            }
        }

        var request = new HttpRequest();
        request.FromXml(xml.OuterXml);
        request.InitHeaders();
        request.URL = (httpClientBase.RemoteIPHost.PathAndQuery);
        request.SetHost(httpClientBase.RemoteIPHost.Host);
        request.AsPost();
        return request;
    }

    public static void CreateParam(XmlDocument xml, XmlNode xmlNode, object value)
    {
        if (value == null)
        {
            return;
        }
        if (value is int)
        {
            var valueElement = xml.CreateElement("i4");
            valueElement.InnerText = value.ToString();
            xmlNode.AppendChild(valueElement);
        }
        else if (value is bool)
        {
            var valueElement = xml.CreateElement("boolean");
            valueElement.InnerText = (value).ToString();
            xmlNode.AppendChild(valueElement);
        }
        else if (value is double)
        {
            var valueElement = xml.CreateElement("double");
            valueElement.InnerText = ((double)value).ToString();
            xmlNode.AppendChild(valueElement);
        }
        else if (value is string)
        {
            var valueElement = xml.CreateElement("string");
            valueElement.InnerText = value.ToString();
            xmlNode.AppendChild(valueElement);
        }
        else if (value is DateTime)
        {
            var valueElement = xml.CreateElement("dateTime.iso8601");
            valueElement.InnerText = ((DateTime)value).ToString();
            xmlNode.AppendChild(valueElement);
        }
        else if (value is byte[])
        {
            var valueElement = xml.CreateElement("base64");
            var str = Convert.ToBase64String((byte[])value);
            valueElement.InnerText = str;
            xmlNode.AppendChild(valueElement);
        }
        else if (typeof(IList).IsAssignableFrom(value.GetType()))
        {
            var array = (IList)value;
            XmlElement arrayElement;

            arrayElement = xml.CreateElement("array");

            xmlNode.AppendChild(arrayElement);

            var dataElememt = xml.CreateElement("data");
            arrayElement.AppendChild(dataElememt);

            foreach (var item in array)
            {
                var valueElement = xml.CreateElement("value");
                dataElememt.AppendChild(valueElement);
                CreateParam(xml, valueElement, item);
            }
        }
        else
        {
            var valueElement = xml.CreateElement("struct");
            xmlNode.AppendChild(valueElement);

            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                var memberElement = xml.CreateElement("member");
                valueElement.AppendChild(memberElement);

                var nameElement = xml.CreateElement("name");
                nameElement.InnerText = propertyInfo.Name;
                memberElement.AppendChild(nameElement);

                var oValueElement = xml.CreateElement("value");
                memberElement.AppendChild(oValueElement);

                var oValue = propertyInfo.GetValue(value);
                CreateParam(xml, oValueElement, oValue);
            }
        }
    }

    public static void CreatResponse(HttpResponse httpResponse, object value)
    {
        var xml = new XmlDocument();

        var xmlDecl = xml.CreateXmlDeclaration("1.0", string.Empty, string.Empty);
        xml.AppendChild(xmlDecl);

        var xmlElement = xml.CreateElement("methodResponse");

        xml.AppendChild(xmlElement);

        var paramsElement = xml.CreateElement("params");
        xmlElement.AppendChild(paramsElement);

        var paramElement = xml.CreateElement("param");
        paramsElement.AppendChild(paramElement);

        var valueElement = xml.CreateElement("value");
        paramElement.AppendChild(valueElement);

        CreateParam(xml, valueElement, value);

        using (var xmlBlock = new ByteBlock(1024*64))
        {
            xml.Save(xmlBlock.AsStream());

            var xmlString = xmlBlock.Span.ToString(Encoding.UTF8);

            httpResponse.FromXml(xmlString);
        }
    }
}