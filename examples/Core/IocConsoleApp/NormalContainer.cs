using System.Reflection;
using TouchSocket.Core;

namespace IocConsoleApp
{
    /// <summary>
    /// 常规Ioc容器，使用IL和反射实现，支持运行时注册和获取
    /// </summary>
    internal static class NormalContainer
    {
        public static void Run()
        {
            ConstructorInject();
            PropertyInject();
            MethodInject();
        }

        /// <summary>
        /// 构造函数注入
        /// </summary>
        private static void ConstructorInject()
        {
            var container = GetContainer();
            container.RegisterSingleton<MyClass1>();
            container.RegisterSingleton<MyClass2>();

            var myClass1 = container.Resolve<MyClass1>();
            var myClass2 = container.Resolve<MyClass2>();

            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// 属性注入
        /// </summary>
        private static void PropertyInject()
        {
            var container = GetContainer();
            container.RegisterSingleton<MyClass1>();
            container.RegisterSingleton<MyClass1>("key");
            container.RegisterSingleton<MyClass2>();

            container.RegisterSingleton<MyClass3>();

            var myClass3 = container.Resolve<MyClass3>();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// 方法注入
        /// </summary>
        private static void MethodInject()
        {
            var container = GetContainer();
            container.RegisterSingleton<MyClass1>();
            container.RegisterSingleton<MyClass4>();

            var myClass4 = container.Resolve<MyClass4>();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
        }

        private static IContainer GetContainer()
        {
            return new Container();//默认IOC容器

            //return new AspNetCoreContainer(new ServiceCollection());//使用Aspnetcore的容器
        }
    }

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

    internal class MyClass3
    {
        /// <summary>
        /// 直接按类型，默认方式获取
        /// </summary>
        [DependencyInject]
        public MyClass1 MyClass1 { get; set; }

        /// <summary>
        /// 获得指定类型的对象，然后赋值到object
        /// </summary>
        [DependencyInject(typeof(MyClass2))]
        public object MyClass2 { get; set; }

        /// <summary>
        /// 按照类型+Key获取
        /// </summary>
        [DependencyInject("key")]
        public MyClass1 KeyMyClass1 { get; set; }
    }

    internal class MyClass4
    {
        public MyClass1 MyClass1 { get; private set; }

        [DependencyInject]
        public void MethodInject(MyClass1 myClass1)
        {
            this.MyClass1 = myClass1;
        }
    }
}