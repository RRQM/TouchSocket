//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using System;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkContainer : BenchmarkBase
    {
        public BenchmarkContainer()
        {
            this.m_touchIoc.RegisterSingleton<MySingletonClass>();
            this.m_touchIoc.RegisterTransient<MyTransientClass>();

            this.m_services.AddSingleton<MySingletonClass>();
            this.m_services.AddTransient<MyTransientClass>();

            this.m_serviceProvider = this.m_services.BuildServiceProvider();
            this.m_container = new MyContainer();
        }

        private readonly Container m_touchIoc = new Container();
        private readonly ServiceCollection m_services = new ServiceCollection();
        private readonly IServiceProvider m_serviceProvider;
        private readonly MyContainer m_container;

        [Benchmark]
        public void TouchCreateSingleton()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var ins = this.m_touchIoc.Resolve(typeof(MySingletonClass));
            }
        }

        [Benchmark]
        public void AspCreateSingleton()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var ins = this.m_serviceProvider.GetService(typeof(MySingletonClass));
            }
        }

        [Benchmark]
        public void TouchCreateSingletonForGenerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var ins = this.m_container.Resolve(typeof(MySingletonClass));
            }
        }

        [Benchmark]
        public void TouchCreateTransient()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var ins = this.m_touchIoc.Resolve(typeof(MyTransientClass));
            }
        }

        [Benchmark]
        public void AspCreateTransient()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var ins = this.m_serviceProvider.GetService(typeof(MyTransientClass));
            }
        }

        [Benchmark]
        public void TouchCreateTransientForGenerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var ins = this.m_container.Resolve(typeof(MyTransientClass));
            }
        }
    }

    [AddSingletonInject(typeof(MySingletonClass))]
    [GeneratorContainer]
    internal sealed partial class MyContainer : ManualContainer
    {
    }

    [AutoInjectForSingleton]
    public class MySingletonClass
    {
        public int MyProperty { get; set; }
        public string MyProperty2 { get; set; }
    }

    [AutoInjectForTransient]
    public class MyTransientClass
    {
        public int MyProperty { get; set; }
        public string MyProperty2 { get; set; }
    }
}