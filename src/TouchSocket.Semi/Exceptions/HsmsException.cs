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

namespace TouchSocket.Semi;

/// <summary>
/// 表示 HSMS 协议操作期间引发的异常。
/// </summary>
public class HsmsException : Exception
{
    /// <summary>
    /// 初始化 <see cref="HsmsException"/> 的新实例。
    /// </summary>
    public HsmsException() { }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="HsmsException"/> 的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    public HsmsException(string message) : base(message) { }

    /// <summary>
    /// 使用指定的错误消息和对导致此异常的内部异常的引用来初始化 <see cref="HsmsException"/> 的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的异常。</param>
    public HsmsException(string message, Exception innerException) : base(message, innerException) { }
}
