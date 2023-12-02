using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.AspNetCore;

namespace TouchSocket.Hosting
{
    /// <summary>
    /// SetupObjectHostedService
    /// </summary>
    public abstract class SetupConfigObjectHostedService<TConfigObject> : IHostedService where TConfigObject : ISetupConfigObject
    {
        private TouchSocketConfig m_config;
        private TConfigObject m_configObject;
        private IResolver m_resolver;

        /// <summary>
        /// Config配置
        /// </summary>
        public TouchSocketConfig Config { get => this.m_config; }

        /// <summary>
        /// 实际对象。
        /// </summary>
        public TConfigObject ConfigObject { get => this.m_configObject; }

        /// <summary>
        /// IResolver
        /// </summary>
        public IResolver Resolver { get => this.m_resolver; }

        #region Internal

        internal void SetConfig(TouchSocketConfig config)
        {
            this.m_config = config;
        }

        internal void SetObject(TConfigObject configObject)
        {
            this.m_configObject = configObject;
        }

        internal void SetProvider(IServiceProvider serviceProvider, AspNetCoreContainer container)
        {
            container.BuildResolver(serviceProvider);
            this.m_resolver = container;
            this.OnSetResolver(this.Resolver);
        }

        #endregion Internal

        /// <summary>
        /// 在设置完成<see cref="IResolver"/>时调用。
        /// </summary>
        /// <param name="resolver"></param>
        protected virtual void OnSetResolver(IResolver resolver)
        { 
        
        }

        /// <summary>
        /// 启动Host。并且调用<see cref="ISetupConfigObject.SetupAsync(TouchSocketConfig)"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            this.m_config.RemoveValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty);
            this.m_config.SetResolver(this.m_resolver);
            await this.m_configObject.SetupAsync(this.m_config);
        }

        /// <inheritdoc/>
        public abstract Task StopAsync(CancellationToken cancellationToken);
    }
}