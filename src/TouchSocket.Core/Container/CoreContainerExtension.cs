// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Core;

/// <summary>
/// 容器扩展类，提供依赖注入容器的注册、解析等扩展方法
/// </summary>
public static class CoreContainerExtension
{
    #region RegisterSingleton

    /// <summary>
    /// 注册单例实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="instance">单例实例对象</param>
    public static void RegisterSingleton<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, TTo instance)
        where TFrom : class
        where TTo : class, TFrom
    {
        ThrowHelper.ThrowIfNull(instance, nameof(instance));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), instance));
    }

    /// <summary>
    /// 注册单例实例，使用实例的运行时类型作为服务类型
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="instance">单例实例对象</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] T>(this IRegistrator registrator, T instance)
        where T : class
    {
        ThrowHelper.ThrowIfNull(instance, nameof(instance));
        var instanceType = typeof(T);
        registrator.Register(new DependencyDescriptor(instanceType, instanceType, instance));
    }

    /// <summary>
    /// 注册单例服务，容器将自动创建实例
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    public static void RegisterSingleton(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        registrator.Register(new DependencyDescriptor(fromType, fromType, Lifetime.Singleton));
    }

    /// <summary>
    /// 注册单例实例，使用指定的键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键，用于区分同一类型的不同注册</param>
    /// <param name="instance">单例实例对象</param>
    public static void RegisterSingleton<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, object key, TTo instance)
        where TFrom : class
        where TTo : class, TFrom
    {
        ThrowHelper.ThrowIfNull(instance, nameof(instance));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), instance), key);
    }

    /// <summary>
    /// 注册单例实例，使用指定的服务类型和键
    /// </summary>
    /// <typeparam name="T">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="instance">单例实例对象</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] T>(this IRegistrator registrator, Type fromType, T instance, object key)
        where T : class
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(instance, nameof(instance));
        registrator.Register(new DependencyDescriptor(fromType, typeof(T), instance), key);
    }

    /// <summary>
    /// 注册单例实例，使用指定的服务类型
    /// </summary>
    /// <typeparam name="T">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="instance">单例实例对象</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] T>(this IRegistrator registrator, Type fromType, T instance)
        where T : class
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(instance, nameof(instance));
        registrator.Register(new DependencyDescriptor(fromType, typeof(T), instance));
    }

    /// <summary>
    /// 注册单例实例，使用指定的键
    /// </summary>
    /// <typeparam name="T">服务和实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="instance">单例实例对象</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] T>(this IRegistrator registrator, T instance, object key)
        where T : class
    {
        ThrowHelper.ThrowIfNull(instance, nameof(instance));
        registrator.Register(new DependencyDescriptor(typeof(T), typeof(T), instance), key);
    }

    /// <summary>
    /// 注册单例服务，容器将自动创建实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator)
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Singleton));
    }

    /// <summary>
    /// 注册单例服务映射，容器将自动创建实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    public static void RegisterSingleton<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator)
        where TFrom : class
        where TTo : class, TFrom
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Singleton));
    }

    /// <summary>
    /// 注册单例服务，容器将自动创建实例，使用指定的键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, object key)
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Singleton), key);
    }

    /// <summary>
    /// 注册单例服务映射，容器将自动创建实例，使用指定的键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, object key)
        where TFrom : class
        where TTo : class, TFrom
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Singleton), key);
    }

    /// <summary>
    /// 注册单例服务映射，容器将自动创建实例
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="toType">实现类型</param>
    public static void RegisterSingleton(this IRegistrator registrator, Type fromType, [DynamicallyAccessedMembers(AOT.Container)] Type toType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(toType, nameof(toType));
        registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Singleton));
    }

    /// <summary>
    /// 注册单例服务映射，使用指定的键，容器将自动创建实例
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="toType">实现类型</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton(this IRegistrator registrator, Type fromType, [DynamicallyAccessedMembers(AOT.Container)] Type toType, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(toType, nameof(toType));
        registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Singleton), key);
    }

    /// <summary>
    /// 注册单例服务，使用工厂方法创建实例
    /// </summary>
    /// <typeparam name="TType">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托，接收 <see cref="IResolver"/> 参数并返回服务实例</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] TType>(this IRegistrator registrator, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TType), Lifetime.Singleton)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册单例服务映射，使用工厂方法创建实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托，接收 <see cref="IResolver"/> 参数并返回服务实例</param>
    public static void RegisterSingleton<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Singleton)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册单例服务，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Singleton)
        {
            ImplementationFactory = func
        }, key);
    }

    /// <summary>
    /// 注册单例服务映射，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Singleton)
        {
            ImplementationFactory = func
        }, key);
    }

    /// <summary>
    /// 注册单例服务，使用工厂方法创建实例
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="func">工厂委托</param>
    public static void RegisterSingleton(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(fromType, Lifetime.Singleton)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册单例服务，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterSingleton(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(fromType, Lifetime.Singleton)
        {
            ImplementationFactory = func
        }, key);
    }

    #endregion RegisterSingleton

    #region RegisterScoped

    /// <summary>
    /// 注册作用域服务映射，在每个作用域内为单例，不同作用域之间是不同的实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTO">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    public static void RegisterScoped<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTO>(this IRegistrator registrator)
        where TFrom : class
        where TTO : class, TFrom
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTO), Lifetime.Scoped));
    }

    /// <summary>
    /// 注册作用域服务，在每个作用域内为单例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    public static void RegisterScoped<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator)
        where TFrom : class
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Scoped));
    }

    /// <summary>
    /// 注册作用域服务，使用指定的键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键</param>
    public static void RegisterScoped<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, object key)
        where TFrom : class
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Scoped), key);
    }

    /// <summary>
    /// 注册作用域服务映射，使用指定的键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTO">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键</param>
    public static void RegisterScoped<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTO>(this IRegistrator registrator, object key)
        where TFrom : class
        where TTO : class, TFrom
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTO), Lifetime.Scoped), key);
    }

    /// <summary>
    /// 注册作用域服务
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    public static void RegisterScoped(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        registrator.Register(new DependencyDescriptor(fromType, fromType, Lifetime.Scoped));
    }

    /// <summary>
    /// 注册作用域服务，使用指定的键
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="key">注册键</param>
    public static void RegisterScoped(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        registrator.Register(new DependencyDescriptor(fromType, fromType, Lifetime.Scoped), key);
    }

    /// <summary>
    /// 注册作用域服务映射
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="toType">实现类型</param>
    public static void RegisterScoped(this IRegistrator registrator, Type fromType, [DynamicallyAccessedMembers(AOT.Container)] Type toType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(toType, nameof(toType));
        registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Scoped));
    }

    /// <summary>
    /// 注册作用域服务映射，使用指定的键
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="toType">实现类型</param>
    /// <param name="key">注册键</param>
    public static void RegisterScoped(this IRegistrator registrator, Type fromType, [DynamicallyAccessedMembers(AOT.Container)] Type toType, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(toType, nameof(toType));
        registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Scoped), key);
    }

    /// <summary>
    /// 注册作用域服务，使用工厂方法创建实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    public static void RegisterScoped<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Scoped)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册作用域服务映射，使用工厂方法创建实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    public static void RegisterScoped<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Scoped)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册作用域服务，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterScoped<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Scoped)
        {
            ImplementationFactory = func
        }, key);
    }

    /// <summary>
    /// 注册作用域服务映射，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterScoped<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Scoped)
        {
            ImplementationFactory = func
        }, key);
    }

    /// <summary>
    /// 注册作用域服务，使用工厂方法创建实例
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="func">工厂委托</param>
    public static void RegisterScoped(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(fromType, Lifetime.Scoped)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册作用域服务，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterScoped(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(fromType, Lifetime.Scoped)
        {
            ImplementationFactory = func
        }, key);
    }

    #endregion RegisterScoped

    #region Transient

    /// <summary>
    /// 注册瞬态服务映射，每次解析都会创建新实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTO">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    public static void RegisterTransient<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTO>(this IRegistrator registrator)
        where TFrom : class
        where TTO : class, TFrom
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTO), Lifetime.Transient));
    }

    /// <summary>
    /// 注册瞬态服务，每次解析都会创建新实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    public static void RegisterTransient<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator)
        where TFrom : class
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Transient));
    }

    /// <summary>
    /// 注册瞬态服务，使用指定的键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键</param>
    public static void RegisterTransient<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, object key)
        where TFrom : class
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Transient), key);
    }

    /// <summary>
    /// 注册瞬态服务映射，使用指定的键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTO">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键</param>
    public static void RegisterTransient<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTO>(this IRegistrator registrator, object key)
        where TFrom : class
        where TTO : class, TFrom
    {
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTO), Lifetime.Transient), key);
    }

    /// <summary>
    /// 注册瞬态服务
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    public static void RegisterTransient(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        registrator.Register(new DependencyDescriptor(fromType, fromType, Lifetime.Transient));
    }

    /// <summary>
    /// 注册瞬态服务，使用指定的键
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="key">注册键</param>
    public static void RegisterTransient(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        registrator.Register(new DependencyDescriptor(fromType, fromType, Lifetime.Transient), key);
    }

    /// <summary>
    /// 注册瞬态服务映射
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="toType">实现类型</param>
    public static void RegisterTransient(this IRegistrator registrator, Type fromType, [DynamicallyAccessedMembers(AOT.Container)] Type toType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(toType, nameof(toType));
        registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Transient));
    }

    /// <summary>
    /// 注册瞬态服务映射，使用指定的键
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="toType">实现类型</param>
    /// <param name="key">注册键</param>
    public static void RegisterTransient(this IRegistrator registrator, Type fromType, [DynamicallyAccessedMembers(AOT.Container)] Type toType, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(toType, nameof(toType));
        registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Transient), key);
    }

    /// <summary>
    /// 注册瞬态服务，使用工厂方法创建实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    public static void RegisterTransient<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Transient)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册瞬态服务映射，使用工厂方法创建实例
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托，接收 <see cref="IResolver"/> 参数并返回服务实例</param>
    public static void RegisterTransient<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Transient)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册瞬态服务，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterTransient<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Transient)
        {
            ImplementationFactory = func
        }, key);
    }

    /// <summary>
    /// 注册瞬态服务映射，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <typeparam name="TFrom">服务类型</typeparam>
    /// <typeparam name="TTo">实现类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterTransient<TFrom, [DynamicallyAccessedMembers(AOT.Container)] TTo>(this IRegistrator registrator, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TTo), Lifetime.Transient)
        {
            ImplementationFactory = func
        }, key);
    }

    /// <summary>
    /// 注册瞬态服务，使用工厂方法创建实例
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="func">工厂委托</param>
    public static void RegisterTransient(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, Func<IResolver, object> func)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(fromType, Lifetime.Transient)
        {
            ImplementationFactory = func
        });
    }

    /// <summary>
    /// 注册瞬态服务，使用工厂方法创建实例，并指定键
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">服务类型</param>
    /// <param name="func">工厂委托</param>
    /// <param name="key">注册键</param>
    public static void RegisterTransient(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, Func<IResolver, object> func, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        ThrowHelper.ThrowIfNull(func, nameof(func));
        registrator.Register(new DependencyDescriptor(fromType, Lifetime.Transient)
        {
            ImplementationFactory = func
        }, key);
    }

    #endregion Transient

    #region Unregister

    /// <summary>
    /// 从容器中移除指定类型的注册信息
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">要移除的服务类型</param>
    public static void Unregister(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        registrator.Unregister(new DependencyDescriptor(fromType));
    }

    /// <summary>
    /// 从容器中移除指定类型和键的注册信息
    /// </summary>
    /// <param name="registrator">容器注册器</param>
    /// <param name="fromType">要移除的服务类型</param>
    /// <param name="key">注册键</param>
    public static void Unregister(this IRegistrator registrator, [DynamicallyAccessedMembers(AOT.Container)] Type fromType, object key)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));
        registrator.Unregister(new DependencyDescriptor(fromType), key);
    }

    /// <summary>
    /// 从容器中移除指定类型的注册信息
    /// </summary>
    /// <typeparam name="TFrom">要移除的服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    public static void Unregister<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator)
    {
        registrator.Unregister(new DependencyDescriptor(typeof(TFrom)));
    }

    /// <summary>
    /// 从容器中移除指定类型和键的注册信息
    /// </summary>
    /// <typeparam name="TFrom">要移除的服务类型</typeparam>
    /// <param name="registrator">容器注册器</param>
    /// <param name="key">注册键</param>
    public static void Unregister<[DynamicallyAccessedMembers(AOT.Container)] TFrom>(this IRegistrator registrator, object key)
    {
        registrator.Unregister(new DependencyDescriptor(typeof(TFrom)), key);
    }

    #endregion Unregister

    #region Resolve

    /// <summary>
    /// 从容器中解析指定类型的服务实例
    /// </summary>
    /// <typeparam name="T">要解析的服务类型</typeparam>
    /// <param name="resolver">服务提供程序</param>
    /// <returns>解析的服务实例，如果解析失败则返回 <see langword="null"/></returns>
    public static T Resolve<[DynamicallyAccessedMembers(AOT.Container)] T>(this IServiceProvider resolver)
    {
        return (T)resolver.GetService(typeof(T));
    }

    /// <summary>
    /// 从容器中解析指定类型和键的服务实例
    /// </summary>
    /// <typeparam name="T">要解析的服务类型</typeparam>
    /// <param name="resolver">服务解析器</param>
    /// <param name="key">注册键</param>
    /// <returns>解析的服务实例，如果解析失败则返回 <see langword="null"/></returns>
    public static T Resolve<[DynamicallyAccessedMembers(AOT.Container)] T>(this IResolver resolver, object key)
    {
        return (T)resolver.Resolve(typeof(T), key);
    }

    /// <summary>
    /// 创建未在容器中注册的类型实例，但会使用容器解析其构造函数参数
    /// </summary>
    /// <remarks>
    /// 此方法适用于目标类型本身未注册，但其构造函数参数类型已在容器中注册的场景。
    /// 创建的实例具有 <see cref="Lifetime.Transient"/> 生命周期特性。
    /// </remarks>
    /// <param name="resolver">服务提供程序</param>
    /// <param name="fromType">要创建实例的目标类型</param>
    /// <returns>创建的实例</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="fromType"/> 为 <see langword="null"/> 时抛出</exception>
    /// <exception cref="InvalidOperationException">当类型没有公共构造函数时抛出</exception>
    public static object ResolveWithoutRoot(this IServiceProvider resolver, [DynamicallyAccessedMembers(AOT.Container)] Type fromType)
    {
        ThrowHelper.ThrowIfNull(fromType, nameof(fromType));

        var constructors = fromType.GetConstructors();
        if (constructors.Length == 0)
        {
            throw new InvalidOperationException($"类型 {fromType.FullName} 没有公共构造函数");
        }

        var ctor = SelectConstructorForResolve(constructors);
        var parameters = ctor.GetParameters();

        if (parameters.Length == 0)
        {
            return Activator.CreateInstance(fromType);
        }

        var ps = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;

            if (paramType.IsPrimitive || paramType == typeof(string))
            {
                ps[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : default;
            }
            else
            {
                ps[i] = resolver.GetService(paramType);
            }
        }

        return Activator.CreateInstance(fromType, ps);
    }

    /// <summary>
    /// 创建未在容器中注册的类型实例，但会使用容器解析其构造函数参数
    /// </summary>
    /// <typeparam name="T">要创建实例的目标类型</typeparam>
    /// <param name="resolver">服务提供程序</param>
    /// <returns>创建的实例</returns>
    /// <exception cref="InvalidOperationException">当类型没有公共构造函数时抛出</exception>
    public static T ResolveWithoutRoot<[DynamicallyAccessedMembers(AOT.Container)] T>(this IServiceProvider resolver)
    {
        return (T)ResolveWithoutRoot(resolver, typeof(T));
    }

    /// <summary>
    /// 为 <see cref="ResolveWithoutRoot(IServiceProvider, Type)"/> 方法选择最适合的构造函数
    /// </summary>
    /// <remarks>
    /// 选择逻辑：
    /// <list type="number">
    /// <item>优先选择标记了 <see cref="DependencyInjectAttribute"/> 的构造函数</item>
    /// <item>如果有多个标记的构造函数，选择参数最多的</item>
    /// <item>如果没有标记的构造函数，选择所有构造函数中参数最多的</item>
    /// </list>
    /// </remarks>
    /// <param name="constructors">候选构造函数数组</param>
    /// <returns>选中的构造函数</returns>
    private static System.Reflection.ConstructorInfo SelectConstructorForResolve(System.Reflection.ConstructorInfo[] constructors)
    {
        var attributedConstructors = constructors
            .Where(c => c.GetCustomAttributes(typeof(DependencyInjectAttribute), true).Length > 0)
            .ToList();

        if (attributedConstructors.Count > 0)
        {
            return attributedConstructors.OrderByDescending(c => c.GetParameters().Length).First();
        }

        return constructors.OrderByDescending(c => c.GetParameters().Length).First();
    }

    #endregion Resolve

    #region IsRegistered

    /// <summary>
    /// 检查指定类型是否已在容器中注册
    /// </summary>
    /// <typeparam name="T">要检查的服务类型</typeparam>
    /// <param name="registered">注册检查器</param>
    /// <returns>如果类型已注册则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public static bool IsRegistered<[DynamicallyAccessedMembers(AOT.Container)] T>(this IRegistered registered)
    {
        return registered.IsRegistered(typeof(T));
    }

    /// <summary>
    /// 检查指定类型和键是否已在容器中注册
    /// </summary>
    /// <typeparam name="T">要检查的服务类型</typeparam>
    /// <param name="registered">注册检查器</param>
    /// <param name="key">注册键</param>
    /// <returns>如果类型和键的组合已注册则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public static bool IsRegistered<[DynamicallyAccessedMembers(AOT.Container)] T>(this IRegistered registered, object key)
    {
        return registered.IsRegistered(typeof(T), key);
    }

    #endregion IsRegistered
}