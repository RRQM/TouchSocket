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
    public class PluginManager : DisposableObject, IPluginManager
    {
        private readonly object m_locker = new object();
        private readonly Dictionary<Type, PluginModel> m_pluginMethods = new Dictionary<Type, PluginModel>();

        private readonly List<IPlugin> m_plugins = new List<IPlugin>();
        private readonly IResolver m_resolver;

        /// <summary>
        /// 表示插件管理器
        /// </summary>
        /// <param name="resolver"></param>
        public PluginManager(IResolver resolver)
        {
            this.m_resolver = resolver;
        }

        /// <inheritdoc/>
        public bool Enable { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IPlugin> Plugins => this.m_plugins;

        IResolver IResolverObject.Resolver => this.m_resolver;

        void IPluginManager.Add(IPlugin plugin)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(plugin,nameof(plugin));

            this.ThrowIfDisposed();

            lock (this.m_locker)
            {
                if (plugin.GetType().GetCustomAttribute<PluginOptionAttribute>() is PluginOptionAttribute optionAttribute)
                {
                    if (optionAttribute.Singleton)
                    {
                        foreach (var item in this.Plugins)
                        {
                            if (item.GetType() == plugin.GetType())
                            {
                                //throw new Exception($"{plugin.GetType().FullName}类型的插件为单例类型，且已经注册。");

                                return;
                            }
                        }
                    }
                }

                var list = PluginManager.SearchPluginMethod(plugin);

                foreach (var item in list)
                {
                    var pluginModel = this.GetPluginModel(item);

                    var methodInfo = item.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(a =>
                        {
                            return a.GetParameters().Length == 2 && typeof(PluginEventArgs).IsAssignableFrom(a.GetParameters()[1].ParameterType) && a.ReturnType == typeof(Task);
                        })
                        .FirstOrDefault();

                    if (methodInfo != null)
                    {
                        var pluginEntity = new PluginEntity(new Method(methodInfo), plugin);
                        pluginModel.Add(pluginEntity.Run);
                    }
                }
                plugin.Loaded(this);
                this.m_plugins.Add(plugin);

                //var pairs = new List<string>();
                //var methodInfos = plugin.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                //foreach (var methodInfo in methodInfos)
                //{
                //    if (methodInfo.GetParameters().Length == 2 && typeof(PluginEventArgs).IsAssignableFrom(methodInfo.GetParameters()[1].ParameterType) && methodInfo.ReturnType == typeof(Task))
                //    {
                //        var name = methodInfo.GetName();

                //        if (pairs.Contains(name))
                //        {
                //            throw new Exception("插件的接口方法不允许重载");
                //        }
                //        if (list.Contains(name))
                //        {
                //            var pluginModel = this.GetPluginModel(name);
                //            var pluginEntity = new PluginEntity(new Method(methodInfo), plugin);
                //            pluginModel.Add(pluginEntity.Run);
                //        }
                //        pairs.Add(name);
                //    }
                //}
                //plugin.Loaded(this);
                //this.m_plugins.Add(plugin);
            }
        }

        object IPluginManager.Add(Type pluginType)
        {
            if (pluginType.GetCustomAttribute<PluginOptionAttribute>() is PluginOptionAttribute optionAttribute)
            {
                if (optionAttribute.Singleton)
                {
                    foreach (var item in this.Plugins)
                    {
                        if (item.GetType() == pluginType)
                        {
                            return item;
                        }
                    }
                }
            }
            var plugin = this.m_resolver.IsRegistered(pluginType)
                ? (IPlugin)this.m_resolver.Resolve(pluginType)
                : (IPlugin)this.m_resolver.ResolveWithoutRoot(pluginType);
            ((IPluginManager)this).Add(plugin);
            return plugin;
        }

        void IPluginManager.Add(Type interfeceType, Func<object, PluginEventArgs, Task> func)
        {
            lock (this.m_locker)
            {
                var pluginModel = this.GetPluginModel(interfeceType);
                pluginModel.Add(func);
            }
        }

        TPlugin IPluginManager.Add<TPlugin>(Func<IResolver, TPlugin> func)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var plugin = func.Invoke(this.m_resolver);

            ((IPluginManager)this).Add(plugin);
            return plugin;
        }

        /// <inheritdoc/>
        public int GetPluginCount(Type interfeceType)
        {
            return this.m_pluginMethods.TryGetValue(interfeceType, out var pluginModel) ? pluginModel.Count : 0;
        }

        ValueTask<bool> IPluginManager.RaiseAsync(Type interfeceType, object sender, PluginEventArgs e)
        {
            if (!this.Enable)
            {
                return new ValueTask<bool>(false);
            }
            return e.Handled
                ? new ValueTask<bool>(true)
                : !this.m_pluginMethods.TryGetValue(interfeceType, out var pluginModel)
                ? new ValueTask<bool>(false)
                : new ValueTask<bool>(RaisePluginAsync(pluginModel, sender, e));
        }

        private static async Task<bool> RaisePluginAsync(PluginModel pluginModel, object sender, PluginEventArgs e)
        {
            e.LoadModel(pluginModel, sender);
            await e.InvokeNext().ConfigureAwait(false);
            return e.Handled;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.m_locker)
                {
                    foreach (var item in this.m_plugins)
                    {
                        item.SafeDispose();
                    }
                    this.m_pluginMethods.Clear();
                    this.m_plugins.Clear();
                }
            }
            base.Dispose(disposing);
        }

        private static List<Type> SearchPluginMethod(IPlugin plugin)
        {
            var pluginMethodNames = new List<string>();
            var pluginInterfacetypes = plugin.GetType().GetInterfaces().Where(a => typeof(IPlugin).IsAssignableFrom(a)).ToList();

            return pluginInterfacetypes;
            //foreach (var type in pluginInterfacetypes)
            //{
            //    var pairs = new List<string>();

            //    var methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            //    foreach (var methodInfo in methodInfos)
            //    {
            //        if (methodInfo.GetParameters().Length == 2 && typeof(PluginEventArgs).IsAssignableFrom(methodInfo.GetParameters()[1].ParameterType) && methodInfo.ReturnType == typeof(Task))
            //        {
            //            var name = methodInfo.GetName();
            //            if (pairs.Contains(name))
            //            {
            //                throw new Exception("插件的接口方法不允许重载");
            //            }
            //            if (!pluginMethodNames.Contains(name))
            //            {
            //                pluginMethodNames.Add(name);
            //            }

            //            pairs.Add(name);
            //        }
            //    }
            //}
            //return pluginMethodNames;
        }

        private PluginModel GetPluginModel(Type interfeceType)
        {
            if (!this.m_pluginMethods.TryGetValue(interfeceType, out var pluginModel))
            {
                pluginModel = new PluginModel();
                this.m_pluginMethods.Add(interfeceType, pluginModel);
            }
            return pluginModel;
        }
    }
}