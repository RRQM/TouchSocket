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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace TouchSocket.Core;

/// <summary>
/// IOC容器
/// </summary>
public sealed class Container : IContainer
{
    private readonly ConcurrentDictionary<ServiceKey, ServiceEntry> m_services = new();
    private readonly ConcurrentDictionary<Type, Func<IResolver, object>> m_factories = new();

    /// <summary>
    /// IOC容器
    /// </summary>
    public Container()
    {
        this.RegisterSingleton<IResolver>(this);
        this.RegisterSingleton<IServiceProvider>(this);
    }

    /// <inheritdoc/>
    public IResolver BuildResolver() => this;

    /// <inheritdoc/>
    public IScopedResolver CreateScopedResolver() => new ScopedResolver(this);

    /// <inheritdoc/>
    public IEnumerable<DependencyDescriptor> GetDescriptors()
    {
        return this.m_services.Values.Select(e => e.Descriptor);
    }

    /// <inheritdoc/>
    public object GetService(Type serviceType) => this.Resolve(serviceType);


    /// <inheritdoc/>
    public bool IsRegistered(Type fromType) => this.IsRegistered(fromType, null);


    /// <inheritdoc/>
    public void Register(DependencyDescriptor descriptor) => this.Register(descriptor, null);


    /// <inheritdoc/>
    public object Resolve(Type fromType) => this.Resolve(fromType, null);


    /// <inheritdoc/>
    public void Unregister(DependencyDescriptor descriptor) => this.Unregister(descriptor, null);

    /// <summary>
    /// 判断某类型是否已经注册
    /// </summary>
    /// <param name="fromType">要检查的类型</param>
    /// <param name="key">与类型关联的唯一键</param>
    /// <returns>如果类型已注册，则返回 true；否则返回 false</returns>
    public bool IsRegistered(Type fromType, object key)
    {
        var serviceKey = new ServiceKey(fromType, key);
        return this.m_services.ContainsKey(serviceKey);
    }

    /// <summary>
    /// 添加类型描述符
    /// </summary>
    /// <param name="descriptor">依赖描述符</param>
    /// <param name="key">注册键</param>
    public void Register(DependencyDescriptor descriptor, object key)
    {
        var serviceKey = new ServiceKey(descriptor.FromType, key);
        var entry = new ServiceEntry(descriptor);
        // 如果是实例注册，则直接作为单例实例缓存
        if (descriptor.ToInstance != null)
        {
            entry.SingletonInstance = descriptor.ToInstance;
        }
        this.m_services.AddOrUpdate(serviceKey, entry, (k, v) => entry);
    }

    /// <summary>
    /// 解析给定类型和键对应的实例
    /// </summary>
    /// <param name="fromType">要解析的目标类型</param>
    /// <param name="key">可选的实例标识符</param>
    /// <returns>解析出的实例</returns>
    public object Resolve(Type fromType, object key)
    {
        var serviceKey = new ServiceKey(fromType, key);

        if (this.m_services.TryGetValue(serviceKey, out var entry))
        {
            return this.ResolveFromEntry(entry, this);
        }

        // 尝试泛型类型解析
        if (fromType.IsGenericType)
        {
            var genericTypeDefinition = fromType.GetGenericTypeDefinition();
            var genericServiceKey = new ServiceKey(genericTypeDefinition, key);

            if (this.m_services.TryGetValue(genericServiceKey, out var genericEntry))
            {
                return this.ResolveFromEntry(genericEntry, this, fromType.GetGenericArguments());
            }
        }

        return null;
    }

    /// <summary>
    /// 移除注册信息
    /// </summary>
    /// <param name="descriptor">依赖描述符</param>
    /// <param name="key">注册键</param>
    public void Unregister(DependencyDescriptor descriptor, object key)
    {
        var serviceKey = new ServiceKey(descriptor.FromType, key);
        this.m_services.TryRemove(serviceKey, out _);
    }

