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
using System.Collections;
using System.Collections.Generic;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkList : BenchmarkBase
    {
        [Benchmark]
        public void ListAdd()
        {
            var myListClass = new MyListClass();
            var list = new List<MyListClass>();
            for (var i = 0; i < this.Count; i++)
            {
                list.Add(myListClass);
            }
        }

        [Benchmark]
        public void ArrayListAdd()
        {
            var myListClass = new MyListClass();
            var list = new ArrayList();
            for (var i = 0; i < this.Count; i++)
            {
                list.Add(myListClass);
            }
        }
    }

    internal class MyListClass
    {
    }
}