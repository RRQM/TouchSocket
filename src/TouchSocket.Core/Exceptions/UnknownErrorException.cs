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
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 未知错误异常类，继承自Exception，用于处理未知类型的错误。
/// </summary>
public sealed class UnknownErrorException : Exception
{
    /// <summary>
    /// 默认构造函数，当不指定错误消息时，默认使用资源文件中定义的未知错误消息。
    /// </summary>
    public UnknownErrorException() : this(TouchSocketCoreResource.UnknownError)
    {
    }

    /// <summary>
    /// 构造函数，允许指定自定义的错误消息。
    /// </summary>
    /// <param name="message">发生的未知错误的详细信息。</param>
    public UnknownErrorException(string message) : base(message)
    {
    }
}