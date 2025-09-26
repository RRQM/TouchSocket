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

using Autofac;

namespace TouchSocket.Core;

/// <summary>
/// AutofacContainer
/// </summary>
public class AutofacContainer : IRegistrator, IResolver
{
    private readonly ContainerBuilder m_containerBuilder;
    private Autofac.IContainer m_serviceProvider;

    /// <summary>
    /// AutofacContainer
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AutofacContainer(ContainerBuilder containerBuilder)
    {
        this.m_containerBuilder = containerBuilder ?? throw new ArgumentNullException(nameof(containerBuilder));
        containerBuilder.RegisterInstance(this).As<IResolver>();
    }

    /// <summary>
    /// AutofacContainer
    /// </summary>
    /// <param name="container"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AutofacContainer(Autofac.IContainer container)
    {
        this.m_serviceProvider = container ?? throw new ArgumentNullException(nameof(container));
    }

    /// <inheritdoc/>
    public IResolver BuildResolver()
    {
        this.m_serviceProvider = this.m_containerBuilder.Build();
        return this;
    }

    /// <inheritdoc/>
    public IEnumerable<DependencyDescriptor> GetDescriptors()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool IsRegistered(Type fromType, string key)
    {
        throw new NotSupportedException($"{this.GetType().Name}不支持包含Key的设定");
    }

    /// <inheritdoc/>
    public bool IsRegistered(Type fromType)
    {
        if (typeof(IResolver) == fromType)
        {
            return true;
        }
        if (this.m_serviceProvider == null)
        {
            return false;
        }
        return this.m_serviceProvider.IsRegistered(fromType);
    }

    /// <inheritdoc/>
    public void Register(DependencyDescriptor descriptor, string key)
    {
        throw new NotSupportedException($"{this.GetType().Name}不支持包含Key的设定");
    }

    /// <inheritdoc/>
    public void Register(DependencyDescriptor descriptor)
    {
        switch (descriptor.Lifetime)
        {
            case Lifetime.Singleton:
                if (descriptor.ToInstance != null)
                {
                    this.m_containerBuilder.RegisterInstance(descriptor.ToInstance).As(descriptor.FromType).SingleInstance();
                }
                else
                {
                    this.m_containerBuilder.RegisterType(descriptor.ToType).As(descriptor.FromType).SingleInstance();
                }
                break;

            case Lifetime.Transient:
            default:
                this.m_containerBuilder.RegisterType(descriptor.ToType).As(descriptor.FromType);
                break;
        }
    }

    /// <inheritdoc/>
    public void Unregister(DependencyDescriptor descriptor, string key)
    {
        throw new NotSupportedException($"{this.GetType().Name}不支持包含Key的设定");
    }

    /// <inheritdoc/>
    public void Unregister(DependencyDescriptor descriptor)
    {
        throw new NotImplementedException();
    }

    #region Resolve

    /// <inheritdoc/>
    public object GetService(Type serviceType)
    {
        if (typeof(IResolver) == serviceType)
        {
            return this;
        }
        return this.m_serviceProvider.Resolve(serviceType);
    }

    /// <inheritdoc/>
    public object Resolve(Type fromType, string key)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public object Resolve(Type fromType)
    {
        if (typeof(IResolver) == fromType)
        {
            return this;
        }
        return this.m_serviceProvider.Resolve(fromType);
    }

    /// <inheritdoc/>
    public IScopedResolver CreateScopedResolver()
    {
        return new InternalScopedResolver(this);
    }

    #endregion Resolve

    internal class InternalScopedResolver : DisposableObject, IScopedResolver
    {
        public IResolver Resolver { get; set; }

        public InternalScopedResolver(IResolver resolver)
        {
            this.Resolver = resolver;
        }
    }
}