    [UnconditionalSuppressMessage("AOT", "IL2072:RequiresUnreferencedCode", Justification = "通过依赖注册确保类型的构造函数可用")]
    [UnconditionalSuppressMessage("Trim", "IL2026:RequiresUnreferencedCode", Justification = "通过依赖注册确保类型的构造函数可用")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "通过依赖注册确保类型的构造函数可用")]
    private object ResolveFromEntry(ServiceEntry entry, IResolver resolver, Type[] genericArguments = null)
    {
        var descriptor = entry.Descriptor;

        // 如果使用实例注册，直接返回该实例
        if (descriptor.ToInstance != null)
        {
            return descriptor.ToInstance;
        }

        // 处理单例（包含工厂的情况）：统一加锁并缓存
        if (descriptor.Lifetime == Lifetime.Singleton)
        {
            if (entry.SingletonInstance != null)
            {
                return entry.SingletonInstance;
            }

            lock (entry.Lock)
            {
                if (entry.SingletonInstance != null)
                {
                    return entry.SingletonInstance;
                }

                var instance = descriptor.ImplementationFactory != null
                    ? descriptor.ImplementationFactory(resolver)
                    : this.CreateInstance(descriptor.ToType, resolver, genericArguments);

                entry.SingletonInstance = instance;
                descriptor.OnResolved?.Invoke(instance);
                return instance;
            }
        }

        // 处理瞬态和作用域（在Container级别，作用域和瞬态表现相同）：每次创建
        var newInstance = descriptor.ImplementationFactory != null
            ? descriptor.ImplementationFactory(resolver)
            : this.CreateInstance(descriptor.ToType, resolver, genericArguments);
        descriptor.OnResolved?.Invoke(newInstance);
        return newInstance;
    }

    [RequiresUnreferencedCode("此方法使用表达式树和反射创建类型实例，可能在AOT环境下不兼容")]
    [RequiresDynamicCode("此方法使用表达式树编译，在AOT环境下可能不兼容")]
    [UnconditionalSuppressMessage("AOT", "IL2055:MakeGenericType", Justification = "泛型类型参数在运行时通过依赖注册确保可用")]
    internal object CreateInstance([DynamicallyAccessedMembers(
        AOT.Container)] Type implementationType, IResolver resolver, Type[] genericArguments = null)
    {
        var targetType = implementationType;

        // 处理泛型类型
        if (genericArguments != null && implementationType.IsGenericTypeDefinition)
        {
            targetType = implementationType.MakeGenericType(genericArguments);
        }

        // 获取或创建工厂方法
        var factory = this.m_factories.GetOrAdd(targetType, CreateFactoryWrapper);
        return factory(resolver);
    }

    [RequiresUnreferencedCode("表达式树需要访问类型成员")]
    [RequiresDynamicCode("表达式树编译需要动态代码生成")]
    [UnconditionalSuppressMessage("Trim", "IL2070:GetConstructors", Justification = "类型已通过DynamicallyAccessedMembers标注确保构造函数可用")]
    private static Func<IResolver, object> CreateFactoryWrapper([DynamicallyAccessedMembers(AOT.Container)] Type type)
    {
        return CreateFactory(type);
    }

    [RequiresUnreferencedCode("表达式树需要访问类型成员")]
    [UnconditionalSuppressMessage("AOT", "IL2070:GetConstructors", Justification = "AOT环境下构造函数访问是必需的")]
    [UnconditionalSuppressMessage("Trim", "IL2026:RequiresUnreferencedCode", Justification = "表达式树编译需要访问类型成员")]
    private static Func<IResolver, object> CreateFactory([DynamicallyAccessedMembers(AOT.Container)] Type implementationType)
    {
        var constructors = implementationType.GetConstructors();

        if (constructors.Length == 0)
        {
            throw new InvalidOperationException($"类型 {implementationType.FullName} 没有公共构造函数");
        }

        // 选择参数最多的构造函数
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = constructor.GetParameters();

        var resolverParam = Expression.Parameter(typeof(IResolver), "resolver");
        var parameterExpressions = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;

            // 调用resolver.Resolve(Type)方法
            var resolveMethod = typeof(IResolver).GetMethod(nameof(IResolver.Resolve), new[] { typeof(Type) });
            var resolveCall = Expression.Call(resolverParam, resolveMethod, Expression.Constant(paramType));

            // 如果参数类型是值类型或者不允许为空，需要特殊处理
            if (paramType.IsValueType)
            {
                // 值类型需要提供默认值
                var defaultValue = Expression.Default(paramType);
                var nullCheck = Expression.Equal(resolveCall, Expression.Constant(null, typeof(object)));
                parameterExpressions[i] = Expression.Condition(
                    nullCheck,
                    Expression.Convert(defaultValue, typeof(object)),
                    resolveCall);
                parameterExpressions[i] = Expression.Convert(parameterExpressions[i], paramType);
            }
            else
            {
                // 引用类型直接转换
                parameterExpressions[i] = Expression.Convert(resolveCall, paramType);
            }
        }

        // 创建构造函数调用表达式
        var constructorCall = Expression.New(constructor, parameterExpressions);

        // 转换为object类型
        var convertToObject = Expression.Convert(constructorCall, typeof(object));

        // 编译为委托
        var lambda = Expression.Lambda<Func<IResolver, object>>(convertToObject, resolverParam);
        return lambda.Compile();
    }

    private void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] T>(T instance) where T : class
    {
        var descriptor = new DependencyDescriptor(typeof(T), typeof(T), instance);
        this.Register(descriptor);
    }

    /// <summary>
    /// 服务键，用于唯一标识一个服务注册
    /// </summary>
    private readonly struct ServiceKey : IEquatable<ServiceKey>
    {
        public readonly Type Type;
        public readonly object Key;
        private readonly int m_hashCode;

        public ServiceKey(Type type, object key)
        {
            this.Type = type;
            this.Key = key;
            unchecked
            {
                this.m_hashCode = 17;
                this.m_hashCode = this.m_hashCode * 31 + (type?.GetHashCode() ?? 0);
                this.m_hashCode = this.m_hashCode * 31 + (key?.GetHashCode() ?? 0);
            }
        }

        public bool Equals(ServiceKey other)
        {
            return this.Type == other.Type && Equals(this.Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            return obj is ServiceKey other && this.Equals(other);
        }

        public override int GetHashCode() => this.m_hashCode;
    }

    /// <summary>
    /// 服务条目，包含描述符和单例实例
    /// </summary>
    private sealed class ServiceEntry
    {
        public readonly DependencyDescriptor Descriptor;
        public readonly object Lock = new();
        public object SingletonInstance;

        public ServiceEntry(DependencyDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }
    }

    /// <summary>
    /// 作用域解析器实现
    /// </summary>
    private sealed class ScopedResolver : DisposableObject, IScopedResolver
    {
        private readonly Container m_rootContainer;
        private readonly ConcurrentDictionary<ServiceKey, object> m_scopedInstances = new();

        public ScopedResolver(Container rootContainer)
        {
            this.m_rootContainer = rootContainer;
            this.Resolver = new ScopedResolverImpl(this);
        }

        public IResolver Resolver { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放所有作用域内的IDisposable对象
                foreach (var instance in this.m_scopedInstances.Values)
                {
                    if (instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                this.m_scopedInstances.Clear();
            }
            base.Dispose(disposing);
        }

        internal object ResolveScoped(Type fromType, object key)
        {
            var serviceKey = new ServiceKey(fromType, key);

            // 如果已经在作用域中创建过，直接返回
            if (this.m_scopedInstances.TryGetValue(serviceKey, out var existingInstance))
            {
                return existingInstance;
            }

            // 检查根容器中的注册
            if (this.m_rootContainer.m_services.TryGetValue(serviceKey, out var entry))
            {
                return this.ResolveFromScopedEntry(entry, serviceKey);
            }

            // 尝试泛型类型解析
            if (fromType.IsGenericType)
            {
                var genericTypeDefinition = fromType.GetGenericTypeDefinition();
                var genericServiceKey = new ServiceKey(genericTypeDefinition, key);

                if (this.m_rootContainer.m_services.TryGetValue(genericServiceKey, out var genericEntry))
                {
                    return this.ResolveFromScopedEntry(genericEntry, serviceKey, fromType.GetGenericArguments());
                }
            }

            return null;
        }

        [UnconditionalSuppressMessage("AOT", "IL2072:RequiresUnreferencedCode", Justification = "通过依赖注册确保类型的构造函数可用")]
        [UnconditionalSuppressMessage("Trim", "IL2026:RequiresUnreferencedCode", Justification = "通过依赖注册确保类型的构造函数可用")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "通过依赖注册确保类型的构造函数可用")]
        private object ResolveFromScopedEntry(ServiceEntry entry, ServiceKey serviceKey, Type[] genericArguments = null)
        {
            var descriptor = entry.Descriptor;

            // 实例注册直接返回实例，若为Scoped生命周期则缓存
            if (descriptor.ToInstance != null)
            {
                if (descriptor.Lifetime == Lifetime.Scoped)
                {
                    this.m_scopedInstances.TryAdd(serviceKey, descriptor.ToInstance);
                }
                return descriptor.ToInstance;
            }

            // 单例直接从根容器获取（根容器内部会按需缓存，包括工厂）
            if (descriptor.Lifetime == Lifetime.Singleton)
            {
                return this.m_rootContainer.ResolveFromEntry(entry, this.Resolver, genericArguments);
            }

            // 作用域生命周期：检查是否已在当前作用域创建（支持工厂）
            if (descriptor.Lifetime == Lifetime.Scoped)
            {
                return this.m_scopedInstances.GetOrAdd(serviceKey, _ =>
                {
                    var instance = descriptor.ImplementationFactory != null
                        ? descriptor.ImplementationFactory(this.Resolver)
                        : this.m_rootContainer.CreateInstance(descriptor.ToType, this.Resolver, genericArguments);
                    descriptor.OnResolved?.Invoke(instance);
                    return instance;
                });
            }

            // 瞬态生命周期：每次都创建（支持工厂）
            var newInstance = descriptor.ImplementationFactory != null
                ? descriptor.ImplementationFactory(this.Resolver)
                : this.m_rootContainer.CreateInstance(descriptor.ToType, this.Resolver, genericArguments);
            descriptor.OnResolved?.Invoke(newInstance);
            return newInstance;
        }

        /// <summary>
        /// 作用域解析器的IResolver实现
        /// </summary>
        private sealed class ScopedResolverImpl : IResolver
        {
            private readonly ScopedResolver m_scopedResolver;

            public ScopedResolverImpl(ScopedResolver scopedResolver)
            {
                this.m_scopedResolver = scopedResolver;
            }

            public IScopedResolver CreateScopedResolver() => new ScopedResolver(this.m_scopedResolver.m_rootContainer);

            public object GetService(Type serviceType) => this.Resolve(serviceType);


            public object Resolve(Type fromType) => this.Resolve(fromType, null);

            public object Resolve(Type fromType, object key)
            {
                this.m_scopedResolver.ThrowIfDisposed();
                return this.m_scopedResolver.ResolveScoped(fromType, key);
            }
        }
    }
}