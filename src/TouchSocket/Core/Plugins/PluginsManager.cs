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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Reflection;

namespace TouchSocket.Core.Plugins
{
    /// <summary>
    /// 表示插件管理器。
    /// </summary>
    public class PluginsManager : IPluginsManager
    {
        private readonly Dictionary<Type, Dictionary<string, PluginMethod[]>> m_pluginInfoes = new Dictionary<Type, Dictionary<string, PluginMethod[]>>();
        private readonly List<PluginModel> m_plugins = new List<PluginModel>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="container"></param>
        public PluginsManager(IContainer container)
        {
            this.Container = container;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        void IPluginsManager.Add(IPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException();
            }
            if (plugin.GetType().GetCustomAttribute<SingletonPluginAttribute>() is SingletonPluginAttribute singletonPlugin)
            {
                foreach (var item in this.m_plugins)
                {
                    if (item.PluginType == plugin.GetType())
                    {
                        ((IPluginsManager)this).Remove(item.PluginType);
                        break;
                    }
                }
            }
            this.m_plugins.Add(new PluginModel(plugin, plugin.GetType()));
            var types = plugin.GetType().GetInterfaces().Where(a => typeof(IPlugin).IsAssignableFrom(a)).ToArray();
            foreach (var type in types)
            {
                if (!this.m_pluginInfoes.ContainsKey(type))
                {
                    Dictionary<string, PluginMethod[]> pairs = new Dictionary<string, PluginMethod[]>();
                    var ms = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (var item in ms)
                    {
                        if (item.GetParameters().Length == 2 && typeof(TouchSocketEventArgs).IsAssignableFrom(item.GetParameters()[1].ParameterType))
                        {
                            if (pairs.ContainsKey(item.Name))
                            {
                                throw new Exception("插件的接口方法不允许重载");
                            }
                            List<PluginMethod> methods = new List<PluginMethod>();
                            if (item.GetCustomAttribute<AsyncRaiserAttribute>() != null)
                            {
                                var asyncMethod = type.GetMethod($"{item.Name}Async", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                                if (asyncMethod == null)
                                {
                                    throw new Exception("当接口标识为异步时，还应当定义其异步方法，以“Async”结尾");
                                }

                                if (asyncMethod.GetParameters().Length != 2 && typeof(TouchSocketEventArgs).IsAssignableFrom(asyncMethod.GetParameters()[1].ParameterType))
                                {
                                    throw new Exception("异步接口方法不符合设定");
                                }
                                if (asyncMethod.ReturnType != typeof(Task))
                                {
                                    throw new Exception("异步接口方法返回值必须为Task。");
                                }
                                methods.Add(new PluginMethod(asyncMethod, type));
                            }
                            methods.Add(new PluginMethod(item, type));
                            pairs.Add(item.Name, methods.ToArray());
                        }
                    }
                    this.m_pluginInfoes.Add(type, pairs);
                }
            }

            this.m_plugins.Sort(delegate (PluginModel x, PluginModel y)
            {
                if (x.Plugin.Order == y.Plugin.Order) return 0;
                else if (x.Plugin.Order < y.Plugin.Order) return 1;
                else return -1;
            });

            this.Container.RegisterSingleton(plugin);
        }

        /// <summary>
        /// 清除所有插件
        /// </summary>
        void IPluginsManager.Clear()
        {
            foreach (var item in this.m_plugins)
            {
                item.Plugin.SafeDispose();
            }
            this.m_plugins.Clear();
        }

        IEnumerator<IPlugin> IEnumerable<IPlugin>.GetEnumerator()
        {
            return this.m_plugins.Select(a => a.Plugin).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_plugins.Select(a => a.Plugin).GetEnumerator();
        }

        /// <summary>
        /// 触发对应方法
        /// </summary>
        /// <typeparam name="TPlugin">接口类型，此处也必须是接口类型</typeparam>
        /// <param name="name">触发名称</param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        bool IPluginsManager.Raise<TPlugin>(string name, object sender, TouchSocketEventArgs e)
        {
            if (!this.Enable)
            {
                return false;
            }
            if (this.m_pluginInfoes.TryGetValue(typeof(TPlugin), out var value))
            {
                if (value.TryGetValue(name, out PluginMethod[] pluginMethods))
                {
                    for (int i = 0; i < this.m_plugins.Count; i++)
                    {
                        if (e.Handled)
                        {
                            return true;
                        }
                        if (pluginMethods[0].type.IsAssignableFrom(this.m_plugins[i].PluginType))
                        {
                            for (int j = 0; j < pluginMethods.Length; j++)
                            {
                                try
                                {
                                    pluginMethods[j].Invoke(this.m_plugins[i].Plugin, sender, e);
                                }
                                catch (Exception ex)
                                {
                                    this.Container.Resolve<ILog>().Exception(ex);
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="plugin"></param>
        void IPluginsManager.Remove(IPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException();
            }
            foreach (var item in this.m_plugins)
            {
                if (plugin == item.Plugin)
                {
                    if (this.m_plugins.Remove(item))
                    {
                        plugin.SafeDispose();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="type"></param>
        void IPluginsManager.Remove(Type type)
        {
            for (int i = this.m_plugins.Count - 1; i >= 0; i--)
            {
                IPlugin plugin = this.m_plugins[i].Plugin;
                if (plugin.GetType() == type)
                {
                    this.m_plugins.RemoveAt(i);
                    plugin.SafeDispose();
                }
            }
        }
    }

    internal class PluginMethod : Method
    {
        public readonly Type type;

        public PluginMethod(MethodInfo method, Type type) : base(method)
        {
            this.type = type;
        }
    }

    internal class PluginModel
    {
        public PluginModel(IPlugin plugin, Type pluginType)
        {
            this.Plugin = plugin;
            this.PluginType = pluginType;
        }

        public IPlugin Plugin { get; set; }
        public Type PluginType { get; set; }
    }
}