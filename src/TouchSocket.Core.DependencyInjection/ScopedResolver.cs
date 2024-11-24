using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.AspNetCore;

namespace TouchSocket.Core.AspNetCore
{
    internal class ScopedResolver : IResolver, IKeyedServiceProvider
    {
        private readonly IServiceCollection m_services;
        private IServiceProvider m_serviceProvider;

        public ScopedResolver(IServiceCollection services)
        {
            this.m_services = services ?? throw new ArgumentNullException(nameof(services));

            if (services.IsReadOnly)
            {
                return;
            }
            if (!this.IsRegistered(typeof(ILog)))
            {
                services.AddSingleton<IResolver>(provider =>
                {
                    this.m_serviceProvider ??= provider;
                    return this;
                });
            }

            if (!this.IsRegistered(typeof(ILog)))
            {
                services.AddSingleton<ILog>(new LoggerGroup());
            }
        }

        public IServiceProvider ServiceProvider { get => this.m_serviceProvider; set => this.m_serviceProvider = value; }

        /// <inheritdoc/>
        public bool IsRegistered(Type fromType, string key)
        {
            if (typeof(IResolver) == fromType)
            {
                return true;
            }
            foreach (var item in this.m_services)
            {
                if (!item.IsKeyedService)
                {
                    continue;
                }
                if (item.ServiceType == fromType && item.ServiceKey?.ToString() == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public bool IsRegistered(Type fromType)
        {
            if (typeof(IResolver) == fromType)
            {
                return true;
            }
            foreach (var item in this.m_services)
            {
                if (item.ServiceType == fromType)
                {
                    return true;
                }
            }
            return false;
        }

        #region Resolve

        public IScopedResolver CreateScopedResolver()
        {
            var serviceScope = this.m_serviceProvider.CreateScope();

            return new InternalScopedResolver(serviceScope, this.m_services);
        }

        /// <inheritdoc/>
        public object GetKeyedService(Type fromType, object serviceKey)
        {
            return this.m_serviceProvider.GetRequiredKeyedService(fromType, serviceKey);
        }

        /// <inheritdoc/>
        public object GetRequiredKeyedService(Type fromType, object serviceKey)
        {
            return this.m_serviceProvider.GetRequiredKeyedService(fromType, serviceKey);
        }

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            if (serviceType==typeof(IResolver))
            {
                return this;
            }
            return this.m_serviceProvider.GetService(serviceType);
        }

        /// <inheritdoc/>
        public object Resolve(Type fromType, string key)
        {
            return this.m_serviceProvider.GetRequiredKeyedService(fromType, key);
        }

        /// <inheritdoc/>
        public object Resolve(Type fromType)
        {
            if (fromType == typeof(IResolver))
            {
                return this;
            }
            return this.m_serviceProvider.GetService(fromType);
        }

        #endregion Resolve
    }
}
