using RRQMCore.Dependency;
using RRQMCore.Log;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 插件接口
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// 插件执行顺序
        /// <para>该属性值越大，越靠前执行。值相等时，按添加先后顺序</para>
        /// <para>该属性效果，仅在<see cref="IPluginsManager.Add(IPlugin)"/>之前设置有效。</para>
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 日志记录器。
        /// <para>在<see cref="IPluginsManager.Add(IPlugin)"/>之前如果没有赋值的话，随后会用<see cref="IContainer.Resolve{T}"/>填值</para>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// 包含此插件的插件管理器
        /// </summary>
        IPluginsManager PluginsManager { get; set; }
    }
}
