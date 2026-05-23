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

using System.Runtime.CompilerServices;

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 协议专用异常抛出帮助类。
/// </summary>
internal static class CoAPThrowHelper
{
    /// <summary>
    /// 若数据长度不足以构成合法 CoAP 消息头（最少 4 字节），则抛出 <see cref="CoAPException"/>。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfInvalidCoAPMessageLength(int length)
    {
        if (length < CoAPConstant.MinMessageLength)
        {
            ThrowInvalidCoAPMessageLength(length);
        }
    }

    /// <summary>
    /// 若 CoAP 版本号不为 1，则抛出 <see cref="CoAPException"/>。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfInvalidCoAPVersion(int version)
    {
        if (version != CoAPConstant.Version)
        {
            ThrowInvalidCoAPVersion(version);
        }
    }

    /// <summary>
    /// 若 Token 长度超出合法范围（0-8），则抛出 <see cref="CoAPException"/>。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfInvalidTokenLength(int tokenLength)
    {
        if (tokenLength > 8)
        {
            ThrowInvalidTokenLength(tokenLength);
        }
    }

    /// <summary>
    /// 若数据长度小于所需最小长度，则抛出 <see cref="CoAPException"/>。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfDataTooShort(int actualLength, int requiredLength)
    {
        if (actualLength < requiredLength)
        {
            ThrowDataTooShort(actualLength, requiredLength);
        }
    }

    /// <summary>
    /// 若响应码为 4.xx 客户端错误或 5.xx 服务器错误，则抛出 <see cref="CoAPException"/>。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfErrorResponse(CoAPResponseCode responseCode)
    {
        var codeClass = ((byte)responseCode >> 5) & 0x07;
        if (codeClass == 4 || codeClass == 5)
        {
            ThrowErrorResponse(responseCode);
        }
    }

    private static void ThrowDataTooShort(int actual, int required)
    {
        throw new CoAPException($"CoAP 数据长度不足：实际 {actual} 字节，需要至少 {required} 字节。");
    }

    private static void ThrowErrorResponse(CoAPResponseCode responseCode)
    {
        throw new CoAPException(responseCode);
    }

    private static void ThrowInvalidCoAPMessageLength(int length)
    {
        throw new CoAPException($"无效的 CoAP 消息长度：{length}，最少需要 {CoAPConstant.MinMessageLength} 字节。");
    }

    private static void ThrowInvalidCoAPVersion(int version)
    {
        throw new CoAPException($"不支持的 CoAP 版本：{version}，当前仅支持版本 {CoAPConstant.Version}。");
    }

    private static void ThrowInvalidTokenLength(int tokenLength)
    {
        throw new CoAPException($"Token 长度超出合法范围：{tokenLength}，合法范围为 0-8。");
    }
}
