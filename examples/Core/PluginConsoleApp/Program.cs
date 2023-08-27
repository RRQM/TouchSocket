using System.Threading.Tasks;
using System;
using TouchSocket.Core;

namespace PluginConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IPluginsManager pluginsManager = new PluginsManager(new Container())
            {
                Enable = true//必须启用
            };

            pluginsManager.Add<SayHelloPlugin>();
            pluginsManager.Add<SayHiPlugin>();
            pluginsManager.Add<LastSayPlugin>();

            //订阅插件，不仅可以使用声明插件的方式，还可以使用委托。
            pluginsManager.Add(nameof(ISayPlugin.Say), () =>
            {
                Console.WriteLine("在Action1中获得");
            });

            pluginsManager.Add(nameof(ISayPlugin.Say), async (MyPluginEventArgs e) =>
            {
                Console.WriteLine("在Action2中获得");
                await e.InvokeNext();
            });

            while (true)
            {
                Console.WriteLine("请输入hello，或者hi");
                pluginsManager.Raise(nameof(ISayPlugin.Say), new object(), new MyPluginEventArgs()
                {
                    Words = Console.ReadLine()
                });
            }

        }

        public class MyPluginEventArgs : PluginEventArgs
        {
            public string Words { get; set; }
        }

        /// <summary>
        /// 定义一个插件接口，使其继承<see cref="IPlugin"/>
        /// </summary>
        public interface ISayPlugin : IPlugin
        {
            /// <summary>
            /// Say。定义一个插件方法，必须遵循：
            /// 1.必须是两个参数，第一个参数可以是任意类型，一般表示触发源。第二个参数必须继承<see cref="PluginEventArgs"/>
            /// 2.返回值必须是Task。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            /// <returns></returns>
            Task Say(object sender, MyPluginEventArgs e);
        }

        public class SayHelloPlugin : PluginBase, ISayPlugin
        {
            public SayHelloPlugin()
            {
                Order = 0;//插件的添加顺序，该值越大，越靠前执行。默认都是0
            }
            public async Task Say(object sender, MyPluginEventArgs e)
            {
                Console.WriteLine($"{this.GetType().Name}------Enter");
                if (e.Words == "hello")
                {
                    Console.WriteLine($"{this.GetType().Name}------Say");
                    //当满足的时候输出，且不在调用下一个插件。

                    //亦或者设置e.Handled = true，即使调用下一个插件，也会无效
                    e.Handled = true;
                    return;
                }
                await e.InvokeNext();
                Console.WriteLine($"{this.GetType().Name}------Leave");
            }
        }

        public class SayHiPlugin : PluginBase, ISayPlugin
        {
            public async Task Say(object sender, MyPluginEventArgs e)
            {
                Console.WriteLine($"{this.GetType().Name}------Enter");
                if (e.Words == "hi")
                {
                    Console.WriteLine($"{this.GetType().Name}------Say");
                    //当满足的时候输出，且不在调用下一个插件。

                    //亦或者设置e.Handled = true，即使调用下一个插件，也会无效
                    e.Handled = true;
                    return;
                }

                await e.InvokeNext();
                Console.WriteLine($"{this.GetType().Name}------Leave");
            }
        }
        public class LastSayPlugin : PluginBase, ISayPlugin
        {
            public async Task Say(object sender, MyPluginEventArgs e)
            {
                Console.WriteLine($"{this.GetType().Name}------Enter");
                Console.WriteLine($"您输入的{e.Words}似乎不被任何插件处理");
                await e.InvokeNext();
                Console.WriteLine($"{this.GetType().Name}------Leave");
            }
        }
    }
}