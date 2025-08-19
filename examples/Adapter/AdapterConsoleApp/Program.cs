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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AdapterConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();
        var client = await CreateClient();

        ConsoleLogger.Default.Info("输入任意内容，回车发送（将会循环发送10次）");
        while (true)
        {
            var str = Console.ReadLine();
            for (var i = 0; i < 10; i++)
            {
                await client.SendAsync(str);
            }
        }
    }

    private static async Task<TcpClient> CreateClient()
    {
        var client = new TcpClient();
        //载入配置
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetTcpDataHandlingAdapter(() => new MyDataHandleAdapter())
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();//添加一个日志注入
             }));

        await client.ConnectAsync();//调用连接，当连接不成功时，会抛出异常。
        client.Logger.Info("客户端成功连接");
        return client;
    }

    private static async Task<TcpService> CreateService()
    {
        var service = new TcpService();
        service.Received = (client, e) =>
        {
            //从客户端收到信息
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new PeriodPackageAdapter() { CacheTimeout = TimeSpan.FromSeconds(1) })
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })
             .ConfigurePlugins(a =>
             {
                 //a.Add();//此处可以添加插件
             }));
        await service.StartAsync();//启动
        service.Logger.Info("服务器已启动");
        return service;
    }
}

internal class MyDataHandleAdapter : SingleStreamDataHandlingAdapter
{

    

    protected override Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        return Task.CompletedTask;
    }


    public override bool CanSendRequestInfo => false;

    public override void SendInput<TWriter>(ref TWriter writer, in ReadOnlyMemory<byte> memory)
    {
        var length = memory.Length;
        if (length > byte.MaxValue)//超长判断
        {
            throw new OverlengthException("发送数据太长。");
        }

        WriterExtension.WriteValue(ref writer, (byte)length);
        writer.Write(memory.Span);
    }

    public override void SendInput<TWriter>(ref TWriter writer, IRequestInfo requestInfo)
    {
        if (requestInfo is not MyClass myClass)
        {
            throw new ArgumentException("请求信息必须是MyClass类型。", nameof(requestInfo));
        }
        var data = myClass.Data ?? Array.Empty<byte>();
        if (data.Length > byte.MaxValue)//超长判断
        {
            throw new OverlengthException("发送数据太长。");
        }

        WriterExtension.WriteValue(ref writer, (byte)data.Length);//先写长度
        WriterExtension.WriteValue(ref writer, (byte)myClass.DataType);//然后数据类型
        WriterExtension.WriteValue(ref writer, (byte)myClass.OrderType);//然后指令类型
        writer.Write(data);//再写数据
    }
}

internal class MyClass : IRequestInfo
{
    public OrderType OrderType { get; set; }
    public DataType DataType { get; set; }

    public byte[] Data { get; set; }
}

internal enum DataType : byte
{
    Down = 0,
    Up = 1
}

internal enum OrderType : byte
{
    Hold = 0,
    Go = 1
}