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
using TouchSocket.Core.AspNetCore;

namespace TouchSocket.Core;


/// <summary>
/// 提供AspNetCore容器扩展方法。
/// </summary>
public static class AspNetCoreConfigExtension
{
    /// <summary>
    /// 使用<see cref="AspNetCoreContainer"/>作为容器。
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="services">服务集合</param>
    /// <returns>配置对象</returns>
    public static TouchSocketConfig UseAspNetCoreContainer(this TouchSocketConfig config, IServiceCollection services)
    {
        config.SetRegistrator(new AspNetCoreContainer(services));
        return config;
    }

    /// <summary>
    /// 配置容器。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="action">配置操作</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection ConfigureContainer(this IServiceCollection services, Action<IRegistrator> action)
    {
        var container = new AspNetCoreContainer(services);
        action.Invoke(container);
        return services;
    }

    /// <summary>
    /// 设置解析器。
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="provider">服务提供者</param>
    public static void SetResolver(this TouchSocketConfig config, IServiceProvider provider)
    {
        config.SetValue(TouchSocketCoreConfigExtension.ResolverProperty, new InternalResolver(provider));
    }

    #region InternalResolver

    /// <summary>
    /// 内部解析器。
    /// </summary>
    internal class InternalResolver : IResolver
    {
        private readonly IServiceProvider m_serviceProvider;
        private readonly IKeyedServiceProvider m_keyedServiceProvider;

        /// <summary>
        /// 初始化<see cref="InternalResolver"/>的新实例。
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public InternalResolver(IServiceProvider serviceProvider)
        {
            this.m_serviceProvider = serviceProvider;
            this.m_keyedServiceProvider = serviceProvider as IKeyedServiceProvider;
        }

        /// <inheritdoc/>
        public IScopedResolver CreateScopedResolver()
        {
            return new InternalScopedResolver(this.m_serviceProvider.CreateScope());
        }

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            return this.m_serviceProvider.GetService(serviceType);
        }

        /// <inheritdoc/>
        public object Resolve(Type fromType, object key)
        {
            return this.m_keyedServiceProvider.GetKeyedService(fromType, key);
        }

        /// <inheritdoc/>
        public object Resolve(Type fromType)
        {
            return this.m_serviceProvider.GetService(fromType);
        }

        /// <summary>
        /// 内部作用域解析器。
        /// </summary>
        private class InternalScopedResolver : DisposableObject, IScopedResolver
        {
            private readonly IServiceScope serviceScope;
            private readonly InternalResolver resolver;

            /// <summary>
            /// 初始化<see cref="InternalScopedResolver"/>的新实例。
            /// </summary>
            /// <param name="serviceScope">服务作用域</param>
            public InternalScopedResolver(IServiceScope serviceScope)
            {
                this.serviceScope = serviceScope;
                this.resolver = new InternalResolver(serviceScope.ServiceProvider);
            }

            /// <inheritdoc/>
            public IResolver Resolver => this.resolver;

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.serviceScope.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }

    #endregion
}