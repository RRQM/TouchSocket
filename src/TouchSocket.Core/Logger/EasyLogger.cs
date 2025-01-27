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
/// 一个简单的委托日志
/// </summary>
public class EasyLogger : LoggerBase
{
    private readonly Action<LogLevel, object, string, Exception> m_action;

    /// <summary>
    /// 一个简单的委托日志
    /// </summary>
    /// <param name="action">参数依次为：日志类型，触发源，消息，异常</param>
    public EasyLogger(Action<LogLevel, object, string, Exception> action)
    {
        this.m_action = action;
    }

    /// <summary>
    /// 一个简单的委托日志
    /// </summary>
    /// <param name="action">参数为日志消息输出。</param>
    public EasyLogger(Action<string> action)
    {
        void localAction(LogLevel logLevel, object source, string message, Exception exception)
        {
            action.Invoke(this.CreateLogString(logLevel, source, message, exception));
        }
        this.m_action = localAction;
    }

    /// <inheritdoc/>
    /// <param name="logLevel"></param>
    /// <param name="source"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    protected override void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
    {
        try
        {
            this.m_action?.Invoke(logLevel, source, message, exception);
        }
        catch
        {
        }
    }
}