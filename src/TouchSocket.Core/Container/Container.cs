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
using System.Reflection;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// IOC容器
/// </summary>
public sealed class Container : IContainer
{
    private readonly ConcurrentDictionary<string, DependencyDescriptor> m_registrations = new ConcurrentDictionary<string, DependencyDescriptor>();

    /// <summary>
    /// IOC容器
    /// </summary>
    public Container()
    {
        this.RegisterSingleton<IResolver>(this);
        this.RegisterSingleton<IServiceProvider>(this);
    }

    /// <inheritdoc/>
    public IResolver BuildResolver()
    {
        return this;
    }

    /// <inheritdoc/>
    public IScopedResolver CreateScopedResolver()
    {
        return new InternalScopedResolver(this);
    }

    /// <inheritdoc/>
    public IEnumerable<DependencyDescriptor> GetDescriptors()
    {
        return this.m_registrations.Values;
    }

    /// <inheritdoc/>
    [RequiresDynamicCode("使用泛型类型实例化需要动态代码支持")]
    [RequiresUnreferencedCode("DI容器可能需要访问未引用的类型成员")]
    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode", Justification = "IServiceProvider接口实现必须支持动态类型解析")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "IServiceProvider接口实现必须支持泛型类型实例化")]
    [UnconditionalSuppressMessage("Trim", "IL2046:RequiresUnreferencedCode", Justification = "接口实现需要与基接口保持一致")]
    [UnconditionalSuppressMessage("AOT", "IL3051:RequiresDynamicCode", Justification = "接口实现需要与基接口保持一致")]
    public object GetService(Type serviceType)
    {
        return this.Resolve(serviceType);
    }

    /// <inheritdoc/>
    public bool IsRegistered(Type fromType, string key)
    {
        return this.m_registrations.ContainsKey($"{fromType.FullName}{key}");
    }

    /// <inheritdoc/>
    public bool IsRegistered(Type fromType)
    {
        return this.m_registrations.ContainsKey(fromType.FullName);
    }

    /// <inheritdoc/>
    public void Register(DependencyDescriptor descriptor, string key)
    {
        var k = $"{descriptor.FromType.FullName}{key}";
        this.m_registrations.AddOrUpdate(k, descriptor, (k, v) => { return descriptor; });
    }

    /// <inheritdoc/>
    public void Register(DependencyDescriptor descriptor)
    {
        var k = descriptor.FromType.FullName;
        this.m_registrations.AddOrUpdate(k, descriptor, (k, v) => { return descriptor; });
    }

