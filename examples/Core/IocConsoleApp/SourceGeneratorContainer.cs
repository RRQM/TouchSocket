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

using TouchSocket.Core;

namespace IocConsoleApp;

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