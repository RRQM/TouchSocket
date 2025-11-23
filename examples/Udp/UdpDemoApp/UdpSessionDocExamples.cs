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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UdpDemoApp;

#region UdpSession作为服务器使用
internal class UdpSessionServerExample
{
    public async Task ServerExample()
    {
        var udpService = new UdpSession();
        udpService.Received = (c, e) =>
        {
            Console.WriteLine(e.Memory.ToString());
            return EasyTask.CompletedTask;
        };
        await udpService.SetupAsync(new TouchSocketConfig()
            .SetBindIPHost(new IPHost(7789)));
        await udpService.StartAsync();
        Console.WriteLine("等待接收");
    }
}
#endregion

#region UdpSession作为客户端使用
internal class UdpSessionClientExample
{
    public async Task ClientExample()
    {
        var udpClient = new UdpSession();
        await udpClient.SetupAsync(new TouchSocketConfig()
            //.UseUdpReceive()//作为客户端时，如果需要接收数据，那么需要绑定端口。要么使用SetBindIPHost指定端口，要么调用UseUdpReceive绑定随机端口。
            .SetBindIPHost(new IPHost(7788)));
        await udpClient.StartAsync();
    }
}
#endregion

#region UdpSession发送到接收数据的地址
internal class MyPluginClass : PluginBase, IUdpReceivedPlugin
{
    public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
    {
        if (client is IUdpSession udpSession)
        {
            await udpSession.SendAsync(e.EndPoint, Encoding.UTF8.GetBytes("RRQM"));
        }
        await e.InvokeNext();
    }
}
#endregion

#region UdpSession委托接收
internal class UdpSessionDelegateReceiveExample
{
    public async Task DelegateReceiveExample()
    {
        var udpService = new UdpSession();
        udpService.Received = (c, e) =>
        {
            Console.WriteLine(e.Memory.ToString());
            return EasyTask.CompletedTask;
        };
        await udpService.StartAsync(7789);
    }
}
#endregion

#region UdpSession插件接收声明插件1
internal class MyPluginClass1 : PluginBase, IUdpReceivedPlugin
{
    public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
    {
        var msg = e.Memory.ToString();
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
#endregion

#region UdpSession插件接收声明插件2
internal class MyPluginClass2 : PluginBase, IUdpReceivedPlugin
{
    public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
    {
        var msg = e.Memory.ToString();
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
#endregion

#region UdpSession插件接收使用插件
internal class UdpSessionPluginReceiveExample
{
    public async Task PluginReceiveExample()
    {
        var udpService = new UdpSession();
        await udpService.SetupAsync(new TouchSocketConfig()
            .SetBindIPHost(new IPHost(7789))
            .ConfigurePlugins(a =>
            {
                a.Add<MyPluginClass1>();
                a.Add<MyPluginClass2>();
            }));
        await udpService.StartAsync();
    }
}
#endregion

#region UdpSession创建组播服务器
internal class UdpSessionMulticastServerExample
{
    public async Task MulticastServerExample()
    {
        //创建udpService
        var udpService = new UdpSession();
        udpService.Received = (remote, e) =>
        {
            Console.WriteLine(e.Memory.ToString());
            return EasyTask.CompletedTask;
        };
        await udpService.SetupAsync(new TouchSocketConfig()
            .SetBindIPHost(new IPHost(7789))
            .SetEnableBroadcast(true));
        await udpService.StartAsync();

        //加入组播组
        udpService.JoinMulticastGroup(IPAddress.Parse("224.5.6.7"));
    }
}
#endregion

#region UdpSession发送组播数据
internal class UdpSessionMulticastSendExample
{
    public async Task MulticastSendExample()
    {
        var udpClient = new UdpSession();
        await udpClient.SetupAsync(new TouchSocketConfig()
            .SetBindIPHost(new IPHost(7788)));
        await udpClient.StartAsync();

        await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("我是组播"));
    }
}
#endregion

#region UdpSession创建广播服务器
internal class UdpSessionBroadcastServerExample
{
    public async Task BroadcastServerExample()
    {
        //创建udpService
        var udpService = new UdpSession();
        udpService.Received = (remote, e) =>
        {
            Console.WriteLine(e.Memory.ToString());
            return EasyTask.CompletedTask;
        };
        await udpService.SetupAsync(new TouchSocketConfig()
            .SetBindIPHost(new IPHost(7789))
            .SetEnableBroadcast(true));
        await udpService.StartAsync();
    }
}
#endregion

#region UdpSession发送广播数据
internal class UdpSessionBroadcastSendExample
{
    public async Task BroadcastSendExample()
    {
        var udpClient = new UdpSession();
        await udpClient.SetupAsync(new TouchSocketConfig()
            .SetEnableBroadcast(true)//该配置在发送广播时是必须的
            .SetBindIPHost(new IPHost(7788)));
        await udpClient.StartAsync();

        await udpClient.SendAsync(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("我是广播"));
    }
}
#endregion

#region UdpSession传输大于64K的数据
internal class UdpSessionBigDataExample
{
    public async Task BigDataExample()
    {
        var udpSession = new UdpSession();
        udpSession.Received = (endpoint, e) =>
        {
            // 处理接收到的数据
            return EasyTask.CompletedTask;
        };

        await udpSession.SetupAsync(new TouchSocketConfig()
            .SetBindIPHost(new IPHost("127.0.0.1:7789"))
            .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter()));//加载配置
        await udpSession.StartAsync();//启动
    }
}
#endregion
