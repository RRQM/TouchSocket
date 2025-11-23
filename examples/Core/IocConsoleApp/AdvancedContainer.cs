//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议,若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using TouchSocket.Core;
using TouchSocket.Core.AspNetCore;

namespace IocConsoleApp;

/// <summary>
/// 高级IOC容器示例
/// </summary>
internal static class AdvancedContainer
{
    public static void Run()
    {
        UseAspNetCoreContainer();
        UseAspNetCoreContainerWithConfig();
        InterfaceMapping();
        GenericTypeRegistration();
    }

    private static void UseAspNetCoreContainer()
    {
        #region IOC使用AspNetCoreContainer
        var aspNetCoreContainer = new AspNetCoreContainer(new ServiceCollection());
        aspNetCoreContainer.RegisterSingleton<MyClass1>();
        aspNetCoreContainer.RegisterSingleton<MyClass2>();

        aspNetCoreContainer.BuildResolver();

        var myClass1 = aspNetCoreContainer.Resolve<MyClass1>();
        var myClass2 = aspNetCoreContainer.Resolve<MyClass2>();
        #endregion

        Console.WriteLine("UseAspNetCoreContainer completed");
    }

    private static void UseAspNetCoreContainerWithConfig()
    {
        #region IOC配置中使用ServiceCollection
        var config = new TouchSocketConfig()
            .UseAspNetCoreContainer(new ServiceCollection()); //ServiceCollection可以使用现有的
        #endregion

        Console.WriteLine("UseAspNetCoreContainerWithConfig completed");
    }

    private static void InterfaceMapping()
    {
        #region IOC接口映射注册
        var container = new Container();
        // 将接口映射到实现类
        container.RegisterSingleton<IMyService, MyServiceImpl>();

        var service = container.Resolve<IMyService>();
        service.DoSomething();
        #endregion

        Console.WriteLine("InterfaceMapping completed");
    }

    private static void GenericTypeRegistration()
    {
        #region IOC泛型类型注册
        var container = new Container();
        // 注册泛型类型定义（使用瞬态避免单例冲突）
        container.RegisterTransient(typeof(IRepository<>), typeof(Repository<>));

        // 解析具体的泛型类型
        var userRepo = (IRepository<User>)container.Resolve(typeof(IRepository<User>));
        var productRepo = (IRepository<Product>)container.Resolve(typeof(IRepository<Product>));

        userRepo.Save(new User { Name = "Alice" });
        productRepo.Save(new Product { Name = "Book" });
        #endregion

        Console.WriteLine("GenericTypeRegistration completed");
    }
}

#region IOC高级示例类型定义
internal interface IMyService
{
    void DoSomething();
}

internal class MyServiceImpl : IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("MyServiceImpl.DoSomething");
    }
}

internal interface IRepository<T>
{
    void Save(T entity);
}

internal class Repository<T> : IRepository<T>
{
    public void Save(T entity)
    {
        Console.WriteLine($"Saving {typeof(T).Name}: {entity}");
    }
}

internal class User
{
    public string Name { get; set; }
    public override string ToString() => Name;
}

internal class Product
{
    public string Name { get; set; }
    public override string ToString() => Name;
}
#endregion
