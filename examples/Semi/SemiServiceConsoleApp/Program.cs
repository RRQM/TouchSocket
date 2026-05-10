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
using TouchSocket.Semi;
using TouchSocket.Sockets;

namespace SemiServiceConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateHsmsServiceAsync();
        Console.WriteLine("HSMS 服务已启动，按任意键退出...");
        Console.ReadKey();
    }

    #region HSMS创建服务
    private static async Task<HsmsService> CreateHsmsServiceAsync()
    {
        var service = new HsmsService();
        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(5000)
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                a.Add<MyHsmsServicePlugin>();
            }));
        await service.StartAsync();
        return service;
    }
    #endregion
}

#region HSMS服务端插件
internal class MyHsmsServicePlugin : PluginBase, IHsmsConnectedPlugin, IHsmsReceivedPlugin, IHsmsClosedPlugin
{
    public async Task OnHsmsConnected(IHsmsSession client, ConnectedEventArgs e)
    {
        Console.WriteLine($"设备已连接：{((IHsmsSessionClient)client).Id}");
        await e.InvokeNext();
    }

    public async Task OnHsmsReceived(IHsmsSession client, HsmsReceivedEventArgs e)
    {
        var msg = e.Message;
        Console.WriteLine($"收到消息：S{msg.S}F{msg.F}，Body={msg.Body}");

        // 构造并发送 S1F2 响应（On-Line Data）
        if (msg.S == 1 && msg.F == 1 && msg.ReplyExpected)
        {
            #region HSMS服务端发送数据消息
            var response = new HsmsMessage
            {
                S = 1,
                F = 2,
                ReplyExpected = false,
                SystemBytes = msg.SystemBytes,
                Body = new ListSecsItem()
            };
            await ((IHsmsSession)client).SendHsmsMessageAsync(response);
            #endregion
        }

        await e.InvokeNext();
    }

    public async Task OnHsmsClosed(IHsmsSession client, ClosedEventArgs e)
    {
        Console.WriteLine($"设备已断开：{((IHsmsSessionClient)client).Id}");
        await e.InvokeNext();
    }
}
#endregion
