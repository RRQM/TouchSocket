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
        /// 获取已添加的指定名称的插件数量。
        /// </summary>
        /// <param name="interfeceType"></param>
        /// <returns></returns>
        int GetPluginCount(Type interfeceType);

        /// <summary>
        /// 所包含的所有插件。
        /// </summary>
        IEnumerable<IPlugin> Plugins { get; }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        void Add<[DynamicallyAccessedMembers(PluginManagerExtension.PluginAccessedMemberTypes)]TPlugin>(TPlugin plugin)where TPlugin:class,IPlugin;

        
        void Add(Type interfeceType, Func<object, PluginEventArgs, Task> pluginInvokeHandler,Delegate sourceDelegate=default);

        /// <summary>
        /// 触发对应插件
        /// </summary>
        /// <param name="interfeceType"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>表示在执行的插件中，是否处理<see cref="TouchSocketEventArgs.Handled"/>为<see langword="true"/>。</returns>
        ValueTask<bool> RaiseAsync(Type interfeceType, object sender, PluginEventArgs e);
        void Remove(IPlugin plugin);
        void Remove(Type interfeceType, Delegate func);

        //void Add<TSender, TEventArgs>(Type interfeceType, Func<TSender, TEventArgs, Task> func) 
        //    where TSender:class 
        //    where TEventArgs : PluginEventArgs;
    }
}