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
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Reflection;

namespace TouchSocket.Core.Plugins
{
    /// <summary>
    /// 表示插件管理器。
    /// </summary>
    public class PluginsManager : IPluginsManager
    {
        private static readonly Dictionary<Type, Dictionary<string, PluginMethod>> m_pluginInfoes = new Dictionary<Type, Dictionary<string, PluginMethod>>();
        private readonly List<IPlugin> m_plugins = new List<IPlugin>();

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
                    if (item.GetType() == plugin.GetType())
                    {
                        ((IPluginsManager)this).Remove(item);
                        break;
                    }
                }
            }
            this.m_plugins.Add(plugin);
            var types = plugin.GetType().GetInterfaces().Where(a => typeof(IPlugin).IsAssignableFrom(a)).ToArray();
            foreach (var type in types)
            {
                if (!m_pluginInfoes.ContainsKey(type))
                {
                    Dictionary<string, PluginMethod> pairs = new Dictionary<string, PluginMethod>();
                    var ms = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (var item in ms)
                    {
                        if (item.GetParameters().Length == 2 && typeof(TouchSocketEventArgs).IsAssignableFrom(item.GetParameters()[1].ParameterType))
                        {
                            try
                            {
                                pairs.Add(item.Name, new PluginMethod(item, type));
                            }
                            catch
                            {
                                throw new Exception("插件的接口方法不允许重载");
                            }
                        }
                    }
                    m_pluginInfoes.Add(type, pairs);
                }
            }

            this.m_plugins.Sort(delegate (IPlugin x, IPlugin y)
            {
                if (x.Order == y.Order) return 0;
                else if (x.Order < y.Order) return 1;
                else return -1;
            });
        }

        /// <summary>
        /// 清除所有插件
        /// </summary>
        void IPluginsManager.Clear()
        {
            foreach (var item in this.m_plugins)
            {
                item.Dispose();
            }
            this.m_plugins.Clear();
        }

        IEnumerator<IPlugin> IEnumerable<IPlugin>.GetEnumerator()
        {
            return this.m_plugins.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_plugins.GetEnumerator();
        }

        /// <summary>
        /// 触发对应方法
        /// </summary>
        /// <typeparam name="TPlugin">接口类型，此处也必须是接口类型</typeparam>
        /// <param name="name">触发名称</param>
        /// <param name="params">参数</param>
        bool IPluginsManager.Raise<TPlugin>(string name, params object[] @params)
        {
            if (m_pluginInfoes.TryGetValue(typeof(TPlugin), out var value) && (@params.Length == 2) && (@params[1] is TouchSocketEventArgs args))
            {
                if (value.TryGetValue(name, out PluginMethod pluginMethod))
                {
                    for (int i = 0; i < this.m_plugins.Count; i++)
                    {
                        if (args.Handled)
                        {
                            return true;
                        }
                        if (pluginMethod.type.IsAssignableFrom(this.m_plugins[i].GetType()))
                        {
                            try
                            {
                                pluginMethod.Invoke(this.m_plugins[i], @params);
                            }
                            catch
                            {
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
            if (this.m_plugins.Remove(plugin))
            {
                plugin.Dispose();
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
                IPlugin plugin = this.m_plugins[i];
                if (plugin.GetType() == type)
                {
                    this.m_plugins.RemoveAt(i);
                    plugin.Dispose();
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
}