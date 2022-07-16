//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using System;
using TouchSocket.Core.Dependency;

namespace TouchSocket.Core.AspNetCore
{
    /// <summary>
    /// AspNetCoreContainer
    /// </summary>
    public class AspNetCoreContainer : IContainer
    {
        private readonly IServiceCollection m_services;
        readonly Container m_container = new Container();

        /// <summary>
        /// 初始化一个IServiceCollection的容器。
        /// </summary>
        /// <param name="services"></param>
        public AspNetCoreContainer(IServiceCollection services)
        {
            this.m_services = services;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsRegistered(Type fromType, string key = "")
        {
            return ((IScopedContainer)this.m_container).IsRegistered(fromType, key);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        public void Register(DependencyDescriptor descriptor, string key = "")
        {
            this.m_container.Register(descriptor, key);
            switch (descriptor.Lifetime)
            {
                case Lifetime.Singleton:
                    if (descriptor.ToInstance != null)
                    {
                        this.m_services.AddSingleton(descriptor.FromType, descriptor.ToInstance);
                    }
                    else
                    {
                        this.m_services.AddSingleton(descriptor.FromType, descriptor.ToType);
                    }
                    break;
                case Lifetime.Transient:
                default:
                    this.m_services.AddTransient(descriptor.FromType, descriptor.ToType);
                    break;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="ps"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Resolve(Type fromType, object[] ps = null, string key = "")
        {
            ServiceProvider provider = this.m_services.BuildServiceProvider();

            object obj = provider.GetService(fromType);
            if (obj != null)
            {
                return obj;
            }

            return this.m_container.Resolve(fromType, ps, key);
        }
    }
}