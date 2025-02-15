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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace HeartbeatConsoleApp;

internal class Program
{
    /// <summary>
    /// 示例心跳。
    /// 博客地址<see href="https://blog.csdn.net/qq_40374647/article/details/125598921"/>
    /// </summary>
    /// <param name="args"></param>
    private static async Task Main(string[] args)
    {
        var consoleAction = new ConsoleAction();

        //服务器
        var service = new TcpService();
        await service.SetupAsync(new TouchSocketConfig()//载入配置
                 .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                 .SetTcpDataHandlingAdapter(() => new MyFixedHeaderDataHandlingAdapter())
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();
                 })
                 .ConfigurePlugins(a =>
                 {
                     a.Add<HeartbeatAndReceivePlugin>();
                 }));
        await service.StartAsync();//启动
        service.Logger.Info("服务器成功启动");

        //客户端
        var tcpClient = new TcpClient();
        await tcpClient.SetupAsync(new TouchSocketConfig()
              .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
              .SetTcpDataHandlingAdapter(() => new MyFixedHeaderDataHandlingAdapter())
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  a.Add<HeartbeatAndReceivePlugin>();
              }));
        await tcpClient.ConnectAsync();
        tcpClient.Logger.Info("客户端成功连接");

        consoleAction.OnException += ConsoleAction_OnException;
        consoleAction.Add("1", "发送心跳", async () =>
          {
              await tcpClient.PingAsync();
          });
        consoleAction.Add("2", "发送数据", async () =>
          {
              await tcpClient.SendAsync(new MyRequestInfo()
              {
                  DataType = DataType.Data,
                  Data = Encoding.UTF8.GetBytes(Console.ReadLine())
              }
               .PackageAsBytes());
          });
        consoleAction.ShowAll();

        await consoleAction.RunCommandLineAsync();
    }

    private static void ConsoleAction_OnException(Exception obj)
    {
        Console.WriteLine(obj);
    }
}

#region 数据格式解析

internal class MyFixedHeaderDataHandlingAdapter : CustomFixedHeaderDataHandlingAdapter<MyRequestInfo>
{
    public override int HeaderLength => 3;

    public override bool CanSendRequestInfo => false;

    protected override MyRequestInfo GetInstance()
    {
        return new MyRequestInfo();
    }
}

internal class MyRequestInfo : IFixedHeaderRequestInfo
{
    public DataType DataType { get; set; }
    public byte[] Data { get; set; }

    public int BodyLength { get; private set; }

    public bool OnParsingBody(ReadOnlySpan<byte> body)
    {
        if (body.Length == this.BodyLength)
        {
            this.Data = body.ToArray();
            return true;
        }
        return false;
    }

    public bool OnParsingHeader(ReadOnlySpan<byte> header)
    {
        if (header.Length == 3)
        {
            this.BodyLength = TouchSocketBitConverter.Default.To<ushort>(header) - 1;
            this.DataType = (DataType)header[2];
            return true;
        }
        return false;
    }

    public void Package(ByteBlock byteBlock)
    {
        byteBlock.WriteUInt16((ushort)((this.Data == null ? 0 : this.Data.Length) + 1));
        byteBlock.WriteByte((byte)this.DataType);
        if (this.Data != null)
        {
            byteBlock.Write(this.Data);
        }
    }

    public byte[] PackageAsBytes()
    {
        using var byteBlock = new ByteBlock();
        this.Package(byteBlock);
        return byteBlock.ToArray();
    }

    public override string ToString()
    {
        return $"数据类型={this.DataType}，数据={(this.Data == null ? "null" : Encoding.UTF8.GetString(this.Data))}";
    }
}

internal enum DataType : byte
{
    Ping,
    Pong,
    Data
}

#endregion 数据格式解析

/// <summary>
/// 一个心跳计数器扩展。
/// </summary>
internal static class DependencyExtensions
{
    public static readonly DependencyProperty<Timer> HeartbeatTimerProperty =
        new("HeartbeatTimer", null);

    public static async Task<bool> PingAsync<TClient>(this TClient client) where TClient : ISender, ILoggerObject
    {
        try
        {
            await client.SendAsync(new MyRequestInfo() { DataType = DataType.Ping }.PackageAsBytes());
            return true;
        }
        catch (Exception ex)
        {
            client.Logger.Exception(ex);
        }

        return false;
    }

    public static async Task<bool> PongAsync<TClient>(this TClient client) where TClient : ISender, ILoggerObject
    {
        try
        {
            await client.SendAsync(new MyRequestInfo() { DataType = DataType.Pong }.PackageAsBytes());
            return true;
        }
        catch (Exception ex)
        {
            client.Logger.Exception(ex);
        }

        return false;
    }
}

internal class HeartbeatAndReceivePlugin : PluginBase, ITcpConnectedPlugin, ITcpClosedPlugin, ITcpReceivedPlugin
{
    private readonly int m_timeTick;
    private readonly ILog m_logger;

    [DependencyInject]
    public HeartbeatAndReceivePlugin(ILog logger, int timeTick = 1000 * 5)
    {
        this.m_timeTick = timeTick;
        this.m_logger = logger;
    }

    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        //此处可判断，如果不是客户端，则不用使用心跳。
        if (client.IsClient && client is ITcpClient tcpClient)
        {
            if (client.GetValue(DependencyExtensions.HeartbeatTimerProperty) is Timer timer)
            {
                timer.Dispose();
            }

            client.SetValue(DependencyExtensions.HeartbeatTimerProperty, new Timer(async (o) =>
            {
                await tcpClient.PingAsync();
            }, null, 0, this.m_timeTick));
        }


        await e.InvokeNext();
    }

    public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
    {
        if (client.GetValue(DependencyExtensions.HeartbeatTimerProperty) is Timer timer)
        {
            timer.Dispose();
            client.SetValue(DependencyExtensions.HeartbeatTimerProperty, null);
        }

        await e.InvokeNext();
    }

    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is MyRequestInfo myRequest)
        {
            this.m_logger.Info(myRequest.ToString());

            if (client is ITcpClient tcpClient)
            {
                if (myRequest.DataType == DataType.Ping)
                {
                    await tcpClient.PongAsync();
                }
            }

        }
        await e.InvokeNext();
    }
}