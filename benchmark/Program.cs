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
            var consoleAction = new ConsoleAction();

            consoleAction.Add("0", "全部", () => { BenchmarkRunner.Run(typeof(Program).Assembly); });
            consoleAction.Add("1", "内存池", () => { BenchmarkRunner.Run<BenchmarkBytePool>(); });
            consoleAction.Add("2", "PluginManager", () => { BenchmarkRunner.Run<BenchmarkPluginManager>(); });
            consoleAction.Add("3", "BenchmarkType", () => { BenchmarkRunner.Run<BenchmarkType>(); });
#if NET6_0_OR_GREATER
            consoleAction.Add("4", "BenchmarkValueEnumerator", () => { BenchmarkRunner.Run<BenchmarkValueEnumerator>(); });
#endif
            consoleAction.Add("5", "CreateObject", () => { BenchmarkRunner.Run<BenchmarkNewCreateObject>(); });
            consoleAction.Add("6", "BenchmarkList", () => { BenchmarkRunner.Run<BenchmarkList>(); });
#if NET6_0_OR_GREATER
            consoleAction.Add("7", "序列化", () => { BenchmarkRunner.Run<BenchmarkSerialization>(); });
            consoleAction.Add("8", "结构体序列化", () => { BenchmarkRunner.Run<BenchmarkStructSerialization>(); });
            consoleAction.Add("9", "复杂序列化测试", () => { BenchmarkRunner.Run<BenchmarkSerializationComposite>(); });
#endif
            consoleAction.Add("10", "容器创建测试", () => { BenchmarkRunner.Run<BenchmarkContainer>(); });
            consoleAction.Add("11", "依赖属性性能", () => { BenchmarkRunner.Run<BenchmarkDependencyObject>(); });
            consoleAction.Add("12", "方法调用", () => { BenchmarkRunner.Run<BenchmarkInvokeMethod>(); });
            consoleAction.Add("13", "Ref传递", () => { BenchmarkRunner.Run<BenchmarkRefParameter>(); });
            consoleAction.Add("14", "Lazy测试", () => { BenchmarkRunner.Run<BenchmarkLazy>(); });
            consoleAction.Add("15", "字符串匹配", () => { BenchmarkRunner.Run<BenchmarkStringSwitch>(); });
#if NET8_0_OR_GREATER
            consoleAction.Add("16", "不安全私有访问", () => { BenchmarkRunner.Run<BenchmarkAccess>(); });
#endif
            consoleAction.ShowAll();
            while (true)
            {
                consoleAction.Run(Console.ReadLine());
            }
        }
    }
}