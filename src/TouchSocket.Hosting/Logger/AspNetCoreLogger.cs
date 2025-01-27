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

using Microsoft.Extensions.Logging;
using System;
using TouchSocket.Core;
using LogLevel = TouchSocket.Core.LogLevel;

namespace TouchSocket.Hosting;

internal class AspNetCoreLogger : LoggerBase
{
    private readonly ILoggerFactory m_loggerFactory;

    public AspNetCoreLogger(ILoggerFactory loggerFactory)
    {
        this.m_loggerFactory = loggerFactory;
    }

    private ILogger GetLogger(object source)
    {
        source ??= this;
        var type = source.GetType();
        return this.m_loggerFactory.CreateLogger(type);
    }


    protected override void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                this.GetLogger(source).LogTrace(exception, message);
                break;
            case LogLevel.Debug:
                this.GetLogger(source).LogDebug(exception, message);
                break;
            case LogLevel.Info:
                this.GetLogger(source).LogInformation(exception, message);
                break;
            case LogLevel.Warning:
                this.GetLogger(source).LogWarning(exception, message);
                break;
            case LogLevel.Error:
                this.GetLogger(source).LogError(exception, message);
                break;
            case LogLevel.Critical:
                this.GetLogger(source).LogCritical(exception, message);
                break;
            case LogLevel.None:
            default:
                break;
        }
    }
}