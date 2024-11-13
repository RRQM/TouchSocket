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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// PluginManagerExtension
    /// </summary>
    public static class PluginManagerExtension
    {
        public const DynamicallyAccessedMemberTypes PluginAccessedMemberTypes = DynamicallyAccessedMemberTypes.All;

        public static TPlugin Add<[DynamicallyAccessedMembers(PluginAccessedMemberTypes)] TPlugin>(this IPluginManager pluginManager, Func<IResolver, TPlugin> func) where TPlugin : class, IPlugin
        {
            ThrowHelper.ThrowArgumentNullExceptionIf(func, nameof(func));
            var plugin = func.Invoke(pluginManager.Resolver);

            pluginManager.Add(plugin);
            return plugin;
        }

        public static object Add(this IPluginManager pluginManager, [DynamicallyAccessedMembers(PluginAccessedMemberTypes)] Type pluginType)
        {
            if (pluginType.GetCustomAttribute<PluginOptionAttribute>() is PluginOptionAttribute optionAttribute)
            {
                if (optionAttribute.Singleton)
                {
                    foreach (var item in pluginManager.Plugins)
                    {
                        if (item.GetType() == pluginType)
                        {
                            return item;
                        }
                    }
                }
            }
            IPlugin plugin;
            if (pluginManager.Resolver.IsRegistered(pluginType))
            {
                plugin = (IPlugin)pluginManager.Resolver.Resolve(pluginType);
            }
            else
            {
                plugin = (IPlugin)pluginManager.Resolver.ResolveWithoutRoot(pluginType);
            }
            pluginManager.Add(plugin);
            return plugin;
        }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin">插件类型</typeparam>
        /// <returns>插件类型实例</returns>
        public static TPlugin Add<[DynamicallyAccessedMembers(PluginAccessedMemberTypes)] TPlugin>(this IPluginManager pluginManager) where TPlugin : class, IPlugin
        {
            TPlugin plugin;
            if (pluginManager.Resolver.IsRegistered(typeof(TPlugin)))
            {
                plugin = (TPlugin)pluginManager.Resolver.Resolve(typeof(TPlugin));
            }
            else
            {
                plugin = (TPlugin)pluginManager.Resolver.ResolveWithoutRoot(typeof(TPlugin));
            }
            pluginManager.Add(plugin);
            return plugin;
        }

        public static void Add<TSender, TEventArgs>(this IPluginManager pluginManager, Type interfeceType, Func<TSender, TEventArgs, Task> func) where TEventArgs : PluginEventArgs
        {
            Task newFunc(object sender, PluginEventArgs e)
            {
                return func((TSender)sender, (TEventArgs)e);
            }
            pluginManager.Add(interfeceType, newFunc, func);
        }

        public static void Add<TEventArgs>(this IPluginManager pluginManager, Type interfeceType, Func<TEventArgs, Task> func) where TEventArgs : PluginEventArgs
        {
            Task newFunc(object sender, PluginEventArgs e)
            {
                return func((TEventArgs)e);
            }
            pluginManager.Add(interfeceType, newFunc, func);
        }

        public static void Add(this IPluginManager pluginManager, Type interfeceType, Func<Task> func)
        {
            async Task newFunc(object sender, PluginEventArgs e)
            {
                await func().ConfigureAwait(false);
                await e.InvokeNext().ConfigureAwait(false);
            }
            pluginManager.Add(interfeceType, newFunc, func);
        }

        public static void Add<T>(this IPluginManager pluginManager, Type interfeceType, Action<T> action) where T : class
        {
            if (typeof(PluginEventArgs).IsAssignableFrom(typeof(T)))
            {
                async Task newFunc(object sender, PluginEventArgs e)
                {
                    action(e as T);
                    await e.InvokeNext().ConfigureAwait(false);
                }
                pluginManager.Add(interfeceType, newFunc, action);
            }
            else
            {
                async Task newFunc(object sender, PluginEventArgs e)
                {
                    action((T)sender);
                    await e.InvokeNext().ConfigureAwait(false);
                }
                pluginManager.Add(interfeceType, newFunc, action);
            }
        }

        public static void Add(this IPluginManager pluginManager, Type interfeceType, Action action)
        {
            async Task newFunc(object sender, PluginEventArgs e)
            {
                action();
                await e.InvokeNext().ConfigureAwait(false);
            }
            pluginManager.Add(interfeceType, newFunc, action);
        }
    }
}