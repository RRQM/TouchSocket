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

using System.Xml.Linq;

namespace TouchSocket.Core
{
    public static class XElementExtension
    {
        public static void AddAttribute(this XElement element, XName name, object value)
        {
            if (name is null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(name));
            }

            element.Add(new XAttribute(name, value ?? string.Empty));
        }

        public static T GetAttributeValue<T>(this XElement xmlNode, XName name, T defaultValue) where T : unmanaged
        {
            var str = xmlNode.GetAttributeValue(name);
            if (str.IsNullOrEmpty())
            {
                return defaultValue;
            }
            return (T)StringExtension.ParseToType(str, typeof(T));
        }

        public static T GetAttributeValue<T>(this XElement xmlNode, XName name) where T : unmanaged
        {
            return GetAttributeValue<T>(xmlNode, name, default);
        }

        public static string GetAttributeValue(this XElement xmlNode, XName name)
        {
            return xmlNode.Attribute(name)?.Value;
        }

        /// <summary>
        /// 获取对应属性值，如果没有找到这个属性，或属性值为null，则采用设定的默认值。
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetAttributeValue(this XElement xmlNode, XName name, string defaultValue)
        {
            return xmlNode.Attribute(name)?.Value ?? defaultValue;
        }
    }
}