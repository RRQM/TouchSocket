using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// TouchRpcContainerExtension
    /// </summary>
    public static class TouchRpcContainerExtension
    {
        /// <summary>
        /// 设置文件资源控制器。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="value"></param>
        public static IContainer SetFileResourceController(this IContainer container, IFileResourceController value)
        {
            return container.RegisterSingleton<IFileResourceController>(value);
        }

        /// <summary>
        /// 设置文件资源控制器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        public static IContainer SetFileResourceController<T>(this IContainer container) where T : class, IFileResourceController
        {
            return container.RegisterSingleton<IFileResourceController, T>();
        }

        /// <summary>
        /// 默认的全局资源控制器。
        /// </summary>
        public static readonly FileResourceController FileController=new FileResourceController();

        /// <summary>
        /// 获取文件资源控制器。如果没有注册的话，会新建一个<see cref="FileResourceController"/>
        /// </summary>
        /// <param name="container"></param>
        public static IFileResourceController GetFileResourceController(this IContainer container)
        {
            if (container.IsRegistered(typeof(IFileResourceController)))
            {
                return container.Resolve<IFileResourceController>();
            }
            return FileController;
        }
    }
}
