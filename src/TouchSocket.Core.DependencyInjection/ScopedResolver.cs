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

using Microsoft.Extensions.DependencyInjection;
using System;

namespace TouchSocket.Core.AspNetCore;

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
        if (serviceType == typeof(IResolver))
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