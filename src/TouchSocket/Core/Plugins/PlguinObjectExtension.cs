using System;
using TouchSocket.Core.Dependency;

namespace TouchSocket.Core.Plugins
{
    /// <summary>
    /// PlguinObjectExtension
    /// </summary>
    public static class PlguinObjectExtension
    {
        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin">插件类型</typeparam>
        /// <returns>插件类型实例</returns>
        public static TPlugin AddPlugin<TPlugin>(this IPlguinObject plguinObject) where TPlugin : class, IPlugin
        {
            var obj = plguinObject.Container.Resolve<TPlugin>();
            AddPlugin(plguinObject, obj);
            return obj;
        }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plguinObject"></param>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddPlugin(this IPlguinObject plguinObject, IPlugin plugin)
        {
            plguinObject.PluginsManager.Add(plugin);
        }

        /// <summary>
        /// 清空插件
        /// </summary>
        public static void ClearPlugins(this IPlguinObject plguinObject)
        {
            plguinObject.PluginsManager.Clear();
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="plguinObject"></param>
        /// <param name="plugin"></param>
        public static void RemovePlugin(this IPlguinObject plguinObject, IPlugin plugin)
        {
            plguinObject.PluginsManager.Remove(plugin);
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plguinObject"></param>
        public static void RemovePlugin<T>(this IPlguinObject plguinObject) where T : IPlugin
        {
            plguinObject.PluginsManager.Remove(typeof(T));
        }
    }
}