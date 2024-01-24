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
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 具有设置配置的对象
    /// </summary>
    public abstract class SetupConfigObject : ConfigObject, ISetupConfigObject
    {
        private TouchSocketConfig m_config;

        /// <inheritdoc/>
        public override TouchSocketConfig Config => this.m_config;

        /// <inheritdoc/>
        public IPluginManager PluginManager { get; private set; }

        /// <inheritdoc/>
        public IResolver Resolver { get; private set; }

        /// <inheritdoc/>
        public void Setup(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginManager.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(config);
            this.PluginManager.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));
        }

        /// <inheritdoc/>
        public async Task SetupAsync(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            await this.PluginManager.RaiseAsync(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config)).ConfigureFalseAwait();
            this.LoadConfig(config);
            //return EasyTask.CompletedTask;
            await this.PluginManager.RaiseAsync(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config)).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Config.SafeDispose();
                this.PluginManager.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.m_config = config ?? throw new ArgumentNullException(nameof(config));

            if (!config.TryGetValue(TouchSocketCoreConfigExtension.ResolverProperty, out var resolver))
            {
                if (!config.TryGetValue(TouchSocketCoreConfigExtension.RegistratorProperty, out var registrator))
                {
                    registrator = new Container();
                }
                if (!registrator.IsRegistered(typeof(ILog)))
                {
                    registrator.RegisterSingleton<ILog>(new LoggerGroup());
                }

                if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IRegistrator> actionContainer)
                {
                    actionContainer.Invoke(registrator);
                }

                resolver = registrator.BuildResolver();
            }

            var pluginManager = new PluginManager(resolver);

            if (this.Config.GetValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty) is Action<IPluginManager> actionPluginManager)
            {
                pluginManager.Enable = true;
                actionPluginManager.Invoke(pluginManager);
            }

            this.Logger ??= resolver.Resolve<ILog>();

            this.PluginManager = pluginManager;
            this.Resolver = resolver;
        }

#if NET6_0_OR_GREATER

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            this.Config.SafeDispose();
            this.PluginManager.SafeDispose();
            await base.DisposeAsyncCore();
        }
#endif
    }
}