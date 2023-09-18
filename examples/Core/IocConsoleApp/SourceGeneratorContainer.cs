using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace IocConsoleApp
{
    internal static class SourceGeneratorContainer
    {
        public static void Run()
        {
            MyContainer container = new MyContainer();
            var interface10 = container.Resolve<IInterface10>();
            var interface11 = container.Resolve<IInterface11>();
        }
    }

    [AddSingletonInject(typeof(IInterface10), typeof(MyClass10))]//直接添加现有类型为单例
    [AddTransientInject(typeof(IInterface11), typeof(MyClass11))]//直接添加现有类型为瞬态
    [GeneratorContainer]
    partial class MyContainer : ManualContainer
    {

    }
    interface IInterface10
    {

    }
    class MyClass10 : IInterface10
    {

    }

    interface IInterface11
    {

    }
    class MyClass11 : IInterface11
    {

    }

    [AutoInjectForSingleton]//将声明的类型直接标识为单例注入
    class MyClass12
    {

    }

    [AutoInjectForTransient]//将声明的类型直接标识为瞬态注入
    class MyClass13
    {

    }
}
