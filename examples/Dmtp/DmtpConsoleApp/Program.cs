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

using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var action = new ConsoleAction();
        action.Add("1", "测试连接", Connect_1);
        action.Add("2", "测试以普通Tcp连接", Connect_2);
        action.Add("3", "发送消息", SendAsync);
        action.OnException += Action_OnException;
        var service = await CreateTcpDmtpService();

        action.ShowAll();

        await action.RunCommandLineAsync();
    }


    private static void Action_OnException(Exception obj)
    {
        Console.WriteLine(obj.Message);
    }

    private static async Task SendAsync()
    {
        using var client = new TcpDmtpClient();

        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .ConfigurePlugins(a =>
             {
                 //此处使用委托注册插件。和类插件功能一样
                 a.AddDmtpReceivedPlugin(async (c, e) =>
                 {
                     var msg = e.DmtpMessage.BodyByteBlock.ToString();
                     await Console.Out.WriteLineAsync($"收到服务器回信，协议{e.DmtpMessage.ProtocolFlags}收到信息，内容：{msg}");
                     await e.InvokeNext();
                 });
             })
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetDmtpOption(new DmtpOption()
             {
                 VerifyToken = "Dmtp",//设置Token验证连接
                 Id = "defaultId",//设置默认Id
                 Metadata = new Metadata().Add("a", "a")//设置Metadata，可以传递更多的验证信息
             }));
        await client.ConnectAsync();

        client.Logger.Info($"{nameof(Connect_1)}连接成功，Id={client.Id}");

        //使用Dmtp送消息。必须指定一个protocol，这是一个ushort类型的值。
        //20以内的值，框架在使用，所以在发送时要指定一个大于20的值
        //同时需要注意，当Dmtp添加其他功能组件的时候，可能也会占用协议。
        //例如：
        //DmtpRpc会用[20,25)的协议。
        //文件传输会用[25,35)的协议。

        //此处使用1000，基本就不会冲突。
        await client.SendAsync(1000, Encoding.UTF8.GetBytes("hello"));
    }

    /// <summary>
    /// 使用普通tcp连接
    /// </summary>
    /// <returns></returns>
    private static async Task Connect_2()
    {
        using var tcpClient = new TcpClient();//创建一个普通的tcp客户端。
        tcpClient.Received = (client, e) =>
        {
            //此处接收服务器返回的消息

            var head = e.ByteBlock.ToArray(0, 2);
            e.ByteBlock.Seek(2, SeekOrigin.Begin);
            var flags = e.ByteBlock.ReadUInt16(EndianType.Big);
            var length = e.ByteBlock.ReadInt32(EndianType.Big);

            var json = e.ByteBlock.Span.ToString(Encoding.UTF8);

            ConsoleLogger.Default.Info($"收到响应：flags={flags},length={length},json={json.Replace("\r\n", string.Empty).Replace(" ", string.Empty)}");


            return Task.CompletedTask;
        };

        #region 基础Flag协议

        Console.WriteLine($"{nameof(DmtpActor.P0_Close)}-flag-->{DmtpActor.P0_Close}");
        Console.WriteLine($"{nameof(DmtpActor.P1_Handshake_Request)}-flag-->{DmtpActor.P1_Handshake_Request}");
        Console.WriteLine($"{nameof(DmtpActor.P2_Handshake_Response)}-flag-->{DmtpActor.P2_Handshake_Response}");
        Console.WriteLine($"{nameof(DmtpActor.P3_ResetId_Request)}-flag-->{DmtpActor.P3_ResetId_Request}");
        Console.WriteLine($"{nameof(DmtpActor.P4_ResetId_Response)}-flag-->{DmtpActor.P4_ResetId_Response}");
        Console.WriteLine($"{nameof(DmtpActor.P5_Ping_Request)}-flag-->{DmtpActor.P5_Ping_Request}");
        Console.WriteLine($"{nameof(DmtpActor.P6_Ping_Response)}-flag-->{DmtpActor.P6_Ping_Response}");
        Console.WriteLine($"{nameof(DmtpActor.P7_CreateChannel_Request)}-flag-->{DmtpActor.P7_CreateChannel_Request}");
        Console.WriteLine($"{nameof(DmtpActor.P8_CreateChannel_Response)}-flag-->{DmtpActor.P8_CreateChannel_Response}");
        Console.WriteLine($"{nameof(DmtpActor.P9_ChannelPackage)}-flag-->{DmtpActor.P9_ChannelPackage}");

        #endregion 基础Flag协议

        #region 连接

        //开始链接服务器
        await tcpClient.ConnectAsync("127.0.0.1:7789");

        //以json的数据方式。
        //其中Token、Metadata为连接的验证数据，分别为字符串、字符串字典类型。
        //Id则表示指定的默认id，字符串类型。
        //Sign为本次请求的序号，一般在连接时指定一个大于0的任意数字即可。
        var json = @"{""Token"":""Dmtp"",""Metadata"":{""a"":""a""},""Id"":null,""Sign"":1}";

        //将json转为utf-8编码。
        var jsonBytes = Encoding.UTF8.GetBytes(json);

        using (var byteBlock = new ByteBlock(1024*64))
        {
            //按照Head+Flags+Length+Data的格式。
            byteBlock.Write(Encoding.ASCII.GetBytes("dm"));
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)1));
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(jsonBytes.Length));
            byteBlock.Write(jsonBytes);

            await tcpClient.SendAsync(byteBlock.Memory);
        }

        #endregion 连接

        #region Ping

        json = "{\"Sign\":2,\"Route\":false,\"SourceId\":null,\"TargetId\":null}";
        jsonBytes = Encoding.UTF8.GetBytes(json);

        using (var byteBlock = new ByteBlock(1024*64))
        {
            //按照Head+Flags+Length+Data的格式。
            byteBlock.Write(Encoding.ASCII.GetBytes("dm"));
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)5));
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(jsonBytes.Length));
            byteBlock.Write(jsonBytes);

            await tcpClient.SendAsync(byteBlock.Memory);
        }

        #endregion Ping

        await Task.Delay(2000);
    }

    /// <summary>
    /// 使用已封装的客户端执行：
    /// 1、设置Token验证连接。
    /// 2、设置Metadata，可以传递更多的验证信息。
    /// 3、设置默认Id。
    /// </summary>
    private static async Task Connect_1()
    {
        using var client = new TcpDmtpClient();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetDmtpOption(new DmtpOption()
             {
                 VerifyToken = "Dmtp",//设置Token验证连接
                 Id = "defaultId",//设置默认Id
                 Metadata = new Metadata().Add("a", "a")//设置Metadata，可以传递更多的验证信息
             }));
        await client.ConnectAsync();

        await client.PingAsync();

        client.Logger.Info($"{nameof(Connect_1)}连接成功，Id={client.Id}");
    }

    private static async Task<TcpDmtpService> CreateTcpDmtpService()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.Add<MyVerifyPlugin>();
                   a.Add<MyFlagsPlugin>();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Dmtp"//设定连接口令，作用类似账号密码
               });

        await service.SetupAsync(config);
        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");
        return service;
    }
}

internal class MyVerifyPlugin : PluginBase, IDmtpHandshakingPlugin
{
    public async Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        if (e.Metadata["a"] != "a")
        {
            e.IsPermitOperation = false;//不允许连接
            e.Message = "元数据不对";//同时返回消息
            e.Handled = true;//表示该消息已在此处处理。
            return;
        }
        if (e.Token == "Dmtp")
        {
            e.IsPermitOperation = true;
            e.Handled = true;
            return;
        }

        await e.InvokeNext();
    }
}

internal class MyFlagsPlugin : PluginBase, IDmtpReceivedPlugin
{
    public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
    {
        if (e.DmtpMessage.ProtocolFlags == 1000)
        {
            //判断完协议以后，从 e.DmtpMessage.BodyByteBlock可以拿到实际的数据
            var msg = e.DmtpMessage.BodyByteBlock.ToString();
            await Console.Out.WriteLineAsync($"从协议{e.DmtpMessage.ProtocolFlags}收到信息，内容：{msg}");

            //向客户端回发消息
            await client.SendAsync(1001, Encoding.UTF8.GetBytes("收到"));
            return;
        }

        //flags不满足，调用下一个插件
        await e.InvokeNext();
    }
}