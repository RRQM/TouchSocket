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
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// TouchSocketHttp辅助工具类
/// </summary>
public static class TouchSocketHttpUtility
{
    /// <summary>
    /// 非缓存上限
    /// </summary>
    public const int NoCacheMaxSize = 1024 * 1024;

    /// <summary>
    /// 获取一个只读的字节序列，表示回车换行(CRLF)。
    /// </summary>
    /// <value>
    /// 一个包含回车和换行字节的只读字节序列。
    /// </value>
    public static ReadOnlySpan<byte> CRLF => new byte[] { (byte)'\r', (byte)'\n' };

    #region ByteBlock Append

    public static void AppendAnd<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write("&"u8);
    }

    public static void AppendColon<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write(":"u8);
    }

    public static void AppendEqual<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write("="u8);
    }

    public static void AppendHTTP<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write("HTTP"u8);
    }

    public static void AppendQuestionMark<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write("?"u8);
    }

    public static void AppendRn<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write("\r\n"u8);
    }

    public static void AppendSlash<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write("/"u8);
    }

    public static void AppendSpace<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write(StringExtension.DefaultSpaceUtf8Span);
    }
    
    public static void AppendUtf8String<TByteBlock>(ref TByteBlock byteBlock,string value) where TByteBlock : IByteBlock
    {
        byteBlock.WriteNormalString(value,Encoding.UTF8);
    }

    public static void AppendHex<TByteBlock>(ref TByteBlock byteBlock, int value) where TByteBlock : IByteBlock
    {
        AppendUtf8String(ref byteBlock,$"{value:X}");
    }
    #endregion ByteBlock Append
}