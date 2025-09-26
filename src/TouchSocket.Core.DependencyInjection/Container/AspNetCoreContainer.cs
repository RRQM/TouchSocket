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

namespace TouchSocket.Core.AspNetCore;

/// <summary>
/// AspNetCoreContainer 类实现了 IRegistrator、IResolver 和 IKeyedServiceProvider 接口，
/// 提供了一个容器解决方案，用于在 ASP.NET Core 应用中注册服务和解析服务。
/// 它旨在简化依赖注入过程，并支持 keyed 服务的获取。
/// </summary>
public class AspNetCoreContainer : IRegistrator, IResolver, IKeyedServiceProvider
{
    private readonly ScopedResolver m_scopedResolver;
    private readonly IServiceCollection m_services;

    /// <summary>
    /// 初始化AspNetCoreContainer实例。
    /// </summary>
    /// <param name="services">IServiceCollection实例，用于注册服务。</param>
    public AspNetCoreContainer(IServiceCollection services)
    {
        this.m_services = services ?? throw new ArgumentNullException(nameof(services));
        this.m_scopedResolver = new ScopedResolver(services);
    }

    /// <summary>
    /// 获取当前对象的IServiceProvider实例。
    /// </summary>
    public IServiceProvider ServiceProvider => this.m_scopedResolver.ServiceProvider;

    /// <inheritdoc/>
    public IResolver BuildResolver()
    {
        this.m_scopedResolver.ServiceProvider = this.m_services.BuildServiceProvider();
        return this;
    }

    /// <summary>
    /// 以传入的<see cref="IServiceProvider"/>作为服务提供
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public IResolver BuildResolver(IServiceProvider provider)
    {
        this.m_scopedResolver.ServiceProvider = provider;
        return this;
    }

    /// <inheritdoc/>
    public IEnumerable<DependencyDescriptor> GetDescriptors()
    {
        return this.m_services.Select(s =>
        {
            var descriptor = new DependencyDescriptor(s.ServiceType, s.ImplementationType, (Lifetime)((int)s.Lifetime))
            {
                ToInstance = s.ImplementationInstance
            };
            return descriptor;
        });
    }

    /// <inheritdoc/>
    public bool IsRegistered(Type fromType, string key)
    {
        return this.m_scopedResolver.IsRegistered(fromType, key);
    }

    /// <inheritdoc/>
    public bool IsRegistered(Type fromType)
    {
        return this.m_scopedResolver.IsRegistered(fromType);
    }

    /// <inheritdoc/>
    public void Register(DependencyDescriptor descriptor, string key)
    {
        switch (descriptor.Lifetime)
        {
            case Lifetime.Singleton:
                {
                    if (descriptor.ToInstance != null)
                    {
                        this.m_services.AddKeyedSingleton(descriptor.FromType, key, descriptor.ToInstance);
                    }
                    else
                    {
                        this.m_services.AddKeyedSingleton(descriptor.FromType, key, descriptor.ToType);
                    }
                    break;
                }
            case Lifetime.Scoped:
                {
                    this.m_services.AddKeyedScoped(descriptor.FromType, key, descriptor.ToType);
                    break;
                }

            case Lifetime.Transient:
            default:
                this.m_services.AddKeyedTransient(descriptor.FromType, key, descriptor.ToType);
                break;
        }
    }

    /// <inheritdoc/>
    public void Register(DependencyDescriptor descriptor)
    {
        switch (descriptor.Lifetime)
        {
            case Lifetime.Singleton:
                {
                    if (descriptor.ToInstance != null)
                    {
                        this.m_services.AddSingleton(descriptor.FromType, descriptor.ToInstance);
                    }
                    else
                    {
                        this.m_services.AddSingleton(descriptor.FromType, descriptor.ToType);
                    }
                    break;
                }
            case Lifetime.Scoped:
                {
                    this.m_services.AddScoped(descriptor.FromType, descriptor.ToType);
                    break;
                }
            case Lifetime.Transient:
            default:
                this.m_services.AddTransient(descriptor.FromType, descriptor.ToType);
                break;
        }
    }

    /// <inheritdoc/>
    public void Unregister(DependencyDescriptor descriptor, string key)
    {
        var array = this.m_services.ToArray();
        foreach (var item in array)
        {
            if (!item.IsKeyedService)
            {
                continue;
            }
            if (item.ServiceType == descriptor.FromType && item.ServiceKey?.ToString() == key)
            {
                this.m_services.Remove(item);
                return;
            }
        }
    }

    /// <inheritdoc/>
    public void Unregister(DependencyDescriptor descriptor)
    {
        var array = this.m_services.ToArray();
        foreach (var item in array)
        {
            if (item.ServiceType == descriptor.FromType)
            {
                this.m_services.Remove(item);
                return;
            }
        }
    }

    #region Resolve

    /// <inheritdoc/>
    public IScopedResolver CreateScopedResolver()
    {
        var serviceScope = this.m_scopedResolver.CreateScope();
        return new InternalScopedResolver(serviceScope, this.m_services);
    }

    /// <inheritdoc/>
    public object GetKeyedService(Type fromType, object serviceKey)
    {
        return this.m_scopedResolver.GetKeyedService(fromType, serviceKey);
    }

    /// <inheritdoc/>
    public object GetRequiredKeyedService(Type fromType, object serviceKey)
    {
        return this.GetRequiredKeyedService(fromType, serviceKey);
    }

    /// <inheritdoc/>
    public object GetService(Type serviceType)
    {
        return this.m_scopedResolver.GetService(serviceType);
    }

    /// <inheritdoc/>
    public object Resolve(Type fromType, string key)
    {
        return this.m_scopedResolver.GetKeyedService(fromType, key);
    }

    /// <inheritdoc/>
    public object Resolve(Type fromType)
    {
        return this.m_scopedResolver.GetService(fromType);
    }

    #endregion Resolve
}