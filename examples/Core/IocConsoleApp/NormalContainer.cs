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