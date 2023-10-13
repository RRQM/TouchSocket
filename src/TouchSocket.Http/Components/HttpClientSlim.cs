﻿#if !NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
    /// </summary>
    public class HttpClientSlim : DisposableObject
    {
        private readonly System.Net.Http.HttpClient m_httpClient;

        /// <summary>
        /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpClientSlim(System.Net.Http.HttpClient httpClient = default)
        {
            httpClient ??= new System.Net.Http.HttpClient();
            this.m_httpClient = httpClient;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <summary>
        /// Ioc容器
        /// </summary>
        public IContainer Container { get; private set; }

        /// <summary>
        /// 插件管理器
        /// </summary>
        public IPluginsManager PluginsManager { get; private set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; private set; }

        /// <summary>
        /// 通讯客户端
        /// </summary>
        public System.Net.Http.HttpClient HttpClient => this.m_httpClient;

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            this.m_httpClient.BaseAddress ??= config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            this.Logger ??= this.Container.Resolve<ILog>();

        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientSlim Setup(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginsManager.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(this.Config);
            this.PluginsManager.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));

            return this;
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.Config = config;

            if (!(config.GetValue(TouchSocketCoreConfigExtension.ContainerProperty) is IContainer container))
            {
                container = new Container();
            }

            if (!container.IsRegistered(typeof(ILog)))
            {
                container.RegisterSingleton<ILog, LoggerGroup>();
            }

            if (!(config.GetValue(TouchSocketCoreConfigExtension.PluginsManagerProperty) is IPluginsManager pluginsManager))
            {
                pluginsManager = new PluginsManager(container);
            }

            if (container.IsRegistered(typeof(IPluginsManager)))
            {
                pluginsManager = container.Resolve<IPluginsManager>();
            }
            else
            {
                container.RegisterSingleton<IPluginsManager>(pluginsManager);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IContainer> actionContainer)
            {
                actionContainer.Invoke(container);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty) is Action<IPluginsManager> actionPluginsManager)
            {
                pluginsManager.Enable = true;
                actionPluginsManager.Invoke(pluginsManager);
            }
            this.Container = container;
            this.PluginsManager = pluginsManager;
        }
    }
}
#endif
