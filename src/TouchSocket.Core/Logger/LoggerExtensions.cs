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

namespace TouchSocket.Core;

/// <summary>
/// 日志扩展方法
/// </summary>
public static class LoggerExtensions
{
    #region LoggerGroup 日志

    /// <summary>
    /// 在指定类型的日志记录器中记录严重级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="msg">日志消息</param>
    public static void Critical<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Critical, null, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录严重级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Critical<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Critical, source, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录调试级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="msg">日志消息</param>
    public static void Debug<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Debug, null, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录调试级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Debug<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Debug, source, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录错误级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="msg">日志消息</param>
    public static void Error<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, null, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录错误级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Error<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, source, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录异常日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="ex">异常实例</param>
    public static void Exception<TLog>(this ILog logger, Exception ex) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, null, ex.Message, ex);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录异常日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="ex">异常实例</param>
    public static void Exception<TLog>(this ILog logger, object source, Exception ex) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, source, ex.Message, ex);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录信息级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="msg">日志消息</param>
    public static void Info<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Info, null, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录信息级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Info<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Info, source, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="logLevel">日志级别</param>
    /// <param name="source">日志来源</param>
    /// <param name="message">日志消息</param>
    /// <param name="exception">异常实例</param>
    public static void Log<TLog>(this ILog logger, LogLevel logLevel, object source, string message, Exception exception) where TLog : ILog
    {
        if (logger is LoggerGroup loggerGroup)
        {
            loggerGroup.Log<TLog>(logLevel, source, message, exception);
        }
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录跟踪级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="msg">日志消息</param>
    public static void Trace<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Trace, null, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录跟踪级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Trace<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Trace, source, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录警告级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="msg">日志消息</param>
    public static void Warning<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Warning, null, msg, null);
    }

    /// <summary>
    /// 在指定类型的日志记录器中记录警告级别日志
    /// </summary>
    /// <typeparam name="TLog">日志记录器类型</typeparam>
    /// <param name="logger">日志组实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Warning<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Warning, source, msg, null);
    }

    #endregion

    #region 普通日志

    /// <summary>
    /// 记录严重级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="msg">日志消息</param>
    public static void Critical(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Critical, null, msg, null);
    }

    /// <summary>
    /// 记录严重级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Critical(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Critical, source, msg, null);
    }

    /// <summary>
    /// 记录调试级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="msg">日志消息</param>
    public static void Debug(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Debug, null, msg, null);
    }

    /// <summary>
    /// 记录调试级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Debug(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Debug, source, msg, null);
    }


    /// <summary>
    /// 记录调试级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="ex">由异常来的日志消息</param>
    public static void Debug(this ILog logger, object source, Exception ex)
    {
        logger.Log(LogLevel.Debug, source, ex.Message, ex);
    }

    /// <summary>
    /// 记录错误级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="msg">日志消息</param>
    public static void Error(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Error, null, msg, null);
    }

    /// <summary>
    /// 记录错误级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Error(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Error, source, msg, null);
    }

    /// <summary>
    /// 记录异常日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="ex">异常实例</param>
    public static void Exception(this ILog logger, Exception ex)
    {
        logger.Log(LogLevel.Error, null, ex.Message, ex);
    }

    /// <summary>
    /// 记录异常日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="ex">异常实例</param>
    public static void Exception(this ILog logger, object source, Exception ex)
    {
        logger.Log(LogLevel.Error, source, ex.Message, ex);
    }

    /// <summary>
    /// 记录异常日志（包含自定义消息）
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">自定义消息</param>
    /// <param name="ex">异常实例</param>
    public static void Exception(this ILog logger, object source, string msg, Exception ex)
    {
        logger.Log(LogLevel.Error, source, msg, ex);
    }

    /// <summary>
    /// 记录信息级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="msg">日志消息</param>
    public static void Info(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Info, null, msg, null);
    }

    /// <summary>
    /// 记录信息级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Info(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Info, source, msg, null);
    }

    /// <summary>
    /// 记录跟踪级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="msg">日志消息</param>
    public static void Trace(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Trace, null, msg, null);
    }

    /// <summary>
    /// 记录跟踪级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Trace(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Trace, source, msg, null);
    }

    /// <summary>
    /// 记录警告级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="msg">日志消息</param>
    public static void Warning(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Warning, null, msg, null);
    }

    /// <summary>
    /// 记录警告级别日志
    /// </summary>
    /// <param name="logger">日志实例</param>
    /// <param name="source">日志来源</param>
    /// <param name="msg">日志消息</param>
    public static void Warning(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Warning, source, msg, null);
    }

    #endregion
}