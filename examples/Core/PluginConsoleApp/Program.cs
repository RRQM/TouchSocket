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

using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace PluginConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IPluginManager pluginManager = new PluginManager(new Container())
        {
            Enable = true//必须启用
        };

        //添加插件
        pluginManager.Add<SayHelloPlugin>();
        pluginManager.Add<SayHelloAction>();
        pluginManager.Add<SayHelloGenerator>();
        pluginManager.Add<SayHiPlugin>();
        pluginManager.Add<LastSayPlugin>();



        //订阅插件，不仅可以使用声明插件的方式，还可以使用委托。
        pluginManager.Add(typeof(ISayPlugin), () =>
        {
            //无参委托，一般做通知
            Console.WriteLine("在Action1中获得");
        });

        pluginManager.Add(typeof(ISayPlugin), async (MyPluginEventArgs e) =>
        {
            //只1个指定参数，当参数是事件参数时，需要主动InvokeNext
            Console.WriteLine("在Action2中获得");
            await e.InvokeNext();
        });

        pluginManager.Add(typeof(ISayPlugin), async (client, e) =>
        {
            //2个不指定参数，需要主动InvokeNext
            Console.WriteLine("在Action3中获得");
            await e.InvokeNext();
        });

        pluginManager.Add(typeof(ISayPlugin), async (object client, MyPluginEventArgs e) =>
        {
            //2个指定参数，需要主动InvokeNext
            Console.WriteLine("在Action3中获得");
            await e.InvokeNext();
        });

        while (true)
        {
            Console.WriteLine("请输入hello、helloaction、hellogenerator、hi或者其他");
            await pluginManager.RaiseAsync(typeof(ISayPlugin), new object(), new MyPluginEventArgs()
            {
                Words = Console.ReadLine()
            });
        }
    }
}

public class MyPluginEventArgs : PluginEventArgs
{
    public string Words { get; set; }
}

/// <summary>
/// 定义一个插件接口，使其继承<see cref="IPlugin"/>
/// </summary>
[DynamicMethod]
public interface ISayPlugin : IPlugin
{
    /// <summary>
    /// Say。定义一个插件方法，必须遵循：
    /// 1.必须是两个参数，第一个参数可以是任意类型，一般表示触发源。第二个参数必须继承<see cref="PluginEventArgs"/>
    /// 2.返回值必须是Task。
    /// </summary>
    /// <param name="sender">触发主体</param>
    /// <param name="e">传递参数</param>
    /// <returns></returns>
    Task Say(object sender, MyPluginEventArgs e);
}

public class SayHelloPlugin : PluginBase, ISayPlugin
{
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

internal class LastSayPlugin : PluginBase, ISayPlugin
{
    public async Task Say(object sender, MyPluginEventArgs e)
    {
        Console.WriteLine($"{this.GetType().Name}------Enter");
        Console.WriteLine($"您输入的{e.Words}似乎不被任何插件处理");
        await e.InvokeNext();
        Console.WriteLine($"{this.GetType().Name}------Leave");
    }
}

public class SayHelloAction : PluginBase
{
    protected override void Loaded(IPluginManager pluginManager)
    {
        base.Loaded(pluginManager);

        //注册本地方法为委托
        pluginManager.Add<object, MyPluginEventArgs>(typeof(ISayPlugin), this.Say);
    }

    public async Task Say(object sender, MyPluginEventArgs e)
    {
        Console.WriteLine($"{this.GetType().Name}------Enter");
        if (e.Words == "helloaction")
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

public partial class SayHelloGenerator : PluginBase, ISayPlugin
{
    /// <summary>
    /// 使用源生成插件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public async Task Say(object sender, MyPluginEventArgs e)
    {
        Console.WriteLine($"{this.GetType().Name}------Enter");
        if (e.Words == "hellogenerator")
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