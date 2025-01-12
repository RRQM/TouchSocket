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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 表示插件管理器。
    /// </summary>
    public sealed class PluginManager : DisposableObject, IPluginManager
    {
        private readonly Lock m_locker = LockFactory.Create();
        private readonly IScopedResolver m_scopedResolver;
        private Dictionary<Type, PluginInvokeLine> m_pluginMethods = new Dictionary<Type, PluginInvokeLine>();

        private List<IPlugin> m_plugins = new List<IPlugin>();

        /// <summary>
        /// 表示插件管理器
        /// </summary>
        /// <param name="resolver"></param>
        public PluginManager(IResolver resolver)
        {
            this.m_scopedResolver = resolver.CreateScopedResolver();
            this.Enable = true;
        }

        /// <inheritdoc/>
        public bool Enable { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IPlugin> Plugins => this.m_plugins;

        /// <inheritdoc/>
        public IResolver Resolver => this.m_scopedResolver.Resolver;

        /// <inheritdoc/>
        public void Add(Type pluginType, Func<object, PluginEventArgs, Task> pluginInvokeHandler, Delegate sourceDelegate = default)
        {
            lock (this.m_locker)
            {
                var pluginInvokeLine = this.GetPluginInvokeLine(pluginType);
                pluginInvokeLine.Add(new PluginEntity(pluginInvokeHandler, sourceDelegate ?? pluginInvokeHandler));
            }
        }

        /// <inheritdoc/>
        public IPlugin Add([DynamicallyAccessedMembers(PluginManagerExtension.PluginAccessedMemberTypes)] Type pluginType)
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(pluginType, nameof(pluginType));

            this.ThrowIfDisposed();

            IPlugin plugin;

            lock (this.m_locker)
            {
                var fromIoc = false;
                if (pluginType.GetCustomAttribute<PluginOptionAttribute>() is PluginOptionAttribute optionAttribute)
                {
                    fromIoc = optionAttribute.FromIoc;
                    if (fromIoc)
                    {
                        plugin = default;
                    }
                    else
                    {
                        plugin = (IPlugin)this.Resolver.ResolveWithoutRoot(pluginType);
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
                }
                else
                {
                    plugin = (IPlugin)this.Resolver.ResolveWithoutRoot(pluginType);
                }

                foreach (var pluginInterface in pluginType.GetInterfaces())
                {
                    if (!IsPluginInterface(pluginInterface))
                    {
                        continue;
                    }
                    var pluginInvokeLine = this.GetPluginInvokeLine(pluginInterface);

                    foreach (var methodInfo in pluginInterface.GetMethods())
                    {
                        if (IsPluginMethod(methodInfo))
                        {
                            if (fromIoc)
                            {
                                var pluginEntity = new PluginEntity(new Method(methodInfo), pluginType, this);
                                pluginInvokeLine.Add(pluginEntity);
                            }
                            else
                            {
                                var pluginEntity = new PluginEntity(new Method(methodInfo), plugin);
                                pluginInvokeLine.Add(pluginEntity);
                            }
                        }
                    }
                }

                if (plugin != null)
                {
                    plugin.Loaded(this);
                    this.m_plugins.Add(plugin);
                }
                return plugin;
            }
        }

        /// <inheritdoc/>
        public void Add<[DynamicallyAccessedMembers(PluginManagerExtension.PluginAccessedMemberTypes)] TPlugin>(TPlugin plugin) where TPlugin : class, IPlugin
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(plugin, nameof(plugin));

            this.ThrowIfDisposed();

            lock (this.m_locker)
            {
                var pluginType = plugin.GetType();

                var fromIoc = false;
                if (pluginType.GetCustomAttribute<PluginOptionAttribute>() is PluginOptionAttribute optionAttribute)
                {
                    fromIoc = optionAttribute.FromIoc;
                    if (optionAttribute.Singleton)
                    {
                        foreach (var item in this.Plugins)
                        {
                            if (item.GetType() == pluginType)
                            {
                                return;
                            }
                        }
                    }
                }

                foreach (var pluginInterface in pluginType.GetInterfaces())
                {
                    if (!IsPluginInterface(pluginInterface))
                    {
                        continue;
                    }
                    var pluginInvokeLine = this.GetPluginInvokeLine(pluginInterface);

                    foreach (var methodInfo in pluginInterface.GetMethods())
                    {
                        if (IsPluginMethod(methodInfo))
                        {
                            var pluginEntity = new PluginEntity(new Method(methodInfo), plugin);
                            pluginInvokeLine.Add(pluginEntity);
                        }
                    }
                }

                plugin.Loaded(this);
                this.m_plugins.Add(plugin);
            }
        }

        /// <inheritdoc/>
        public int GetFromIocCount(Type pluginType)
        {
            if (!this.Enable)
            {
                return 0;
            }

            if (this.m_pluginMethods.TryGetValue(pluginType, out var pluginInvokeLine))
            {
                return pluginInvokeLine.FromIocCount;
            }

            return 0;
        }

        /// <inheritdoc/>
        public int GetPluginCount(Type pluginType)
        {
            if (this.m_pluginMethods.TryGetValue(pluginType, out var pluginModel))
            {
                return pluginModel.GetPluginEntities().Count;
            }
            else
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        public ValueTask<bool> RaiseAsync(Type pluginType, IResolver resolver, object sender, PluginEventArgs e)
        {
            if (!this.Enable)
            {
                return new ValueTask<bool>(false);
            }

            if (e.Handled)
            {
                return new ValueTask<bool>(true);
            }

            if (!this.m_pluginMethods.TryGetValue(pluginType, out var pluginInvokeLine))
            {
                return new ValueTask<bool>(false);
            }
            return new ValueTask<bool>(this.RaisePluginAsync(pluginInvokeLine, resolver, sender, e));
        }

        /// <inheritdoc/>
        public void Remove(IPlugin plugin)
        {
            lock (this.m_locker)
            {
                var dic = new Dictionary<Type, PluginInvokeLine>(this.m_pluginMethods);

                foreach (var item in dic)
                {
                    var pluginInvokeLine = item.Value;
                    pluginInvokeLine.Remove(plugin);
                }
                this.m_pluginMethods = dic;

                var list = new List<IPlugin>(this.m_plugins);
                var result = list.Remove(plugin);

                this.m_plugins = list;

                if (result)
                {
                    plugin.Unloaded(this);
                }
            }
        }

        /// <inheritdoc/>
        public void Remove(Type pluginType, Delegate func)
        {
            lock (this.m_locker)
            {
                var pluginInvokeLine = this.GetPluginInvokeLine(pluginType);

                pluginInvokeLine.Remove(func);
            }
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
                    this.m_scopedResolver.SafeDispose();
                }
            }
            base.Dispose(disposing);
        }

        private static bool IsPluginInterface(Type type)
        {
            if (type == typeof(IPlugin))
            {
                return false;
            }
            return typeof(IPlugin).IsAssignableFrom(type);
        }

        private static bool IsPluginMethod(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Length == 2 && typeof(PluginEventArgs).IsAssignableFrom(methodInfo.GetParameters()[1].ParameterType) && methodInfo.ReturnType == typeof(Task);
        }

        private PluginInvokeLine GetPluginInvokeLine([DynamicallyAccessedMembers(PluginManagerExtension.PluginAccessedMemberTypes)] Type interfeceType)
        {
            if (!this.m_pluginMethods.TryGetValue(interfeceType, out var pluginModel))
            {
                pluginModel = new PluginInvokeLine();
                this.m_pluginMethods.Add(interfeceType, pluginModel);
            }
            return pluginModel;
        }

        private async Task<bool> RaisePluginAsync(PluginInvokeLine pluginInvokeLine, IResolver resolver, object sender, PluginEventArgs e)
        {
            if (resolver == null)
            {
                e.LoadModel(pluginInvokeLine.GetPluginEntities(), sender, this.Resolver);
                await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return e.Handled;
            }
            else
            {
                e.LoadModel(pluginInvokeLine.GetPluginEntities(), sender, resolver);
                await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return e.Handled;
            }
        }
    }
}