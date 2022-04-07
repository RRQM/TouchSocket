using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 具有插件功能的对象
    /// </summary>
    public interface IPlguinObject
    {
        /// <summary>
        /// 内置IOC容器
        /// </summary>
        IContainer Container { get; set; }

        /// <summary>
        /// 插件管理器
        /// </summary>
        IPluginsManager PluginsManager { get; set; }

        /// <summary>
        /// 是否已启用插件
        /// </summary>
        bool UsePlugin { get; }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin">插件类型</typeparam>
        /// <returns>插件类型实例</returns>
        TPlugin AddPlugin<TPlugin>() where TPlugin : IPlugin;

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        void AddPlugin(IPlugin plugin);

        /// <summary>
        /// 清空插件
        /// </summary>
        void ClearPlugins();

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="plugin"></param>
        void RemovePlugin(IPlugin plugin);

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RemovePlugin<T>() where T : IPlugin;
    }
}
