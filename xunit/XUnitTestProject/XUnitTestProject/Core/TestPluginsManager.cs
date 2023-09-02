using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    public class TestPluginsManager
    {
        [Fact]
        public async Task RaiseAsyncShouldBeOk()
        {
            IPluginsManager pluginsManager = new PluginsManager(new Container())
            {
                Enable = true
            };

            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();

            var sender = new object();
            await pluginsManager.RaiseAsync(nameof(IMyPlugin.Run1), sender, new PluginEventArgs());

            foreach (var item in pluginsManager.Plugins.Cast<MyPluginClass1>())
            {
                Assert.True(item.Sender == sender);
                Assert.True(item.EnterCount == 1);
                Assert.True(item.LeaveCount == 1);
            }
        }

        [Fact]
        public void RaiseShouldBeOk()
        {
            IPluginsManager pluginsManager = new PluginsManager(new Container())
            {
                Enable = true
            };

            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass1>();

            var sender = new object();
            pluginsManager.Raise(nameof(IMyPlugin.Run1), sender, new PluginEventArgs());

            foreach (var item in pluginsManager.Plugins.Cast<MyPluginClass1>())
            {
                Assert.True(item.Sender == sender);
                Assert.True(item.EnterCount == 1);
                Assert.True(item.LeaveCount == 1);
            }
        }

        [Fact]
        public void RaiseGenericShouldBeOk()
        {
            GlobalEnvironment.OptimizedPlatforms = OptimizedPlatforms.Unity;
            IPluginsManager pluginsManager = new PluginsManager(new Container())
            {
                Enable = true
            };

            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass2>();
            pluginsManager.Add<MyPluginClass1>();
            pluginsManager.Add<MyPluginClass2>();

            var sender = 1;
            pluginsManager.Raise(nameof(IMyPlugin.Run1), sender, new PluginEventArgs());

            foreach (var item in pluginsManager.Plugins)
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

        private class MyPluginClass1 : PluginBase, IMyPlugin
        {
            public object Sender { get; set; }
            public int EnterCount { get; set; }
            public int LeaveCount { get; set; }

            async Task IMyPlugin<object>.Run1(object sender, PluginEventArgs e)
            {
                this.Sender = sender;
                this.EnterCount++;
                await e.InvokeNext();
                this.LeaveCount++;
            }
        }
    }
}