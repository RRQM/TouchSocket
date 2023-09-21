using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// PluginBase
    /// </summary>
    public class PluginBase : DisposableObject, IPlugin
    {
        /// <inheritdoc/>
        [Obsolete("该属性已被弃用，插件顺序将直接由添加顺序决定。本设置将在正式版发布时直接删除", true)]
        public int Order { get; set; }

        /// <inheritdoc cref="IPlugin.Loaded(IPluginsManager)"/>
        protected virtual void Loaded(IPluginsManager pluginsManager)
        {
        }

        void IPlugin.Loaded(IPluginsManager pluginsManager)
        {
            this.Loaded(pluginsManager);
        }
    }
}