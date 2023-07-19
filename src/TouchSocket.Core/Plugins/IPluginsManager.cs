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
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 插件管理器接口
    /// </summary>
    public interface IPluginsManager : IDisposable
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
        void Add(IPlugin plugin);

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="pluginType">插件类型</param>
        object Add(Type pluginType);

        /// <summary>
        /// 添加插件异步执行委托
        /// </summary>
        /// <param name="name"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        void Add(string name, Func<object, PluginEventArgs, Task> func);


        /// <summary>
        /// 触发对应插件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task<bool> RaiseAsync(string name, object sender, PluginEventArgs e);

        /// <summary>
        /// 触发对应插件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        bool Raise(string name, object sender, PluginEventArgs e);
    }
}