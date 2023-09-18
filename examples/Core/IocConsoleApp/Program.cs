using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TouchSocket.Core;
using TouchSocket.Core.AspNetCore;

namespace IocConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConstructorInject();
            PropertyInject();
            MethodInject();
            DefaultParmaterInject();
            Console.ReadKey();
        }

        /// <summary>
        /// 构造函数注入
        /// </summary>
        static void ConstructorInject()
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
        static void PropertyInject()
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
        static void MethodInject()
        {
            var container = GetContainer();
            container.RegisterSingleton<MyClass1>();
            container.RegisterSingleton<MyClass4>();

            var myClass4 = container.Resolve<MyClass4>();
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// 默认参数注入
        /// </summary>
        static void DefaultParmaterInject()
        {
            var container = GetContainer();
            container.RegisterSingleton<MyClass1>();
            container.RegisterTransient<MyClass5>();//默认参数只在瞬时生命有效

            var myClass5_1 = container.Resolve<MyClass5>(new object[] { 10, "hello" });
            var myClass5_2 = container.Resolve<MyClass5>(new object[] { 20, "hi" });
            var myClass5_3 = container.Resolve<MyClass5>(new object[] { 20, "hi", new MyClass1() });

            //以下结果为true，因为在构建时并未覆盖MyClass1，所以MyClass1只能从容器获得，而MyClass1在容器是以单例注册，所以相同
            var b1 = myClass5_1.MyClass1 == myClass5_2.MyClass1;

            //以下结果为false，因为在构建时覆盖MyClass1，所以MyClass1是从ps参数直接传递的，所以不相同
            var b2 = myClass5_1.MyClass1 == myClass5_3.MyClass1;
        }

        static IContainer GetContainer()
        {
            return new Container();//默认IOC容器

            //return new AspNetCoreContainer(new ServiceCollection());//使用Aspnetcore的容器
        }
    }


    class MyClass1
    {

    }

    class MyClass2
    {
        public MyClass2(MyClass1 myClass1)
        {
            this.MyClass1 = myClass1;
        }

        public MyClass1 MyClass1 { get; }
    }

    class MyClass3
    {
        /// <summary>
        /// 直接按类型，默认方式获取
        /// </summary>
        [DependencyInject]
        public MyClass1 MyClass1 { get; set; }

        /// <summary>
        /// 获得指定类型的对象，然后赋值到object
        /// </summary>
        [DependencyParamterInject(typeof(MyClass2))]
        public object MyClass2 { get; set; }

        /// <summary>
        /// 按照类型+Key获取
        /// </summary>
        [DependencyParamterInject("key")]
        public MyClass1 KeyMyClass1 { get; set; }
    }

    class MyClass4
    {
        public MyClass1 MyClass1 { get; private set; }


        [DependencyInject]
        public void MethodInject(MyClass1 myClass1)
        {
            this.MyClass1 = myClass1;
        }
    }

    class MyClass5
    {
        public MyClass5(int a, string b, MyClass1 myClass1)
        {
            this.A = a;
            this.B = b;
            this.MyClass1 = myClass1;
        }

        public int A { get; }
        public string B { get; }
        public MyClass1 MyClass1 { get; }
    }
}