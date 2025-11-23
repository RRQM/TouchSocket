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

using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Core;

/// <summary>
/// 日志记录器容器扩展类
/// </summary>
public static class LoggerContainerExtension
{
    #region ConsoleLogger

    /// <summary>
    /// 添加控制台日志到日志组。
    /// </summary>
    /// <returns></returns>
    public static void AddConsoleLogger(this LoggerGroup loggerGroup, LogLevel logLevel = LogLevel.Info)
    {
        ConsoleLogger.Default.LogLevel = logLevel;
        loggerGroup.AddLogger(ConsoleLogger.Default);
    }

    /// <summary>
    /// 为注册器容器添加控制台日志记录器的扩展方法。
    /// </summary>
    /// <param name="container">要添加控制台日志记录器的注册器。</param>
    /// <param name="logLevel"></param>
    /// <returns>添加了控制台日志记录器的注册器。</returns>
    public static IRegistrator AddConsoleLogger(this IRegistrator container, LogLevel logLevel = LogLevel.Info)
    {
        ConsoleLogger.Default.LogLevel = logLevel;
        // 向注册器中添加控制台日志记录器实例
        AddLogger(container, ConsoleLogger.Default);
        // 返回注册器，允许链式调用
        return container;
    }

    #endregion ConsoleLogger

    #region EasyLogger

    /// <summary>
    /// 添加委托日志到日志组。
    /// </summary>
    /// <param name="loggerGroup">日志组对象，表示要添加委托日志到哪个日志组。</param>
    /// <param name="action">一个委托，表示日志记录的行动，包含日志级别、日志信息、标签以及异常信息。</param>
    /// <param name="logLevel"></param>
    /// <returns>该方法没有返回值。</returns>
    public static void AddEasyLogger(this LoggerGroup loggerGroup, Action<LogLevel, object, string, Exception> action, LogLevel logLevel = LogLevel.Info)
    {
        // 创建一个EasyLogger实例，传入日志记录的委托方法
        var easyLogger = new EasyLogger(action)
        {
            LogLevel = logLevel
        };

        // 将创建的EasyLogger实例添加到loggerGroup中
        loggerGroup.AddLogger(easyLogger);
    }

    /// <summary>
    /// 添加委托日志到日志组。
    /// </summary>
    /// <param name="loggerGroup">日志组对象，表示要向其添加新日志器的组。</param>
    /// <param name="action">一个委托，定义了日志记录的方式。</param>
    /// <param name="logLevel"></param>
    public static void AddEasyLogger(this LoggerGroup loggerGroup, Action<string> action, LogLevel logLevel = LogLevel.Info)
    {
        // 创建一个使用提供的日志委托的EasyLogger实例
        var easyLogger = new EasyLogger(action)
        {
            LogLevel = logLevel
        };

        // 将新创建的EasyLogger实例添加到日志组中
        loggerGroup.AddLogger(easyLogger);
    }

    /// <summary>
    /// 为注册容器添加一个简易的日志记录器。
    /// </summary>
    /// <param name="container">日志记录器需要注册到的容器。</param>
    /// <param name="action">一个委托动作，当记录日志时将被执行。它接收日志级别、日志消息、发生日志的源和任何异常信息作为参数。</param>
    /// <param name="logLevel"></param>
    /// <returns>返回注册容器，以便进行链式调用。</returns>
    public static IRegistrator AddEasyLogger(this IRegistrator container, Action<LogLevel, object, string, Exception> action, LogLevel logLevel = LogLevel.Info)
    {
        // 创建并添加EasyLogger实例到容器中。
        AddLogger(container, new EasyLogger(action) { LogLevel = logLevel });
        // 返回注册容器以支持链式调用。
        return container;
    }

    /// <summary>
    /// 向注册容器中添加一个简易的日志记录器
    /// </summary>
    /// <param name="container">要添加日志记录器的注册容器</param>
    /// <param name="action">一个委托，定义了如何处理日志消息</param>
    /// <param name="logLevel"></param>
    /// <returns>返回修改后的注册容器，支持链式调用</returns>
    public static IRegistrator AddEasyLogger(this IRegistrator container, Action<string> action, LogLevel logLevel = LogLevel.Info)
    {
        AddLogger(container, new EasyLogger(action) { LogLevel = logLevel }); // 创建并添加EasyLogger实例到注册容器中
        return container; // 返回注册容器以支持链式调用
    }

    #endregion EasyLogger

    #region FileLogger

