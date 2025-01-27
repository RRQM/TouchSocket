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
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// PluginManagerExtension
/// </summary>
public static class PluginManagerExtension
{
    /// <summary>
    /// 插件访问成员类型，用于指定动态访问的成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes PluginAccessedMemberTypes = DynamicallyAccessedMemberTypes.All;

    /// <summary>
    /// 向插件管理器中添加一个指定类型的插件。
    /// </summary>
    /// <param name="pluginManager">插件管理器实例。</param>
    /// <param name="func">一个函数，用于通过解析器创建插件实例。</param>
    /// <typeparam name="TPlugin">要添加的插件类型。</typeparam>
    /// <returns>返回添加到插件管理器中的插件实例。</returns>
    public static TPlugin Add<[DynamicallyAccessedMembers(PluginAccessedMemberTypes)] TPlugin>(this IPluginManager pluginManager, Func<IResolver, TPlugin> func) where TPlugin : class, IPlugin
    {
        // 检查传入的函数是否为null，并抛出异常
        ThrowHelper.ThrowArgumentNullExceptionIf(func, nameof(func));

        // 使用提供的函数和插件管理器的解析器来创建插件实例
        var plugin = func.Invoke(pluginManager.Resolver);

        // 将创建的插件实例添加到插件管理器中
        pluginManager.Add(plugin);

        // 返回创建的插件实例
        return plugin;
    }


    /// <summary>
    /// 添加插件
    /// </summary>
    /// <typeparam name="TPlugin">插件类型</typeparam>
    /// <returns>插件类型实例</returns>
    public static TPlugin Add<[DynamicallyAccessedMembers(PluginAccessedMemberTypes)] TPlugin>(this IPluginManager pluginManager) where TPlugin : class, IPlugin
    {
        return (TPlugin)pluginManager.Add(typeof(TPlugin));
    }

    /// <summary>
    /// 扩展方法，用于向插件管理器添加一个新的事件处理函数。
    /// </summary>
    /// <param name="pluginManager">插件管理器接口，用于管理插件的加载和事件处理函数的注册。</param>
    /// <param name="interfaceType">插件接口类型，用于指定该事件处理函数将关联的插件类型。</param>
    /// <param name="func">异步事件处理函数，接受事件发送者和事件参数作为输入，并返回一个任务。</param>
    /// <typeparam name="TSender">事件发送者的类型。</typeparam>
    /// <typeparam name="TEventArgs">事件参数的类型，必须继承自PluginEventArgs。</typeparam>
    public static void Add<TSender, TEventArgs>(this IPluginManager pluginManager, Type interfaceType, Func<TSender, TEventArgs, Task> func) where TEventArgs : PluginEventArgs
    {
        // 创建一个新的任务，封装了传入的事件处理函数，以适应插件管理器所需的参数类型。
        Task newFunc(object sender, PluginEventArgs e)
        {
            // 调用传入的事件处理函数，传入转换后的参数。
            return func((TSender)sender, (TEventArgs)e);
        }

        // 调用插件管理器的Add方法，注册新的事件处理函数。
        pluginManager.Add(interfaceType, newFunc, func);
    }

    /// <summary>
    /// 扩展方法，用于向插件管理器添加一个新的事件处理函数。
    /// </summary>
    /// <param name="pluginManager">插件管理器接口，用于添加事件处理函数。</param>
    /// <param name="interfaceType">插件接口的类型，用于指定事件处理函数关联的插件类型。</param>
    /// <param name="func">要添加的事件处理函数，当事件触发时将异步执行此函数。</param>
    /// <typeparam name="TEventArgs">事件参数的类型，必须继承自PluginEventArgs。</typeparam>
    public static void Add<TEventArgs>(this IPluginManager pluginManager, Type interfaceType, Func<TEventArgs, Task> func) where TEventArgs : PluginEventArgs
    {
        // 创建一个新的任务，封装传入的事件处理函数，使其与插件管理器期望的签名匹配。
        Task newFunc(object sender, PluginEventArgs e)
        {
            // 转换基础PluginEventArgs为具体的TEventArgs类型，然后调用传入的事件处理函数。
            return func((TEventArgs)e);
        }
        // 调用插件管理器的Add方法，注册新的事件处理函数。
        pluginManager.Add(interfaceType, newFunc, func);
    }

