//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Core.Config
{
    /// <summary>
    /// 配置文件基类
    /// </summary>
    public class TouchSocketConfig : DependencyObject
    {
        private IContainer m_container;

        private IPluginsManager m_pluginsManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TouchSocketConfig()
        {
            this.SetContainer(new Container());
        }

        /// <summary>
        /// IOC容器。
        /// </summary>
        public IContainer Container => this.m_container;

        /// <summary>
        /// 使用插件
        /// </summary>
        public bool IsUsePlugin { get; set; }

        /// <summary>
        /// 插件管理器
        /// </summary>
        public IPluginsManager PluginsManager => this.m_pluginsManager;

        /// <summary>
        /// 设置注入容器。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TouchSocketConfig SetContainer(IContainer value)
        {
            this.m_container = value;
            this.m_container.RegisterTransient<ILog, ConsoleLogger>();
            this.SetPluginsManager(new PluginsManager(this.m_container));
            return this;
        }

        /// <summary>
        /// 设置PluginsManager
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TouchSocketConfig SetPluginsManager(IPluginsManager value)
        {
            this.m_pluginsManager = value;
            this.m_container.RegisterSingleton<IPluginsManager>(value);
            return this;
        }

        /// <summary>
        /// 启用插件
        /// </summary>
        /// <returns></returns>
        public TouchSocketConfig UsePlugin()
        {
            this.IsUsePlugin = true;
            return this;
        }
    }
}