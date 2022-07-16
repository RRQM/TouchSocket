using TouchSocket.Core.Log;

namespace TouchSocket.Core.Dependency
{
    /// <summary>
    /// RRQMSocketContainerExtensions
    /// </summary>
    public static class ContainerExtension
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
    }
}
