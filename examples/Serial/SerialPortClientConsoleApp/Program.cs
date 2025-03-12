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
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace SerialPortClientConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var client = new SerialPortClient();
        client.Connecting = (client, e) => { return EasyTask.CompletedTask; };//即将连接到端口
        client.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到端口
        client.Closing = (client, e) => { return EasyTask.CompletedTask; };//即将从端口断开连接。此处仅主动断开才有效。
        client.Closed = (client, e) => { return EasyTask.CompletedTask; };//从端口断开连接，当连接不成功时不会触发。
        client.Received = async (c, e) =>
        {
            await Console.Out.WriteLineAsync(e.ByteBlock.Span.ToString(Encoding.UTF8));
        };

        await client.SetupAsync(new TouchSocketConfig()
             .SetSerialPortOption(new SerialPortOption()
             {
                 BaudRate = 9600,//波特率
                 DataBits = 8,//数据位
                 Parity = System.IO.Ports.Parity.None,//校验位
                 PortName = "COM1",//COM
                 StopBits = System.IO.Ports.StopBits.One//停止位
             })
             .SetSerialDataHandlingAdapter(() => new PeriodPackageAdapter(){ CacheTimeout = TimeSpan.FromMilliseconds(100)})
             .ConfigurePlugins(a =>
             {
                 a.Add<MyPlugin>();
             }));

        await client.ConnectAsync();

        //using (var receiver = client.CreateReceiver())
        //{
        //    while (true)
        //    {
        //        using (CancellationTokenSource tokenSource=new CancellationTokenSource(TimeSpan.FromSeconds(10)))
        //        {
        //            using (var receiverResult = await receiver.ReadAsync(tokenSource.Token))
        //            {
        //                if (receiverResult.IsCompleted)
        //                {
        //                    //断开
        //                }

        //                //按照适配器类型。此处可以获取receiverResult.ByteBlock或者receiverResult.RequestInfo
        //                await Console.Out.WriteLineAsync(receiverResult.ByteBlock.Span.ToString(Encoding.UTF8));
        //            }
        //        }
        //    }
        //}



        Console.WriteLine("连接成功");

        while (true)
        {
            await client.SendAsync(Console.ReadLine());
        }
    }
}

internal class MyClassPlugin : PluginBase, ISerialConnectedPlugin
{
    public async Task OnSerialConnected(ISerialPortSession client, ConnectedEventArgs e)
    {
        await e.InvokeNext();
    }
}

public class MyPlugin : PluginBase, ISerialReceivedPlugin
{
    public async Task OnSerialReceived(ISerialPortSession client, ReceivedDataEventArgs e)
    {
        //这里处理数据接收
        //根据适配器类型，e.ByteBlock与e.RequestInfo会呈现不同的值，具体看文档=》适配器部分。
        var byteBlock = e.ByteBlock;
        var requestInfo = e.RequestInfo;

        //e.Handled = true;//表示该数据已经被本插件处理，无需再投递到其他插件。

        
        await e.InvokeNext();
    }
}

