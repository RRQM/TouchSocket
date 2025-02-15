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

namespace TouchSocket.Core;

/// <summary>
/// 日志接口
/// </summary>
public interface ILog
{
    /// <summary>
    /// 日志输出级别。
    /// 当<see cref="Log(LogLevel, object, string, Exception)"/>的类型，在该设置之内时，才会真正输出日志。
    /// </summary>
    LogLevel LogLevel { get; set; }

    /// <summary>
    /// 获取或设置日期时间格式
    /// </summary>
    string DateTimeFormat { get; set; }

    /// <summary>
    /// 日志记录
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="source"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    void Log(LogLevel logLevel, object source, string message, Exception exception);
}