    /// <inheritdoc/>
    [RequiresDynamicCode("使用泛型类型实例化需要动态代码支持")]
    [RequiresUnreferencedCode("DI容器可能需要访问未引用的类型成员")]
    [UnconditionalSuppressMessage("AOT", "IL2055:MakeGenericType", Justification = "容器中的泛型类型在运行时是已知的")]
    [UnconditionalSuppressMessage("AOT", "IL3050:MakeGenericType", Justification = "容器中的泛型类型在运行时是已知的")]
    [UnconditionalSuppressMessage("Trim", "IL2046:RequiresUnreferencedCode", Justification = "接口实现需要与基接口保持一致")]
    [UnconditionalSuppressMessage("AOT", "IL3051:RequiresDynamicCode", Justification = "接口实现需要与基接口保持一致")]
    public object Resolve(Type fromType, string key)
    {
        string k;
        DependencyDescriptor descriptor;
        if (fromType.IsGenericType)
        {
            var type = fromType.GetGenericTypeDefinition();
            k = $"{type.FullName}{key}";
            if (this.m_registrations.TryGetValue(k, out descriptor))
            {
                if (descriptor.ImplementationFactory != null)
                {
                    return descriptor.ImplementationFactory.Invoke(this);
                }

                if (descriptor.Lifetime == Lifetime.Singleton)
                {
                    if (descriptor.ToInstance != null)
                    {
                        return descriptor.ToInstance;
                    }
                    lock (descriptor)
                    {
                        if (descriptor.ToInstance != null)
                        {
                            return descriptor.ToInstance;
                        }
                        else
                        {
                            return descriptor.ToType.IsGenericType
                            ? (descriptor.ToInstance = this.Create(descriptor, descriptor.ToType.MakeGenericType(fromType.GetGenericArguments())))
                             : (descriptor.ToInstance = this.Create(descriptor, descriptor.ToType));
                        }
                    }
                }

                return descriptor.ToType.IsGenericType
                    ? this.Create(descriptor, descriptor.ToType.MakeGenericType(fromType.GetGenericArguments()))
                  : this.Create(descriptor, descriptor.ToType);
            }
        }
        k = $"{fromType.FullName}{key}";
        if (this.m_registrations.TryGetValue(k, out descriptor))
        {
            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory.Invoke(this);
            }
            if (descriptor.Lifetime == Lifetime.Singleton)
            {
                if (descriptor.ToInstance != null)
                {
                    return descriptor.ToInstance;
                }
                lock (descriptor)
                {
                    return descriptor.ToInstance ??= this.Create(descriptor, descriptor.ToType);
                }
            }
            return this.Create(descriptor, descriptor.ToType);
        }
        return default;
    }

    /// <inheritdoc/>
    [RequiresDynamicCode("使用泛型类型实例化需要动态代码支持")]
    [RequiresUnreferencedCode("DI容器可能需要访问未引用的类型成员")]
    [UnconditionalSuppressMessage("AOT", "IL2055:MakeGenericType", Justification = "容器中的泛型类型在运行时是已知的")]
    [UnconditionalSuppressMessage("AOT", "IL3050:MakeGenericType", Justification = "容器中的泛型类型在运行时是已知的")]
    [UnconditionalSuppressMessage("Trim", "IL2046:RequiresUnreferencedCode", Justification = "接口实现需要与基接口保持一致")]
    [UnconditionalSuppressMessage("AOT", "IL3051:RequiresDynamicCode", Justification = "接口实现需要与基接口保持一致")]
    public object Resolve(Type fromType)
    {
        string k;
        DependencyDescriptor descriptor;
        if (fromType.IsGenericType)
        {
            var type = fromType.GetGenericTypeDefinition();
            k = type.FullName;
            if (this.m_registrations.TryGetValue(k, out descriptor))
            {
                if (descriptor.ImplementationFactory != null)
                {
                    return descriptor.ImplementationFactory.Invoke(this);
                }

                if (descriptor.Lifetime == Lifetime.Singleton)
                {
                    if (descriptor.ToInstance != null)
                    {
                        return descriptor.ToInstance;
                    }
                    lock (descriptor)
                    {
                        if (descriptor.ToInstance != null)
                        {
                            return descriptor.ToInstance;
                        }
                        else
                        {
                            return descriptor.ToType.IsGenericType
                                  ? (descriptor.ToInstance = this.Create(descriptor, descriptor.ToType.MakeGenericType(fromType.GetGenericArguments())))
                                   : (descriptor.ToInstance = this.Create(descriptor, descriptor.ToType));
                        }
                    }
                }

                return descriptor.ToType.IsGenericType
                ? this.Create(descriptor, descriptor.ToType.MakeGenericType(fromType.GetGenericArguments()))
                       : this.Create(descriptor, descriptor.ToType);
            }
        }
        k = fromType.FullName;
        if (this.m_registrations.TryGetValue(k, out descriptor))
        {
            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory.Invoke(this);
            }
            if (descriptor.Lifetime == Lifetime.Singleton)
            {
                if (descriptor.ToInstance != null)
                {
                    return descriptor.ToInstance;
                }
                lock (descriptor)
                {
                    return descriptor.ToInstance ??= this.Create(descriptor, descriptor.ToType);
                }
            }
            return this.Create(descriptor, descriptor.ToType);
        }
        return default;
    }

    /// <inheritdoc/>
    public void Unregister(DependencyDescriptor descriptor, string key)
    {
        var k = $"{descriptor.FromType.FullName}{key}";
        this.m_registrations.TryRemove(k, out _);
    }

    /// <inheritdoc/>
    public void Unregister(DependencyDescriptor descriptor)
    {
        var k = descriptor.FromType.FullName;
        this.m_registrations.TryRemove(k, out _);
    }

    [RequiresUnreferencedCode("DI容器需要访问类型的构造函数、属性和方法")]
    [UnconditionalSuppressMessage("AOT", "IL2070:GetConstructors", Justification = "DI容器需要访问构造函数进行对象创建")]
    [UnconditionalSuppressMessage("AOT", "IL2067:CreateInstance", Justification = "DI容器需要通过Activator创建实例")]
    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode", Justification = "DI容器明确需要访问未引用的成员")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Create方法内部调用Resolve方法需要动态代码支持")]
    private object Create(DependencyDescriptor descriptor, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type toType)
    {
        var ctor = toType.GetConstructors().FirstOrDefault(x => x.IsDefined(typeof(DependencyInjectAttribute), true));
        if (ctor is null)
        {
            //如果没有被特性标记，那就取构造函数参数最多的作为注入目标
            if (toType.GetConstructors().Length == 0)
            {
                throw new Exception(TouchSocketCoreResource.NotFindPublicConstructor.Format(toType));
            }
            ctor = toType.GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
        }

        DependencyTypeAttribute dependencyTypeAttribute = null;
        if (toType.IsDefined(typeof(DependencyTypeAttribute), true))
        {
            dependencyTypeAttribute = toType.GetCustomAttribute<DependencyTypeAttribute>();
        }

        var parameters = ctor.GetParameters();
        var ps = new object[parameters.Length];

        if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Constructor))
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsPrimitive || parameters[i].ParameterType == typeof(string))
                {
                    ps[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : default;
                }
                else
                {
                    if (parameters[i].IsDefined(typeof(DependencyInjectAttribute), true))
                    {
                        var attribute = parameters[i].GetCustomAttribute<DependencyInjectAttribute>();
                        var type = attribute.Type ?? parameters[i].ParameterType;
                        ps[i] = this.Resolve(type, attribute.Key);
                    }
                    else
                    {
                        ps[i] = this.Resolve(parameters[i].ParameterType, null);
                    }
                }
            }
        }

        var instance = ps.Length == 0 ? Activator.CreateInstance(toType) : Activator.CreateInstance(toType, ps);

        if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Property))
        {
            var propetys = toType.GetProperties().Where(x => x.IsDefined(typeof(DependencyInjectAttribute), true));
            foreach (var item in propetys)
            {
                if (item.CanWrite)
                {
                    object obj;
                    if (item.IsDefined(typeof(DependencyInjectAttribute), true))
                    {
                        var attribute = item.GetCustomAttribute<DependencyInjectAttribute>();
                        var type = attribute.Type ?? item.PropertyType;
                        obj = this.Resolve(type, attribute.Key);
                    }
                    else
                    {
                        obj = this.Resolve(item.PropertyType, null);
                    }
                    item.SetValue(instance, obj);
                }
            }
        }

        if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Method))
        {
            var methods = toType.GetMethods(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(DependencyInjectAttribute), true)).ToList();
            foreach (var item in methods)
            {
                parameters = item.GetParameters();
                ps = new object[parameters.Length];
                for (var i = 0; i < ps.Length; i++)
                {
                    if (parameters[i].ParameterType.IsPrimitive || parameters[i].ParameterType == typeof(string))
                    {
                        ps[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : default;
                    }
                    else
                    {
                        if (parameters[i].IsDefined(typeof(DependencyInjectAttribute), true))
                        {
                            var attribute = parameters[i].GetCustomAttribute<DependencyInjectAttribute>();
                            var type = attribute.Type ?? parameters[i].ParameterType;
                            ps[i] = this.Resolve(type, attribute.Key);
                        }
                        else
                        {
                            ps[i] = this.Resolve(parameters[i].ParameterType, null);
                        }
                    }
                }
                item.Invoke(instance, ps);
            }
        }
        descriptor.OnResolved?.Invoke(instance);
        return instance;
    }
}