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

            pluginsManager.Add<MyPluginClass>();
            pluginsManager.Add<MyPluginClass>();
            pluginsManager.Add<MyPluginClass>();
            pluginsManager.Add<MyPluginClass>();
            pluginsManager.Add<MyPluginClass>();

            var sender = new object();
            await pluginsManager.RaiseAsync(nameof(IMyPlugin.Run1), sender, new PluginEventArgs());

            foreach (MyPluginClass item in pluginsManager.Plugins)
            {
                Assert.True(item.Sender == sender);
                Assert.True(item.Count == 2);
            }
        }

        private interface IMyPlugin : IPlugin
        {
            Task Run1(object sender, PluginEventArgs e);
        }

        private class MyPluginClass : PluginBase, IMyPlugin
        {
            public object Sender { get; set; }
            public int Count { get; set; }

            public async Task Run1(object sender, PluginEventArgs e)
            {
                Sender = sender;
                Count++;
                await e.InvokeNext();
                Count++;
            }
        }
    }
}