    /// <summary>
    /// 扩展方法，用于向插件管理器中添加一个新的插件。
    /// </summary>
    /// <param name="pluginManager">插件管理器实例，用于添加插件。</param>
    /// <param name="interfaceType">插件需要实现的接口类型。</param>
    /// <param name="func">插件的具体逻辑，作为一个异步任务执行。</param>
    public static void Add(this IPluginManager pluginManager, Type interfaceType, Func<Task> func)
    {
        // 定义一个新的异步任务，封装传入的插件逻辑和插件链的继续执行
        async Task newFunc(object sender, PluginEventArgs e)
        {
            // 执行传入的插件逻辑，不捕获当前上下文
            await func().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 继续执行插件链中的下一个插件，不捕获当前上下文
            await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        // 将封装后的插件逻辑添加到插件管理器中
        pluginManager.Add(interfaceType, newFunc, func);
    }

    /// <summary>
    /// 扩展方法，用于向插件管理器添加新插件。
    /// </summary>
    /// <param name="pluginManager">插件管理器接口，用于添加插件。</param>
    /// <param name="interfaceType">插件需要实现的接口类型。</param>
    /// <param name="action">插件被调用时执行的操作。</param>
    /// <typeparam name="T">插件的类型，必须是类类型。</typeparam>
    public static void Add<T>(this IPluginManager pluginManager, Type interfaceType, Action<T> action) where T : class
    {
        // 判断泛型类型T是否继承自PluginEventArgs
        if (typeof(PluginEventArgs).IsAssignableFrom(typeof(T)))
        {
            // 如果T是PluginEventArgs的子类，则定义一个新的异步任务处理方法
            async Task newFunc(object sender, PluginEventArgs e)
            {
                // 执行传入的action，这里将e转换为T类型
                action(e as T);
                // 调用下一个插件，确保插件链的执行顺序
                await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            // 将新定义的处理方法添加到插件管理器中
            pluginManager.Add(interfaceType, newFunc, action);
        }
        else
        {
            // 如果T不是PluginEventArgs的子类，则定义另一个异步任务处理方法
            async Task newFunc(object sender, PluginEventArgs e)
            {
                // 执行传入的action，这里将sender转换为T类型
                action((T)sender);
                // 调用下一个插件，确保插件链的执行顺序
                await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            // 将新定义的处理方法添加到插件管理器中
            pluginManager.Add(interfaceType, newFunc, action);
        }
    }

    /// <summary>
    /// 扩展方法，用于向插件管理器中添加一个新的插件处理程序。
    /// </summary>
    /// <param name="pluginManager">插件管理器实例，允许通过扩展方法语法调用此方法。</param>
    /// <param name="interfaceType">插件所实现的接口类型，用于标识和分类插件。</param>
    /// <param name="action">插件处理程序将要执行的动作。</param>
    public static void Add(this IPluginManager pluginManager, Type interfaceType, Action action)
    {
        // 创建一个新的异步处理程序，它将在插件事件被触发时执行指定的动作，
        // 并在动作完成后调用事件的InvokeNext方法以继续执行下一个插件处理程序。
        async Task newFunc(object sender, PluginEventArgs e)
        {
            action(); // 执行插件处理程序指定的动作。
            await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext); // 继续执行下一个插件处理程序。
        }

        // 调用插件管理器的Add方法，注册新的异步处理程序和动作。
        pluginManager.Add(interfaceType, newFunc, action);
    }

    ///<inheritdoc cref="IPluginManager.RaiseAsync(Type, IResolver, object, PluginEventArgs)"/>
    public static ValueTask<bool> RaiseAsync(this IPluginManager pluginManager, Type pluginType, object sender, PluginEventArgs e)
    {
        return pluginManager.RaiseAsync(pluginType, default, sender, e);
    }
}