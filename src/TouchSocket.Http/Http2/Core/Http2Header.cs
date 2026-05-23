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

namespace TouchSocket.Http;

/// <summary>
/// HTTP/2 头部字段，包含名称和值。
/// </summary>
internal readonly struct Http2Header
{
    /// <summary>
    /// 初始化 <see cref="Http2Header"/>
    /// </summary>
    public Http2Header(string name, string value)
    {
        this.Name = name;
        this.Value = value;
    }

    /// <summary>
    /// 头部名称
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// 头部值
    /// </summary>
    public readonly string Value;

    /// <summary>
    /// 支持解构模式
    /// </summary>
    public void Deconstruct(out string name, out string value)
    {
        name = this.Name;
        value = this.Value;
    }
}
