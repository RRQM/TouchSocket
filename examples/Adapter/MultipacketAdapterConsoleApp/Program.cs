using System.Text;
using System.Text.Json;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace MultipacketAdapterConsoleApp;
internal class Program
{
    static async Task Main(string[] args)
    {
        var tcpService = await GetTcpService();
        var tcpClient = await GetTcpClient();

        while (true)
        {
            var input = Console.ReadLine();
            if (input.HasValue())
            {
                if (input == "1")
                {
                    await tcpClient.SendAsync(new MyPack1() { MyProperty1 = 10 });
                }
                else if (input == "2")
                {
                    await tcpClient.SendAsync(new MyPack2() { MyProperty1 = "Hello World" });
                }
                else
                {
                    throw new Exception("输入错误");
                }
            }
        }
    }

    static async Task<TcpClient> GetTcpClient()
    {
        var tcpClient = new TcpClient();
        await tcpClient.SetupAsync(new TouchSocketConfig()
            .SetTcpDataHandlingAdapter(() => new MyAdapter())
               .SetRemoteIPHost("tcp://127.0.0.1:7789"));
        await tcpClient.ConnectAsync();

        return tcpClient;
    }

    static async Task<TcpService> GetTcpService()
    {
        var service = new TcpService();

        service.Received = async (client, e) =>
        {
            //从客户端收到信息
            switch (e.RequestInfo)
            {
                case MyPack1 pack:
                    {
                        Console.WriteLine($"收到MyPack1，MyProperty1={pack.MyProperty1}");
                        break;
                    }
                case MyPack2 pack:
                    {
                        Console.WriteLine($"收到MyPack2，MyProperty1={pack.MyProperty1}");
                        break;
                    }
                default:
                    break;
            }
            await EasyTask.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(7789)
            .SetTcpDataHandlingAdapter(() => new MyAdapter()));

        await service.StartAsync();//启动
        return service;
    }
}

class MyPackBase : IRequestInfo
{

}

class MyPack1 : MyPackBase
{
    public int MyProperty1 { get; set; }
}

class MyPack2 : MyPackBase
{
    public string? MyProperty1 { get; set; }
}

class MyAdapter : CustomDataHandlingAdapter<MyPackBase>
{
    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref MyPackBase request)
    {
        if (reader.BytesRemaining < 5)
        {
            return FilterResult.Cache;
        }

        var header = reader.GetSpan(5);
        var type = header.ReadValue<byte>();

        var length = header.ReadValue<int>(EndianType.Big);
        if (reader.BytesRemaining < length + 5)
        {
            return FilterResult.Cache;
        }

        reader.Advance(5);
        var body = reader.GetSpan(length);
        var json = body.ToUtf8String();
        switch (type)
        {
            case 1:
                request = JsonSerializer.Deserialize<MyPack1>(json);
                break;
            case 2:
                request = JsonSerializer.Deserialize<MyPack2>(json);
                break;
            default:
                request = null;
                break;
        }
        reader.Advance(length);
        return FilterResult.Success;
    }

    public override bool CanSendRequestInfo => true;

    public override void SendInput<TWriter>(ref TWriter writer, IRequestInfo requestInfo)
    {
        switch (requestInfo)
        {
            case MyPack1 pack1:
                {
                    WriterExtension.WriteValue(ref writer, (byte)1);
                    var data = JsonSerializer.Serialize(pack1);
                    WriterAnchor<TWriter> writerAnchor = new WriterAnchor<TWriter>(ref writer, 4);
                    WriterExtension.WriteNormalString(ref writer, data, Encoding.UTF8);
                    var span = writerAnchor.Rewind(ref writer, out var length);
                    span.WriteValue(length, EndianType.Big);
                    break;
                }
            case MyPack2 pack2:
                {
                    WriterExtension.WriteValue(ref writer, (byte)2);
                    var data = JsonSerializer.Serialize(pack2);
                    WriterAnchor<TWriter> writerAnchor = new WriterAnchor<TWriter>(ref writer, 4);
                    WriterExtension.WriteNormalString(ref writer, data, Encoding.UTF8);
                    var span = writerAnchor.Rewind(ref writer, out var length);
                    span.WriteValue(length, EndianType.Big);
                    break;
                }
            default:
                break;
        }
    }
}