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

namespace TouchSocket.Core;

/// <summary>
/// 扩展XElement类，为其添加属性操作相关的方法。
/// </summary>
public static class XElementExtension
{
    /// <summary>
    /// 向指定的XElement元素添加一个属性。
    /// </summary>
    /// <param name="element">要添加属性的XElement对象。</param>
    /// <param name="name">要添加的属性的名称。</param>
    /// <param name="value">要添加的属性的值。</param>
    /// <remarks>
    /// 此方法简化了向XElement对象添加属性的流程。
    /// </remarks>
    public static void AddAttribute(this XElement element, XName name, object value)
    {
        if (name is null)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(name));
        }

        element.Add(new XAttribute(name, value ?? string.Empty));
    }

    /// <summary>
    /// 获取指定名称的属性值，如果属性不存在或值为空，则返回默认值。
    /// </summary>
    /// <typeparam name="T">属性值的类型，必须是值类型。</typeparam>
    /// <param name="xmlNode">要获取属性值的XElement对象。</param>
    /// <param name="name">属性的名称。</param>
    /// <param name="defaultValue">如果属性不存在或值为空时返回的默认值。</param>
    /// <returns>属性值或默认值。</returns>
    public static T GetAttributeValue<T>(this XElement xmlNode, XName name, T defaultValue) where T : unmanaged
    {
        var str = xmlNode.GetAttributeValue(name);
        return str.IsNullOrEmpty() ? defaultValue : (T)StringExtension.ParseToType(str, typeof(T));
    }

    /// <summary>
    /// 获取指定名称的属性值，如果属性不存在，则返回默认值。
    /// </summary>
    /// <typeparam name="T">属性值的类型，必须是值类型。</typeparam>
    /// <param name="xmlNode">要获取属性值的XElement对象。</param>
    /// <param name="name">属性的名称。</param>
    /// <returns>属性值或默认值。</returns>
    /// <remarks>
    /// 此方法重载了GetAttributeValue方法，用于在未指定默认值的情况下获取属性值。
    /// </remarks>
    public static T GetAttributeValue<T>(this XElement xmlNode, XName name) where T : unmanaged
    {
        return GetAttributeValue<T>(xmlNode, name, default);
    }

    /// <summary>
    /// 获取指定名称的属性值。
    /// </summary>
    /// <param name="xmlNode">要获取属性值的XElement对象。</param>
    /// <param name="name">属性的名称。</param>
    /// <returns>属性值，如果属性不存在则返回null。</returns>
    public static string GetAttributeValue(this XElement xmlNode, XName name)
    {
        return xmlNode.Attribute(name)?.Value;
    }

    /// <summary>
    /// 获取指定名称的属性值，如果属性不存在，则返回默认值。
    /// </summary>
    /// <param name="xmlNode">要获取属性值的XElement对象。</param>
    /// <param name="name">属性的名称。</param>
    /// <param name="defaultValue">如果属性不存在时返回的默认值。</param>
    /// <returns>属性值或默认值。</returns>
    public static string GetAttributeValue(this XElement xmlNode, XName name, string defaultValue)
    {
        return xmlNode.Attribute(name)?.Value ?? defaultValue;
    }
}