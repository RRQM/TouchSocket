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

namespace TouchSocket.Core
{
    /// <summary>
    /// PluginBase
    /// </summary>
    public class PluginBase : DisposableObject, IPlugin
    {
        private IPluginManager m_pluginManager;

        /// <summary>
        /// 插件管理器
        /// </summary>
        public IPluginManager PluginManager => this.m_pluginManager;

        void IPlugin.Unloaded(IPluginManager pluginManager)
        {
            this.Unloaded(pluginManager);
        }

        void IPlugin.Loaded(IPluginManager pluginManager)
        {
            this.m_pluginManager = pluginManager;
            this.Loaded(pluginManager);
        }

        /// <inheritdoc cref="IPlugin.Loaded(IPluginManager)"/>
        protected virtual void Loaded(IPluginManager pluginManager)
        {
        }

        /// <inheritdoc cref="IPlugin.Unloaded(IPluginManager)"/>
        protected virtual void Unloaded(IPluginManager pluginManager)
        {
        }
    }
}