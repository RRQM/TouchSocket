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
using System.Reflection;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkPluginManager : BenchmarkBase
    {
        private IPluginManager m_plugins1;
        private IPluginManager m_plugins2;
        private Method method;
        private int plugCount = 10;
        private MethodInfo m_methodInfo;

        public BenchmarkPluginManager()
        {
            //this.Count = 1;

            this.m_plugins1 = new PluginManager(new Container())
            {
                Enable = true
            };
            for (var i = 0; i < this.plugCount; i++)
            {
                this.m_plugins1.Add(new MyPlugin());
            }

            this.m_plugins2 = new PluginManager(new Container())
            {
                Enable = true
            };
            for (var i = 0; i < this.plugCount; i++)
            {
                this.m_plugins2.Add(nameof(MyPlugin.Test), this.Test);
            }
            this.m_methodInfo = typeof(MyPlugin).GetMethod("Test");
            this.method = new Method(this.m_methodInfo);
        }

        private Task Test(object sender, PluginEventArgs e)
        {
            return e.InvokeNext();
        }

        private interface MyPluginInterface : IPlugin
        {
            Task Test(object sender, PluginEventArgs e);
        }

        [Benchmark(Baseline = true)]
        public void DirectRun()
        {
            var myPlugin = new MyPlugin();
            for (var i = 0; i < this.Count; i++)
            {
                var sender = new object();
                var e = new PluginEventArgs();
                var ps = new object[] { sender, e };
                for (var j = 0; j < this.plugCount; j++)
                {
                    myPlugin.Test(sender, e);
                }
            }
        }

        [Benchmark]
        public void ActionRun()
        {
            var myPlugin = new MyPlugin();
            Func<object, PluginEventArgs, Task> func = myPlugin.Test;
            for (var i = 0; i < this.Count; i++)
            {
                var sender = new object();
                var e = new PluginEventArgs();
                var ps = new object[] { sender, e };
                for (var j = 0; j < this.plugCount; j++)
                {
                    func.Invoke(sender, e);
                }
            }
        }

        [Benchmark]
        public void MethodInfoRun()
        {
            var myPlugin = new MyPlugin();
            for (var i = 0; i < this.Count; i++)
            {
                var sender = new object();
                var e = new PluginEventArgs();
                var ps = new object[] { sender, e };
                for (var j = 0; j < this.plugCount; j++)
                {
                    this.m_methodInfo.Invoke(myPlugin, ps);
                }
            }
        }

        [Benchmark]
        public void ExpressionRun()
        {
            var myPlugin = new MyPlugin();

            for (var i = 0; i < this.Count; i++)
            {
                var sender = new object();
                var e = new PluginEventArgs();
                var ps = new object[] { sender, e };
                for (var j = 0; j < this.plugCount; j++)
                {
                    this.method.Invoke(myPlugin, sender, e);
                }
            }
        }

        [Benchmark]
        public void PluginRun()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.m_plugins1.Raise(nameof(MyPluginInterface.Test), new object(), new PluginEventArgs());
            }
        }

        [Benchmark]
        public void PluginActionRun()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.m_plugins2.Raise(nameof(MyPluginInterface.Test), new object(), new PluginEventArgs());
            }
        }

        [Benchmark]
        public async Task PluginActionRunAsync()
        {
            for (var i = 0; i < this.Count; i++)
            {
                await this.m_plugins2.RaiseAsync(nameof(MyPluginInterface.Test), new object(), new PluginEventArgs());
            }
        }

        private class MyPlugin : MyPluginInterface
        {
            public int Order { get; set; }

            public void Dispose()
            {
            }

            public void Loaded(IPluginManager pluginManager)
            {
            }

            public Task Test(object sender, PluginEventArgs e)
            {
                return e.InvokeNext();
            }
        }
    }
}