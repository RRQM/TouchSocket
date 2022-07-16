using System;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Core.Config
{
    /// <summary>
    /// TouchSocketConfigBuilderExtensions
    /// </summary>
    public static class TouchSocketConfigExtensions
    {
        #region 插件

        /// <summary>
        /// 配置插件。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TouchSocketConfig ConfigurePlugins(this TouchSocketConfig config, Action<IPluginsManager> action)
        {
            action?.Invoke(config.PluginsManager);
            return config;
        }

        #endregion 插件

        #region 容器

        /// <summary>
        /// 配置容器注入。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TouchSocketConfig ConfigureContainer(this TouchSocketConfig config, Action<IContainer> action)
        {
            action?.Invoke(config.Container);
            return config;
        }

        #endregion 容器
    }
}