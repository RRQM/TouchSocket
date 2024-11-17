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

namespace XUnitTestProject.Core
{
    
    public class TestPluginManager
    {
        [Fact]
        public async Task RaiseAsyncShouldBeOk()
        {
            IPluginManager PluginManager = new PluginManager(new Container())
            {
                Enable = true
            };

            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();

            var sender = new object();
            await PluginManager.RaiseAsync(nameof(IMyPlugin.Run1), sender, new PluginEventArgs());

            foreach (var item in PluginManager.Plugins.Cast<MyPluginClass1>())
            {
                Assert.True(item.Sender == sender);
                Assert.True(item.EnterCount == 1);
                Assert.True(item.LeaveCount == 1);
            }
        }

        [Fact]
        public void RaiseShouldBeOk()
        {
            IPluginManager PluginManager = new PluginManager(new Container())
            {
                Enable = true
            };

            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass1>();

            var sender = new object();
            PluginManager.Raise(nameof(IMyPlugin.Run1), sender, new PluginEventArgs());

            foreach (var item in PluginManager.Plugins.Cast<MyPluginClass1>())
            {
                Assert.True(item.Sender == sender);
                Assert.True(item.EnterCount == 1);
                Assert.True(item.LeaveCount == 1);
            }
        }

        [Fact]
        public void RaiseGenericShouldBeOk()
        {
            GlobalEnvironment.DynamicBuilderType = DynamicBuilderType.Reflect;
            IPluginManager PluginManager = new PluginManager(new Container())
            {
                Enable = true
            };

            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass2>();
            PluginManager.Add<MyPluginClass1>();
            PluginManager.Add<MyPluginClass2>();

            var sender = 1;
            PluginManager.Raise(nameof(IMyPlugin.Run1), sender, new PluginEventArgs());

            foreach (var item in PluginManager.Plugins)
            {
                if (item is MyPluginClass1 class1)
                {
                    Assert.True((int)class1.Sender == sender);
                    Assert.True(class1.EnterCount == 1);
                    Assert.True(class1.LeaveCount == 1);
                }
                else if (item is MyPluginClass2 class2)
                {
                    Assert.True(class2.Sender == sender);
                    Assert.True(class2.EnterCount == 1);
                    Assert.True(class2.LeaveCount == 1);
                }
            }
        }

        [PluginOption(Singleton = false)]
        class MyPluginClass2 : PluginBase, IMyPlugin<int>
        {
            public int Sender { get; set; }
            public int EnterCount { get; set; }
            public int LeaveCount { get; set; }

            public async Task Run1(int sender, PluginEventArgs e)
            {
                this.Sender = sender;
                this.EnterCount++;
                await e.InvokeNext();
                this.LeaveCount++;
            }
        }

        private interface IMyPlugin<TClient> : IPlugin
        {
            Task Run1(TClient sender, PluginEventArgs e);
        }

        interface IMyPlugin : IMyPlugin<object>
        {
        }

        [PluginOption( Singleton =false)]
        private class MyPluginClass1 : PluginBase, IMyPlugin
        {
            public object Sender { get; set; }
            public int EnterCount { get; set; }
            public int LeaveCount { get; set; }

            public async Task Run1(object sender, PluginEventArgs e)
            {
                this.Sender = sender;
                this.EnterCount++;
                await e.InvokeNext();
                this.LeaveCount++;
            }
        }
    }
}