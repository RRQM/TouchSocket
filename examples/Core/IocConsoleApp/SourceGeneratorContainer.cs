using TouchSocket.Core;

namespace IocConsoleApp
{
    internal static class SourceGeneratorContainer
    {
        public static void Run()
        {
            var container = new MyContainer();
            var interface10 = container.Resolve<IInterface10>();
            var interface11 = container.Resolve<IInterface11>();
        }
    }

    [AddSingletonInject(typeof(IInterface10), typeof(MyClass10))]//直接添加现有类型为单例
    [AddTransientInject(typeof(IInterface11), typeof(MyClass11))]//直接添加现有类型为瞬态
    [GeneratorContainer]
    internal partial class MyContainer : ManualContainer
    {
    }

    internal interface IInterface10
    {
    }

    internal class MyClass10 : IInterface10
    {
    }

    internal interface IInterface11
    {
    }

    internal class MyClass11 : IInterface11
    {
    }

    [AutoInjectForSingleton]//将声明的类型直接标识为单例注入
    internal class MyClass12
    {
    }

    [AutoInjectForTransient]//将声明的类型直接标识为瞬态注入
    internal class MyClass13
    {
    }
}