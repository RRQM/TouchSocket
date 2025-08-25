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
        #region 创建串口客户端
        var client = new SerialPortClient();
        client.Connecting = (client, e) => { return EasyTask.CompletedTask; };//即将连接到端口
        client.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到端口
        client.Closing = (client, e) => { return EasyTask.CompletedTask; };//即将从端口断开连接。此处仅主动断开才有效。
        client.Closed = (client, e) => { return EasyTask.CompletedTask; };//从端口断开连接，当连接不成功时不会触发。

        #region 串口通过Received事件接收数据
        client.Received = async (c, e) =>
        {
            await Console.Out.WriteLineAsync(e.Memory.Span.ToString(Encoding.UTF8));
        };
        #endregion


        var config = new TouchSocketConfig();
        #region 串口通信参数的核心配置
        config.SetSerialPortOption(new SerialPortOption()
        {
            BaudRate = 9600,//波特率
            DataBits = 8,//数据位
            Parity = System.IO.Ports.Parity.None,//校验位
            PortName = "COM1",//COM
            StopBits = System.IO.Ports.StopBits.One,//停止位
            //其他配置                                 
        });
        #endregion

        #region 串口一般推荐周期适配器
        config.SetSerialDataHandlingAdapter(() => new PeriodPackageAdapter() { CacheTimeout = TimeSpan.FromMilliseconds(100) });
        #endregion

        #region 使用串口接收插件
        config.ConfigurePlugins(a =>
        {
            a.Add<MyPlugin>();
        });
        #endregion


        await client.SetupAsync(config);

        await client.ConnectAsync();

        Console.WriteLine("连接成功");
        #endregion


        while (true)
        {
            #region 串口发送数据
            //发送字符串
            await client.SendAsync(Console.ReadLine());

            //也可以发送字节
            await client.SendAsync(new byte[] { 0, 1, 2, 3, 4 });
            #endregion
        }
    }

    private static async Task CreateWithReadAsync()
    {
        #region 串口通过ReadAsync读取
        var client = new SerialPortClient();
        await client.SetupAsync(new TouchSocketConfig());//此处省略配置
        await client.ConnectAsync();

        using (var receiver = client.CreateReceiver())
        {
            while (true)
            {
                //客户端发送数据
                await client.SendAsync(Console.ReadLine());

                //设置10秒超时
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    //等待接收数据
                    using (var receiverResult = await receiver.ReadAsync(cts.Token))
                    {
                        if (receiverResult.IsCompleted)
                        {
                            //已断开
                            break;
                        }

                        //按照适配器类型。此处可以获取receiverResult.ByteBlock或者receiverResult.RequestInfo
                        await Console.Out.WriteLineAsync(receiverResult.Memory.Span.ToString(Encoding.UTF8));
                    }
                }
            }
        }
        #endregion

        #region 串口客户端关闭和释放
        //Close保证异步断开，且触发Closing和Closed事件，随后还可以再次Connect。
        await client.CloseAsync("断开消息");

        //Dispose释放资源，不再使用后调用。Dispose后无法再次Connect。
        client.Dispose();
        #endregion
    }
}

internal class MyClassPlugin : PluginBase, ISerialConnectedPlugin
{
    public async Task OnSerialConnected(ISerialPortSession client, ConnectedEventArgs e)
    {
        await e.InvokeNext();
    }
}

#region 创建串口使用插件接收数据
public class MyPlugin : PluginBase, ISerialReceivedPlugin
{
    public async Task OnSerialReceived(ISerialPortSession client, ReceivedDataEventArgs e)
    {
        //这里处理数据接收
        //根据适配器类型，e.Memory与e.RequestInfo会呈现不同的值，具体看文档=》适配器部分。
        var byteBlock = e.Memory;
        var requestInfo = e.RequestInfo;

        //e.Handled = true;//表示该数据已经被本插件处理，无需再投递到其他插件。

        //如果需要向串口发送数据
        if (client is ISerialPortClient serialPortClient)
        {
            await serialPortClient.SendAsync("hello");
        }
        await e.InvokeNext();
    }
}
#endregion


#region 继承实现串口客户端
class MySerialClient : SerialPortClientBase
{
    protected override async Task OnSerialReceived(ReceivedDataEventArgs e)
    {
        //此处逻辑单线程处理。

        //此处处理数据，功能相当于Received委托。
        string mes = e.Memory.Span.ToString(Encoding.UTF8);
        Console.WriteLine($"已接收到信息：{mes}");

        await base.OnSerialReceived(e);
    }
}
#endregion