    /// <summary>
    /// 添加文件日志到日志组。
    /// </summary>
    /// <param name="loggerGroup">要添加文件日志的LoggerGroup实例。</param>
    /// <param name="rootPath">日志文件的根路径，默认为"logs"。</param>
    /// <param name="logLevel"></param>
    /// <returns>此方法不返回任何值。</returns>
    /// <remarks>
    /// 该方法扩展了LoggerGroup类的功能，允许轻松添加文件日志记录器。
    /// 文件日志记录器会根据当前日期在指定的根路径下创建日志文件夹。
    /// </remarks>
    public static void AddFileLogger(this LoggerGroup loggerGroup, string rootPath = "logs", LogLevel logLevel = LogLevel.Info)
    {
        // 创建并添加一个FileLogger实例到loggerGroup。
        // 指定了日志文件夹的创建路径规则，根据当前日期在根路径下创建日志文件夹。
        loggerGroup.AddLogger(new FileLogger()
        {
            LogLevel = logLevel,
            CreateLogFolder = l => Path.Combine(rootPath, DateTime.Now.ToString("[yyyy-MM-dd]"))
        });
    }

    /// <summary>
    /// 为 LoggerGroup 实例添加 FileLogger 功能的扩展方法。
    /// </summary>
    /// <param name="loggerGroup">要添加 FileLogger 的 LoggerGroup 实例。</param>
    /// <param name="action">一个包含 FileLogger 实例操作的委托，允许设置 FileLogger。</param>
    /// <remarks>
    /// 此方法通过创建一个新的 FileLogger 实例，应用提供的配置操作，然后将配置好的 FileLogger 添加到 LoggerGroup 中。
    /// 它提供了一种简洁的方式，将 FileLogger 集成到现有的 LoggerGroup 结构中，增强了日志记录能力。
    /// </remarks>
    public static void AddFileLogger(this LoggerGroup loggerGroup, Action<FileLogger> action)
    {
        // 创建 FileLogger 实例
        var logger = new FileLogger();

        // 应用配置操作以设置 FileLogger
        action(logger);

        // 将配置好的 FileLogger 添加到 LoggerGroup 中
        loggerGroup.AddLogger(logger);
    }

    /// <summary>
    /// 为注册器添加文件日志记录器扩展方法。
    /// </summary>
    /// <param name="container">要添加文件日志记录器的注册器。</param>
    /// <param name="rootPath">日志文件的基础路径，默认为"logs"。</param>
    /// <param name="logLevel"></param>
    /// <returns>添加了文件日志记录器后的注册器。</returns>
    public static IRegistrator AddFileLogger(this IRegistrator container, string rootPath = "logs", LogLevel logLevel = LogLevel.Info)
    {
        // 创建一个FileLogger实例，并为其指定日志文件夹创建的逻辑
        AddLogger(container, new FileLogger()
        {
            // 指定日志文件的路径为基于当前日期的文件夹
            CreateLogFolder = l => Path.Combine(rootPath, DateTime.Now.ToString("[yyyy-MM-dd]")),
            LogLevel = logLevel
        });
        return container;
    }

    /// <summary>
    /// 为注册器添加文件日志记录器的扩展方法。
    /// </summary>
    /// <param name="container">要添加文件日志记录器的注册器。</param>
    /// <param name="action">一个接受 FileLogger 实例作为参数的委托，用于配置文件日志记录器。</param>
    /// <returns>返回添加了文件日志记录器后的注册器，以便进行链式调用。</returns>
    public static IRegistrator AddFileLogger(this IRegistrator container, Action<FileLogger> action)
    {
        // 创建一个新的 FileLogger 实例。
        var logger = new FileLogger();
        // 使用提供的 Action 委托配置 FileLogger 实例。
        action(logger);
        // 将配置好的 FileLogger 实例添加到注册器中。
        AddLogger(container, logger);
        // 返回注册器实例，支持链式调用。
        return container;
    }

    #endregion FileLogger

    /// <summary>
    /// 添加单例日志到容器。
    /// </summary>
    /// <param name="registrator">要添加日志单例的注册器。</param>
    /// <param name="logger">要添加到容器的日志对象。</param>
    /// <returns>返回更新后的注册器对象，以便进行链式调用。</returns>
    public static IRegistrator AddLogger<[DynamicallyAccessedMembers(AOT.Container)] TLog>(this IRegistrator registrator, TLog logger) where TLog : class, ILog
    {
        // 将日志对象作为单例注册到容器中
        registrator.RegisterSingleton<ILog, TLog>(logger);
        // 返回注册器对象，支持链式调用
        return registrator;
    }

    /// <summary>
    /// 添加单例日志到容器
    /// </summary>
    /// <param name="registrator">要扩展的注册器接口</param>
    /// <param name="loggerAction">用于配置LoggerGroup的委托</param>
    /// <returns>返回被扩展的注册器接口</returns>
    public static IRegistrator AddLogger(this IRegistrator registrator, Action<LoggerGroup> loggerAction)
    {
        // 创建一个LoggerGroup实例
        var loggerGroup = new LoggerGroup();
        // 调用传入的委托来配置LoggerGroup
        loggerAction.Invoke(loggerGroup);
        // 将配置好的LoggerGroup注册为单例
        registrator.RegisterSingleton<ILog, LoggerGroup>(loggerGroup);
        // 返回注册器接口
        return registrator;
    }
}