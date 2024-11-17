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

#if NET6_0_OR_GREATER
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Linq;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class BenchmarkValueEnumerator : BenchmarkBase
    {
        private int[] list;
        public BenchmarkValueEnumerator()
        {
            this.list = Enumerable.Range(0, 10000).ToArray();
        }
        [Benchmark]
        public void RunFor()
        {
            for (var j = 0; j < this.Count; j++)
            {
                for (var i = 0; i < this.list.Length; i++)
                {
                    var value = this.list[i];
                }
            }
        }

        [Benchmark]
        public void RunEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                foreach (var item in this.list)
                {

                }
            }

        }

        [Benchmark]
        public void RunRangeEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                foreach (var item in this.list[0..this.list.Length])
                {

                }
            }
        }
    }
}


#endif