//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Dependency;
using System;
using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 插件管理器接口
    /// </summary>
    public interface IPluginsManager: IEnumerable<IPlugin>
    {
        /// <summary>
        /// 注入容器
        /// </summary>
        public IContainer  Container { get; }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        void Add(IPlugin plugin);

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="plugin"></param>
        void Remove(IPlugin plugin);

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="type"></param>
        void Remove(Type type);

        /// <summary>
        /// 清除所有插件
        /// </summary>
        void Clear();

        /// <summary>
        /// 触发对应方法
        /// </summary>
        /// <typeparam name="TPlugin">接口类型</typeparam>
        /// <param name="name">触发名称</param>
        /// <param name="params">参数</param>
        void Raise<TPlugin>(string name, params object[] @params) where TPlugin : IPlugin;
    }
}
