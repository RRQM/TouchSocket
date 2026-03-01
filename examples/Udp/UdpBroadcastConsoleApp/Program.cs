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

using System.Net;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UdpBroadcastConsoleApp;

internal class Program
{
    private static UdpSession s_udpService = new UdpSession();

    private static async Task Main(string[] args)
    {
        ConsoleAction consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;

        consoleAction.Add("1", "启动服务器", StartServer);
        consoleAction.Add("2", "测试UDP组播", UdpMulticastTester);
        consoleAction.Add("3", "测试UDP广播", UdpBroadcastTester);

        consoleAction.ShowAll();
        await consoleAction.RunCommandLineAsync();
    }

    private static async Task StartServer()
    {
        await s_udpService.SetupAsync(new TouchSocketConfig()
             .SetBindIPHost(new IPHost(7789))
             .SetEnableBroadcast(true)
             .SetUdpConnReset(true)
             .ConfigurePlugins(a =>
             {
                 a.Add<MyPluginClass1>();
                 a.Add<MyPluginClass2>();
                 a.Add<MyPluginClass3>();
             })
             );
        await s_udpService.StartAsync();
        Console.WriteLine("服务器已启动，监听端口：7789");

        //加入组播组，以便接收组播消息
        s_udpService.JoinMulticastGroup(IPAddress.Parse("224.5.6.7"));
        Console.WriteLine("已加入组播组：224.5.6.7");
    }

    private static async Task UdpMulticastTester()
    {
        using var udpClient = new UdpSession();
        await udpClient.SetupAsync(new TouchSocketConfig()
              .SetBindIPHost(new IPHost(7788))
              .SetEnableBroadcast(true));
        await udpClient.StartAsync();

        //发送组播消息，目标地址为组播地址224.5.6.7，端口7789
        await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("我是组播"));
        Console.WriteLine("已发送组播消息：我是组播");

        //发送hello，将由MyPluginClass1处理
        await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("hello"));
        Console.WriteLine("已发送组播消息：hello");

        await Task.Delay(100);//等待接收方处理完毕
    }

    private static async Task UdpBroadcastTester()
    {
        using var udpClient = new UdpSession();
        await udpClient.SetupAsync(new TouchSocketConfig()
              .SetBindIPHost(new IPHost("127.0.0.1:7788"))
              .SetEnableBroadcast(true));
        await udpClient.StartAsync();

        //发送广播消息，目标地址为广播地址255.255.255.255，端口7789
        await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("我是广播"));
        Console.WriteLine("已发送广播消息：我是广播");

        //发送hi，将由MyPluginClass2处理
        await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("hi"));
        Console.WriteLine("已发送广播消息：hi");

        await Task.Delay(100);//等待接收方处理完毕
    }

    private static void ConsoleAction_OnException(Exception obj)
    {
        Console.WriteLine(obj.ToString());
    }

    private class MyPluginClass1 : PluginBase, IUdpReceivedPlugin
    {
        public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
        {
            var msg = e.Memory.Span.ToUtf8String();
            if (msg == "hello")
            {
                Console.WriteLine("已处理Hello");
            }
            else
            {
                //如果判断逻辑发现此处无法处理，即可转到下一个插件
                await e.InvokeNext();
            }
        }
    }

    private class MyPluginClass2 : PluginBase, IUdpReceivedPlugin
    {
        public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
        {
            var msg = e.Memory.Span.ToUtf8String();
            if (msg == "hi")
            {
                Console.WriteLine("已处理Hi");
            }
            else
            {
                //如果判断逻辑发现此处无法处理，即可转到下一个插件
                await e.InvokeNext();
            }
        }
    }

    private class MyPluginClass3 : PluginBase, IUdpReceivedPlugin
    {
        public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
        {
            var msg = e.Memory.Span.ToUtf8String();
            Console.WriteLine(msg);
            await Task.CompletedTask;
        }
    }
}