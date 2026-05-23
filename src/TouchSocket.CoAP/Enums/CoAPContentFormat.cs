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

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 内容格式标识（RFC 7252 Section 12.3）
/// </summary>
public enum CoAPContentFormat : ushort
{
    /// <summary>
    /// text/plain; charset=utf-8，内容格式 0。
    /// </summary>
    TextPlain = 0,

    /// <summary>
    /// application/link-format，CoAP 资源目录格式，内容格式 40。
    /// </summary>
    ApplicationLinkFormat = 40,

    /// <summary>
    /// application/xml，内容格式 41。
    /// </summary>
    ApplicationXml = 41,

    /// <summary>
    /// application/octet-stream，二进制流，内容格式 42。
    /// </summary>
    ApplicationOctetStream = 42,

    /// <summary>
    /// application/exi，EXI 格式，内容格式 47。
    /// </summary>
    ApplicationExi = 47,

    /// <summary>
    /// application/json，JSON 格式，内容格式 50。
    /// </summary>
    ApplicationJson = 50,

    /// <summary>
    /// application/cbor，CBOR 格式，内容格式 60。
    /// </summary>
    ApplicationCbor = 60,
}
