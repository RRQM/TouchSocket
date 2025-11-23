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

using System.Reflection;
using TouchSocket.Core;

namespace IocConsoleApp;

/// <summary>
/// 常规Ioc容器，使用IL和反射实现，支持运行时注册和获取
/// </summary>
internal static class NormalContainer
{
    public static void Run()
    {
        ConstructorInject();
        SingletonLifetime();
        TransientLifetime();
        ScopedLifetime();
        RegisterWithKey();
        RegisterWithFactory();
    }

    /// <summary>
    /// 构造函数注入
    /// </summary>
    private static void ConstructorInject()
    {
        #region IOC构造函数注入
        var container = new Container();
        container.RegisterSingleton<MyClass1>();
        container.RegisterSingleton<MyClass2>();

        var myClass1 = container.Resolve<MyClass1>();
        var myClass2 = container.Resolve<MyClass2>();
        #endregion

        Console.WriteLine(MethodBase.GetCurrentMethod().Name);
    }

    /// <summary>
    /// 单例生命周期
    /// </summary>
    private static void SingletonLifetime()
    {
        #region IOC单例生命周期
        var container = new Container();
        container.RegisterSingleton<MyClass1>();

        var instance1 = container.Resolve<MyClass1>();
        var instance2 = container.Resolve<MyClass1>();
        // instance1 和 instance2 是同一个实例
        Console.WriteLine($"Singleton: {ReferenceEquals(instance1, instance2)}"); // True
        #endregion

        Console.WriteLine(MethodBase.GetCurrentMethod().Name);
    }

    /// <summary>
    /// 瞬态生命周期
    /// </summary>
    private static void TransientLifetime()
    {
        #region IOC瞬态生命周期
        var container = new Container();
        container.RegisterTransient<MyClass1>();

        var instance1 = container.Resolve<MyClass1>();
        var instance2 = container.Resolve<MyClass1>();
        // instance1 和 instance2 是不同的实例
        Console.WriteLine($"Transient: {ReferenceEquals(instance1, instance2)}"); // False
        #endregion

        Console.WriteLine(MethodBase.GetCurrentMethod().Name);
    }

    /// <summary>
    /// 作用域生命周期
    /// </summary>
    private static void ScopedLifetime()
    {
        #region IOC作用域生命周期
        var container = new Container();
        container.RegisterScoped<MyClass1>();

        // 在同一作用域内是同一实例
        using (var scope = container.CreateScopedResolver())
        {
            var instance1 = scope.Resolver.Resolve<MyClass1>();
            var instance2 = scope.Resolver.Resolve<MyClass1>();
            Console.WriteLine($"Scoped (same scope): {ReferenceEquals(instance1, instance2)}"); // True
        }

        // 在不同作用域内是不同实例
        using (var scope1 = container.CreateScopedResolver())
        using (var scope2 = container.CreateScopedResolver())
        {
            var instance1 = scope1.Resolver.Resolve<MyClass1>();
            var instance2 = scope2.Resolver.Resolve<MyClass1>();
            Console.WriteLine($"Scoped (different scopes): {ReferenceEquals(instance1, instance2)}"); // False
        }
        #endregion

        Console.WriteLine(MethodBase.GetCurrentMethod().Name);
    }

    /// <summary>
    /// 使用Key注册
    /// </summary>
    private static void RegisterWithKey()
    {
        #region IOC使用Key注册
        var container = new Container();
        container.RegisterSingleton<MyClass1>();
        container.RegisterSingleton<MyClass1>("specialKey");

        var defaultInstance = container.Resolve<MyClass1>();
        var keyedInstance = container.Resolve<MyClass1>("specialKey");
        // 不同的注册，不同的实例
        Console.WriteLine($"Keyed: {ReferenceEquals(defaultInstance, keyedInstance)}"); // False
        #endregion

        Console.WriteLine(MethodBase.GetCurrentMethod().Name);
    }

    /// <summary>
    /// 使用工厂方法注册
    /// </summary>
    private static void RegisterWithFactory()
    {
        #region IOC使用工厂方法注册
        var container = new Container();
        container.RegisterSingleton<MyClass1>(resolver =>
        {
            Console.WriteLine("Creating MyClass1 using factory");
            return new MyClass1();
        });

        var instance = container.Resolve<MyClass1>();
        #endregion

        Console.WriteLine(MethodBase.GetCurrentMethod().Name);
    }
}

#region IOC定义类型
internal class MyClass1
{
}

internal class MyClass2
{
    public MyClass2(MyClass1 myClass1)
    {
        this.MyClass1 = myClass1;
    }

    public MyClass1 MyClass1 { get; }
}
#endregion