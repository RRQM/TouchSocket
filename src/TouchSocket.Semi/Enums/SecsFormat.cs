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

namespace TouchSocket.Semi;

/// <summary>
/// 表示 SECS-II 数据项的格式类型。
/// </summary>
public enum SecsFormat : byte
{
    /// <summary>
    /// 列表格式（List）。
    /// </summary>
    List = 0,

    /// <summary>
    /// 二进制格式（Binary）。
    /// </summary>
    Binary = 8,

    /// <summary>
    /// 布尔格式（Boolean）。
    /// </summary>
    Boolean = 9,

    /// <summary>
    /// ASCII 字符格式。
    /// </summary>
    ASCII = 16,

    /// <summary>
    /// JIS8 字符格式。
    /// </summary>
    JIS8 = 17,

    /// <summary>
    /// 8 字节有符号整数格式（Int64）。
    /// </summary>
    I8 = 24,

    /// <summary>
    /// 1 字节有符号整数格式（SByte）。
    /// </summary>
    I1 = 25,

    /// <summary>
    /// 2 字节有符号整数格式（Int16）。
    /// </summary>
    I2 = 26,

    /// <summary>
    /// 4 字节有符号整数格式（Int32）。
    /// </summary>
    I4 = 28,

    /// <summary>
    /// 8 字节浮点数格式（Double）。
    /// </summary>
    F8 = 32,

    /// <summary>
    /// 4 字节浮点数格式（Single）。
    /// </summary>
    F4 = 36,

    /// <summary>
    /// 8 字节无符号整数格式（UInt64）。
    /// </summary>
    U8 = 40,

    /// <summary>
    /// 1 字节无符号整数格式（Byte）。
    /// </summary>
    U1 = 41,

    /// <summary>
    /// 2 字节无符号整数格式（UInt16）。
    /// </summary>
    U2 = 42,

    /// <summary>
    /// 4 字节无符号整数格式（UInt32）。
    /// </summary>
    U4 = 44
}
