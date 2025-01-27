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
    public StringHttpContent(string content, Encoding encoding) : base(encoding.GetBytes(content))
    {
        // 构造函数将字符串内容和编码方式作为参数，将字符串内容转换为字节数组后传递给基类。
    }
}
