using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using System;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net60)]
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
        }
        readonly Container m_touchIoc = new Container();
        readonly ServiceCollection m_services = new ServiceCollection();
        readonly IServiceProvider m_serviceProvider;

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
                var ins = this.m_services.BuildServiceProvider().GetService(typeof(MySingletonClass));
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
                var ins = this.m_services.BuildServiceProvider().GetService(typeof(MyTransientClass));
            }
        }
    }

    public class MySingletonClass
    {
        public int MyProperty { get; set; }
        public string MyProperty2 { get; set; }
    }

    public class MyTransientClass
    {
        public int MyProperty { get; set; }
        public string MyProperty2 { get; set; }
    }
}
