//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 表示插件管理器。
    /// </summary>
    public class PluginsManager : DisposableObject, IPluginsManager
    {
        private readonly IContainer m_container;
        private readonly object m_locker = new object();
        private readonly Dictionary<string, PluginModel> m_pluginMethods = new Dictionary<string, PluginModel>();
        private readonly List<IPlugin> m_plugins = new List<IPlugin>();

        /// <summary>
        /// 表示插件管理器
        /// </summary>
        /// <param name="container"></param>
        public PluginsManager(IContainer container)
        {
            this.m_container = container;
        }

        /// <inheritdoc/>
        public bool Enable { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IPlugin> Plugins => this.m_plugins;

        void IPluginsManager.Add(IPlugin plugin)
        {
            if (plugin is null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            lock (this.m_locker)
            {
                this.ThrowIfDisposed();

                if (plugin.GetType().GetCustomAttribute<PluginOptionAttribute>() is PluginOptionAttribute optionAttribute)
                {
                    if (optionAttribute.Singleton)
                    {
                        foreach (var item in this.m_plugins)
                        {
                            if (item.GetType().FullName == plugin.GetType().FullName)
                            {
                                throw new Exception($"{plugin.GetType().FullName}类型的插件为单例类型，且已经注册。");
                            }
                        }
                    }
                    if (!optionAttribute.NotRegister)
                    {
                        this.m_container.RegisterSingleton(plugin);
                    }
                }
                else
                {
                    this.m_container.RegisterSingleton(plugin);
                }
                var pluginInterfacetypes = plugin.GetType().GetInterfaces().Where(a => typeof(IPlugin).IsAssignableFrom(a)).ToArray();
                foreach (var type in pluginInterfacetypes)
                {
                    var pairs = new List<string>();

                    var methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    foreach (var methodInfo in methodInfos)
                    {
                        if (methodInfo.GetParameters().Length == 2 && typeof(PluginEventArgs).IsAssignableFrom(methodInfo.GetParameters()[1].ParameterType) && methodInfo.ReturnType == typeof(Task))
                        {
                            if (pairs.Contains(methodInfo.Name))
                            {
                                throw new Exception("插件的接口方法不允许重载");
                            }

                            var pluginModel = this.GetPluginModel(methodInfo);
                            pluginModel.Plugins.Add(plugin);
                            pluginModel.Plugins.Sort(delegate (IPlugin x, IPlugin y)
                            {
                                return x.Order == y.Order ? 0 : x.Order < y.Order ? 1 : -1;
                            });
                        }
                    }
                }
                this.m_plugins.Add(plugin);
            }
        }

        object IPluginsManager.Add(Type pluginType)
        {
            if (pluginType.GetCustomAttribute<PluginOptionAttribute>() is PluginOptionAttribute optionAttribute)
            {
                if (optionAttribute.Singleton)
                {
                    foreach (var item in this.m_plugins)
                    {
                        if (item.GetType().FullName == pluginType.FullName)
                        {
                            return item;
                        }
                    }
                }
            }

            var plugin = (IPlugin)this.m_container.ResolveWithoutRoot(pluginType);
            ((IPluginsManager)this).Add(plugin);
            return plugin;
        }

        void IPluginsManager.Add(string name, Func<object, PluginEventArgs, Task> func)
        {
            lock (this.m_locker)
            {
                var pluginModel = this.GetPluginModel(name);
                pluginModel.Funcs.Add(func);
            }
        }

        bool IPluginsManager.Raise(string name, object sender, PluginEventArgs e)
        {
            if (!this.Enable)
            {
                return false;
            }
            if (this.m_pluginMethods.TryGetValue(name, out var pluginModel))
            {
                e.LoadModel(pluginModel, sender);
                e.InvokeNext().ConfigureAwait(false).GetAwaiter().GetResult();
                return e.Handled;
            }
            return false;
        }

        async Task<bool> IPluginsManager.RaiseAsync(string name, object sender, PluginEventArgs e)
        {
            if (!this.Enable)
            {
                return false;
            }
            if (this.m_pluginMethods.TryGetValue(name, out var pluginModel))
            {
                e.LoadModel(pluginModel, sender);
                await e.InvokeNext();
                return e.Handled;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (this.m_locker)
            {
                foreach (var item in this.m_plugins)
                {
                    item.SafeDispose();
                }
                this.m_plugins.Clear();
            }
            base.Dispose(disposing);
        }

        private PluginModel GetPluginModel(MethodInfo methodInfo)
        {
            if (!this.m_pluginMethods.TryGetValue(methodInfo.Name, out var pluginModel))
            {
                pluginModel = new PluginModel();
                this.m_pluginMethods.Add(methodInfo.Name, pluginModel);
            }
            pluginModel.Method ??= new Method(methodInfo);
            return pluginModel;
        }

        private PluginModel GetPluginModel(string name)
        {
            if (!this.m_pluginMethods.TryGetValue(name, out var pluginModel))
            {
                pluginModel = new PluginModel();
                this.m_pluginMethods.Add(name, pluginModel);
            }
            return pluginModel;
        }
    }
}