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
/// 表示 CoAP 协议相关的异常。
/// </summary>
public class CoAPException : Exception
{
    /// <summary>
    /// 初始化 <see cref="CoAPException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    public CoAPException(string message) : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CoAPException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的内部异常。</param>
    public CoAPException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CoAPException"/> 类的新实例，使用响应码描述错误。
    /// </summary>
    /// <param name="responseCode">CoAP 响应错误码。</param>
    public CoAPException(CoAPResponseCode responseCode)
        : base($"CoAP 错误响应: {responseCode} (0x{(byte)responseCode:X2})")
    {
        this.ResponseCode = responseCode;
    }

    /// <summary>
    /// 获取关联的 CoAP 响应码（如有）。
    /// </summary>
    public CoAPResponseCode? ResponseCode { get; }
}
