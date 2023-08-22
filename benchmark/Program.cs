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
//------------------------------------------------------------------------------
using BenchmarkConsoleApp.Benchmark;
using BenchmarkDotNet.Running;
using System;
using TouchSocket.Core;

namespace BenchmarkConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            for (var i = 0; i < 100; i++)
            {
                using (var byteBlock = new ByteBlock(1024 * 64))
                {
                }
            }

            var consoleAction = new ConsoleAction();

            consoleAction.Add("0", "全部", () => { BenchmarkRunner.Run(typeof(Program).Assembly); });
            consoleAction.Add("1", "内存池", () => { BenchmarkRunner.Run<BenchmarkBytePool>(); });
            consoleAction.Add("2", "PluginsManager", () => { BenchmarkRunner.Run<BenchmarkPluginsManager>(); });
            consoleAction.Add("3", "BenchmarkType", () => { BenchmarkRunner.Run<BenchmarkType>(); });
            consoleAction.Add("4", "BenchmarkValueEnumerator", () => { BenchmarkRunner.Run<BenchmarkValueEnumerator>(); });
            consoleAction.Add("5", "CreateObject", () => { BenchmarkRunner.Run<BenchmarkNewCreateObject>(); });
            consoleAction.Add("6", "BenchmarkList", () => { BenchmarkRunner.Run<BenchmarkList>(); });
#if NETCOREAPP3_1_OR_GREATER
            consoleAction.Add("7", "序列化", () => { BenchmarkRunner.Run<BenchmarkSerialization>(); });
            consoleAction.Add("8", "结构体序列化", () => { BenchmarkRunner.Run<BenchmarkStructSerialization>(); });
            consoleAction.Add("9", "复杂序列化测试", () => { BenchmarkRunner.Run<BenchmarkSerializationComposite>(); });
#endif
            consoleAction.Add("10", "和SunnyUI测试", () => { BenchmarkRunner.Run<BenchmarkSunnyUI>(); });
            consoleAction.Add("11", "容器创建测试", () => { BenchmarkRunner.Run<BenchmarkContainer>(); });
            consoleAction.Add("12", "依赖属性性能", () => { BenchmarkRunner.Run<BenchmarkDependencyObject>(); });

            consoleAction.ShowAll();
            while (true)
            {
                consoleAction.Run(Console.ReadLine());
            }
        }
    }

    //[SimpleJob(RuntimeMoniker.Net461)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    //[SimpleJob(RuntimeMoniker.Net60)]
    //[MemoryDiagnoser]
    //public class SingleVsFirst
    //{
    //    public readonly List<string> _haystack = new List<string>();
    //    public readonly int _haystackSize = 1000000;
    //    public readonly string _needle = "needle";

    //    public SingleVsFirst()
    //    {
    //        //Add a large amount of items to our list.
    //        Enumerable.Range(1, _haystackSize).ToList().ForEach(x => _haystack.Add(x.ToString()));
    //        //Insert the needle right in the middle.
    //        _haystack.Insert(_haystackSize / 2, _needle);
    //    }

    //    [Benchmark]
    //    public string Single() => _haystack.SingleOrDefault(x => x == _needle);

    //    [Benchmark]
    //    public string First() => _haystack.FirstOrDefault(x => x == _needle);

    //    [Benchmark]
    //    public void Empty()
    //    {
    //    }

    //}
}