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
        await BuildListSecsItemAsync(client);
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
        // 字符串类型
        var asciiItem = new ASCIISecsItem("LOTID-001");       // ASCII 字符串
        var jis8Item = new JIS8SecsItem("装置名称");           // JIS8 字符串

        // 有符号整数（值均为数组，大端序编码）
        var i1Item = new I1SecsItem(new sbyte[] { -1, 0, 127 });
        var i4Item = new I4SecsItem(new int[] { -100000, 0, 100000 });

        // 无符号整数
        var u1Item = new U1SecsItem(new byte[] { 0, 128, 255 });
        var u4Item = new U4SecsItem(new uint[] { 0, 42, 100000 });

        // 浮点类型
        var f4Item = new F4SecsItem(new float[] { 1.0f, 3.14f });
        var f8Item = new F8SecsItem(new double[] { 1.0, 3.14159 });

        // 布尔类型（0=false，非0=true）
        var boolItem = new BooleanSecsItem(new byte[] { 1, 0, 1 });

        // 二进制类型
        var binaryItem = new BinarySecsItem(new byte[] { 0x01, 0x02, 0x03 });

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

    #region HSMS字符串数据项构造
    private static void BuildStringSecsItems()
    {
        // ASCII 字符串：仅支持 ASCII 可打印字符（0x20~0x7E）
        var asciiItem = new ASCIISecsItem("LOTID-001");
        Console.WriteLine($"ASCII: {asciiItem.Value}");

        // JIS8 字符串：使用 UTF-8 编码，可包含中文等多字节字符
        var jis8Item = new JIS8SecsItem("装置名称");
        Console.WriteLine($"JIS8: {jis8Item.Value}");
    }
    #endregion

    #region HSMS整数数据项构造
    private static void BuildIntegerSecsItems()
    {
        // 有符号整数（支持数组，大端序编码）
        var i1Item = new I1SecsItem(new sbyte[] { -1, 0, 127 });        // 1 字节有符号，sbyte[]
        var i2Item = new I2SecsItem(new short[] { -1000, 0, 1000 });    // 2 字节有符号，short[]
        var i4Item = new I4SecsItem(new int[] { -100000, 0, 100000 });  // 4 字节有符号，int[]
        var i8Item = new I8SecsItem(new long[] { -1L, 0L, 1L });        // 8 字节有符号，long[]

        // 无符号整数
        var u1Item = new U1SecsItem(new byte[] { 0, 128, 255 });           // 1 字节无符号，byte[]
        var u2Item = new U2SecsItem(new ushort[] { 0, 1000, 65535 });      // 2 字节无符号，ushort[]
        var u4Item = new U4SecsItem(new uint[] { 0, 100000 });             // 4 字节无符号，uint[]
        var u8Item = new U8SecsItem(new ulong[] { 0, ulong.MaxValue });    // 8 字节无符号，ulong[]

        // 也可以传入单个值（数组长度为 1）
        var singleU4 = new U4SecsItem(new uint[] { 42 });
        Console.WriteLine($"U4 单值: {singleU4.Values.Span[0]}");
    }
    #endregion

    #region HSMS浮点数据项构造
    private static void BuildFloatSecsItems()
    {
        // F4：4 字节单精度浮点，float[]
        var f4Item = new F4SecsItem(new float[] { 1.0f, 3.14f, -1.5f });
        Console.WriteLine($"F4 第一个值: {f4Item.Values.Span[0]}");

        // F8：8 字节双精度浮点，double[]
        var f8Item = new F8SecsItem(new double[] { 1.0, 3.14159265358979, -1.5 });
        Console.WriteLine($"F8 第一个值: {f8Item.Values.Span[0]}");
    }
    #endregion

    #region HSMS布尔与二进制数据项构造
    private static void BuildBooleanAndBinarySecsItems()
    {
        // Boolean：以 byte 存储，0 表示 false，非 0（通常为 1）表示 true
        var boolItem = new BooleanSecsItem(new byte[] { 1, 0, 1 });
        foreach (var b in boolItem.Values.Span)
        {
            Console.WriteLine($"Boolean 值: {b != 0}");
        }

        // Binary：原始字节数组，适合传递不透明二进制数据
        var binaryItem = new BinarySecsItem(new byte[] { 0xAB, 0xCD, 0xEF });
        Console.WriteLine($"Binary 长度: {binaryItem.Data.Length}");
    }
    #endregion

    #region HSMS构建List嵌套数据项
    private static async Task BuildListSecsItemAsync(HsmsClient client)
    {
        // 构造 S6F11（Event Report Send）消息体（L 嵌套结构示意）：
        // L [
        //   U4  [DataID = 1]
        //   U4  [CEID = 2001]
        //   L   [ReportList]
        //     L [Report]
        //       U4    [RPTID = 1001]
        //       L     [VIDList]
        //         ASCII [LOT-001]
        //         F4    [25.5]
        // ]
        var vidList = new ListSecsItem(new SecsItem[]
        {
            new ASCIISecsItem("LOT-001"),
            new F4SecsItem(new float[] { 25.5f })
        });

        var report = new ListSecsItem(new SecsItem[]
        {
            new U4SecsItem(new uint[] { 1001 }),   // RPTID
            vidList                                 // VIDList
        });

        var reportList = new ListSecsItem(new SecsItem[] { report });

        var s6f11Body = new ListSecsItem(new SecsItem[]
        {
            new U4SecsItem(new uint[] { 1 }),       // DataID
            new U4SecsItem(new uint[] { 2001 }),    // CEID
            reportList
        });

        var msg = new HsmsMessage(s6f11Body)
        {
            S = 6,
            F = 11,
            ReplyExpected = false
        };
        await client.SendHsmsMessageAsync(msg);
    }
    #endregion

    #region HSMS解析数据项
    private static void ParseSecsItem(HsmsMessage message)
    {
        if (message.Body is ASCIISecsItem ascii)
        {
            Console.WriteLine($"ASCII: {ascii.Value}");
        }
        else if (message.Body is U4SecsItem u4)
        {
            foreach (var val in u4.Values.Span)
            {
                Console.WriteLine($"U4 值: {val}");
            }
        }
        else if (message.Body is F4SecsItem f4)
        {
            foreach (var val in f4.Values.Span)
            {
                Console.WriteLine($"F4 值: {val}");
            }
        }
        else if (message.Body is BooleanSecsItem boolItem)
        {
            foreach (var val in boolItem.Values.Span)
            {
                Console.WriteLine($"Boolean 值: {val != 0}");
            }
        }
        else if (message.Body is BinarySecsItem binary)
        {
            Console.WriteLine($"Binary 数据长度: {binary.Data.Length}");
        }
        else if (message.Body is ListSecsItem list)
        {
            Console.WriteLine($"List 子项数量: {list.Items.Length}");
            foreach (var item in list.Items.Span)
            {
                Console.WriteLine($"  子项格式: {item.SecsFormat}");
            }
        }
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
