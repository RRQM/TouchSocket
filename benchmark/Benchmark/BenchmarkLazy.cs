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
using System;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkLazy : BenchmarkBase
    {
        public BenchmarkLazy()
        {
            this.Count = 1000000;
        }

        private Lazy<MyBenchmarkLazyClass> m_lazyClass;
        private MyBenchmarkLazyClass m_class;
        private MyBenchmarkLazyClass m_class1;

        [Benchmark]
        public void Lazy()
        {
            this.m_lazyClass = new Lazy<MyBenchmarkLazyClass>(() => new MyBenchmarkLazyClass());
            for (var i = 0; i < this.Count; i++)
            {
                var v = this.m_lazyClass.Value;
            }
        }

        [Benchmark]
        public void Direct()
        {
            this.m_class = new MyBenchmarkLazyClass();
            for (var i = 0; i < this.Count; i++)
            {
                var v = this.m_class;
            }
        }

        private object root = new object();

        [Benchmark]
        public void DirectLock()
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this.m_class1 == null)
                {
                    lock (this.root)
                    {
                        if (this.m_class1 != null)
                        {
                            this.m_class1 = new MyBenchmarkLazyClass();
                        }
                    }
                }
                var v = this.m_class1;
            }
        }
    }

    internal class MyBenchmarkLazyClass
    {
    }
}