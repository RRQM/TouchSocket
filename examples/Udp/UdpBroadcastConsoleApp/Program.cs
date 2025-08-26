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
    private static async Task Main(string[] args)
    {
        //创建udpService
        var udpService = new UdpSession();
        await udpService.SetupAsync(new TouchSocketConfig()
             .SetBindIPHost(new IPHost(7789))
             .UseBroadcast()
             .ConfigurePlugins(a =>
             {
                 a.Add<MyPluginClass1>();
                 a.Add<MyPluginClass2>();
                 a.Add<MyPluginClass3>();
             })
             );
        await udpService.StartAsync();

        //加入组播组
        udpService.JoinMulticastGroup(IPAddress.Parse("224.5.6.7"));

        var udpClient = new UdpSession();
        await udpClient.SetupAsync(new TouchSocketConfig()
              //.UseUdpReceive()//作为客户端时，如果需要接收数据，那么需要绑定端口。要么使用SetBindIPHost指定端口，要么调用UseUdpReceive绑定随机端口。
              .SetBindIPHost(new IPHost(7788))
              .UseBroadcast()//该配置在广播时是必须的
             );
        await udpClient.StartAsync();

        while (true)
        {
            await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("我是组播"));
            await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("我是广播"));

            await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("hello"));
            await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("hi"));


            await Task.Delay(1000);
        }
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