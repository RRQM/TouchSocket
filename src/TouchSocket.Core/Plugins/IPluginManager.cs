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
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 插件管理器接口
    /// </summary>
    public interface IPluginManager : IDisposableObject, IResolverObject
    {
        /// <summary>
        /// 标识该插件管理器是否可用。
        /// </summary>
        bool Enable { get; set; }

        /// <summary>
        /// 所包含的所有插件。
        /// </summary>
        IEnumerable<IPlugin> Plugins { get; }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        void Add<[DynamicallyAccessedMembers(PluginManagerExtension.PluginAccessedMemberTypes)] TPlugin>(TPlugin plugin) where TPlugin : class, IPlugin;

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="pluginType">插件类型</param>
        /// <returns>添加的插件实例</returns>
        IPlugin Add([DynamicallyAccessedMembers(PluginManagerExtension.PluginAccessedMemberTypes)] Type pluginType);

        /// <summary>
        /// 添加一个插件类型及其对应的调用处理程序。
        /// </summary>
        /// <param name="pluginType">插件的类型。</param>
        /// <param name="pluginInvokeHandler">插件调用处理程序，当插件被调用时执行。</param>
        /// <param name="sourceDelegate">可选的源委托，用于标识插件的来源。</param>
        void Add(Type pluginType, Func<object, PluginEventArgs, Task> pluginInvokeHandler, Delegate sourceDelegate = default);

        /// <summary>
        /// 获取来自IOC容器的指定名称的插件数量。
        /// </summary>
        /// <param name="pluginType">插件类型</param>
        /// <returns>插件数量</returns>
        int GetFromIocCount(Type pluginType);

        /// <summary>
        /// 获取已添加的指定名称的插件数量。
        /// </summary>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        int GetPluginCount(Type pluginType);

        /// <summary>
        /// 触发对应插件
        /// </summary>
        /// <param name="pluginType">插件接口类型</param>
        /// <param name="resolver">容器</param>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        /// <returns>表示在执行的插件中，是否处理<see cref="TouchSocketEventArgs.Handled"/>为<see langword="true"/>。</returns>
        ValueTask<bool> RaiseAsync(Type pluginType, IResolver resolver, object sender, PluginEventArgs e);

        /// <summary>
        /// 移除指定的插件实例
        /// </summary>
        /// <param name="plugin">要移除的插件实例</param>
        void Remove(IPlugin plugin);

        /// <summary>
        /// 根据插件类型和功能委托移除插件
        /// </summary>
        /// <param name="pluginType">要移除的插件类型</param>
        /// <param name="func">代表要移除的功能的委托</param>
        void Remove(Type pluginType, Delegate func);
    }
}