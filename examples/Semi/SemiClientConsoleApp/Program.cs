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

namespace SemiClientConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var client = await CreateHsmsClientAsync();

        await SendDataMessageAsync(client);
        await SendLinktestAsync(client);
        await BuildSecsItemsAsync(client);
        await UsePluginClientAsync();

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    #region HSMS创建客户端
    private static async Task<HsmsClient> CreateHsmsClientAsync()
    {
        var client = new HsmsClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:5000")
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                a.Add<MyHsmsClientPlugin>();
            }));
        await client.ConnectAsync();
        return client;
    }
    #endregion

    #region HSMS客户端快捷连接
    private static async Task<HsmsClient> QuickConnectAsync()
    {
        var client = new HsmsClient();
        await client.ConnectAsync("127.0.0.1:5000");
        return client;
    }
    #endregion

    #region HSMS发送数据消息
    private static async Task SendDataMessageAsync(HsmsClient client)
    {
        // 发送 S1F1（Are You There），并等待响应 S1F2
        var request = new HsmsMessage
        {
            S = 1,
            F = 1,
            ReplyExpected = true
        };
        var response = await client.SendHsmsMessageAsync(request);
        Console.WriteLine($"S1F1 响应：S={response?.S} F={response?.F}");
    }
    #endregion

    #region HSMS发送链路测试
    private static async Task SendLinktestAsync(HsmsClient client)
    {
        var response = await client.SendLinkTestAsync();
        Console.WriteLine($"Linktest 响应：MessageType={response?.MessageType}");
    }
    #endregion

    #region HSMS构建SecsItem数据
    private static async Task BuildSecsItemsAsync(HsmsClient client)
    {
        // 构造包含多种 SECS-II 数据项的消息体
        var asciiItem = new ASCIISecsItem();
        var u4Item = new U4SecsItem();
        var i4Item = new I4SecsItem();
        var boolItem = new BooleanSecsItem();
        var binaryItem = new BinarySecsItem();

        // 构造 List 数据项（SECS-II L 格式）
        var listItem = new ListSecsItem();

        // 发送包含 ASCII 数据体的 S2F41（Host Command Send）
        var msg = new HsmsMessage(asciiItem)
        {
            S = 2,
            F = 41,
            ReplyExpected = false
        };
        await client.SendHsmsMessageAsync(msg);
    }
    #endregion

    #region HSMS使用插件客户端
    private static async Task UsePluginClientAsync()
    {
        var client = new HsmsClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:5000")
            .ConfigurePlugins(a =>
            {
                a.Add<MyHsmsClientPlugin>();
            }));
        await client.ConnectAsync();

        // 发送 Separate.req 断开连接
        await client.SendSeparateAsync();
    }
    #endregion
}

#region HSMS客户端插件
internal class MyHsmsClientPlugin : PluginBase, IHsmsConnectedPlugin, IHsmsReceivedPlugin, IHsmsClosedPlugin
{
    public async Task OnHsmsConnected(IHsmsSession client, ConnectedEventArgs e)
    {
        Console.WriteLine("已连接到 HSMS 服务器");
        await e.InvokeNext();
    }

    public async Task OnHsmsReceived(IHsmsSession client, HsmsReceivedEventArgs e)
    {
        var msg = e.Message;
        Console.WriteLine($"收到推送消息：S{msg.S}F{msg.F}");
        await e.InvokeNext();
    }

    public async Task OnHsmsClosed(IHsmsSession client, ClosedEventArgs e)
    {
        Console.WriteLine("与 HSMS 服务器的连接已断开");
        await e.InvokeNext();
    }
}
#endregion
