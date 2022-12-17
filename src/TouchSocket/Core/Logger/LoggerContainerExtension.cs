//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// LoggerContainerExtension
    /// </summary>
    public static class LoggerContainerExtension
    {
        /// <summary>
        /// 设置日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IContainer SetLogger<TLogger>(this IContainer container)
            where TLogger : class, ILog
        {
            container.RegisterTransient<ILog, TLogger>();
            return container;
        }

        /// <summary>
        /// 设置单例日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IContainer SetSingletonLogger<TLogger>(this IContainer container) where TLogger : class, ILog
        {
            container.RegisterSingleton<ILog, TLogger>();
            return container;
        }

        /// <summary>
        /// 设置实例日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="container"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IContainer SetSingletonLogger<TLogger>(this IContainer container, TLogger logger) where TLogger : class, ILog
        {
            container.RegisterSingleton<ILog, TLogger>(logger);
            return container;
        }

        /// <summary>
        /// 使用控制台日志。
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IContainer UseConsoleLogger(this IContainer container)
        {
            container.RegisterSingleton<ILog, ConsoleLogger>();
            return container;
        }

        /// <summary>
        /// 使用委托输出的日志
        /// </summary>
        /// <param name="container"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IContainer UseEasyLogger(this IContainer container, Action<LogType, object, string, System.Exception> action)
        {
            container.RegisterSingleton<ILog>(new EasyLogger(action));
            return container;
        }

        /// <summary>
        /// 使用委托输出的日志
        /// </summary>
        /// <param name="container"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IContainer UseEasyLogger(this IContainer container, Action<string> action)
        {
            container.RegisterSingleton<ILog>(new EasyLogger(action));
            return container;
        }

        /// <summary>
        /// 使用文件日志。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static IContainer UseFileLogger(this IContainer container, string rootPath = null)
        {
            container.RegisterSingleton<ILog>(new FileLogger(rootPath));
            return container;
        }
    }
}