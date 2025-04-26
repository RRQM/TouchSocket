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

using System.Text;

namespace TouchSocket.Http;

/// <summary>
/// 表示以字符串形式存储的 HTTP 内容。
/// </summary>
/// <remarks>
/// 该类继承自 ReadonlyMemoryHttpContent，用于处理只读的内存中 HTTP 内容。
/// 它将字符串内容转换为字节数组，并传递给基类以进行处理。
/// </remarks>
public class StringHttpContent : ReadonlyMemoryHttpContent
{
    /// <summary>
    /// 初始化 StringHttpContent 类的新实例。
    /// </summary>
    /// <param name="content">要包含的字符串内容。</param>
    /// <param name="encoding">用于将字符串内容编码为字节数组的编码方式。</param>
    /// <param name="contentType">内容类型</param>
    public StringHttpContent(string content, Encoding encoding, string contentType = default) : base(encoding.GetBytes(content), contentType)
    {
        // 构造函数将字符串内容和编码方式作为参数，将字符串内容转换为字节数组后传递给基类。
    }

    /// <summary>
    /// 从 JSON 字符串创建 <see cref="StringHttpContent"/> 实例。
    /// </summary>
    /// <param name="json">JSON 字符串。</param>
    /// <param name="encoding">用于编码的字符集。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromJson(string json, Encoding encoding)
    {
        return new StringHttpContent(json, encoding, $"application/json;charset={encoding.BodyName}");
    }

    /// <summary>
    /// 从 JSON 字符串创建 <see cref="StringHttpContent"/> 实例，使用 UTF-8 编码。
    /// </summary>
    /// <param name="json">JSON 字符串。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromJson(string json)
    {
        return new StringHttpContent(json, Encoding.UTF8, $"application/json;charset=UTF-8");
    }

    /// <summary>
    /// 从 XML 字符串创建 <see cref="StringHttpContent"/> 实例。
    /// </summary>
    /// <param name="xml">XML 字符串。</param>
    /// <param name="encoding">用于编码的字符集。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromXml(string xml, Encoding encoding)
    {
        return new StringHttpContent(xml, encoding, $"application/xml;charset={encoding.BodyName}");
    }

    /// <summary>
    /// 从 XML 字符串创建 <see cref="StringHttpContent"/> 实例，使用 UTF-8 编码。
    /// </summary>
    /// <param name="xml">XML 字符串。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromXml(string xml)
    {
        return new StringHttpContent(xml, Encoding.UTF8, $"application/xml;charset=UTF-8");
    }

    /// <summary>
    /// 从 HTML 字符串创建 <see cref="StringHttpContent"/> 实例。
    /// </summary>
    /// <param name="html">HTML 字符串。</param>
    /// <param name="encoding">用于编码的字符集。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromHtml(string html, Encoding encoding)
    {
        return new StringHttpContent(html, encoding, $"text/html;charset={encoding.BodyName}");
    }

    /// <summary>
    /// 从 HTML 字符串创建 <see cref="StringHttpContent"/> 实例，使用 UTF-8 编码。
    /// </summary>
    /// <param name="html">HTML 字符串。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromHtml(string html)
    {
        return new StringHttpContent(html, Encoding.UTF8, $"text/html;charset=UTF-8");
    }

    /// <summary>
    /// 从纯文本字符串创建 <see cref="StringHttpContent"/> 实例。
    /// </summary>
    /// <param name="text">纯文本字符串。</param>
    /// <param name="encoding">用于编码的字符集。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromText(string text, Encoding encoding)
    {
        return new StringHttpContent(text, encoding, $"text/plain;charset={encoding.BodyName}");
    }

    /// <summary>
    /// 从纯文本字符串创建 <see cref="StringHttpContent"/> 实例，使用 UTF-8 编码。
    /// </summary>
    /// <param name="text">纯文本字符串。</param>
    /// <returns>返回一个 <see cref="StringHttpContent"/> 实例。</returns>
    public static StringHttpContent FromText(string text)
    {
        return new StringHttpContent(text, Encoding.UTF8, $"text/plain;charset=UTF-8");
    }
}