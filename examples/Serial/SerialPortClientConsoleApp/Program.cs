using System.Text;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace SerialPortClientConsoleApp
{
    internal class Program
    {
        static async void Main(string[] args)
        {
            var client = new SerialPortClient();
            client.Connecting = (client, e) => { return EasyTask.CompletedTask; };//即将连接到端口
            client.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到端口
            client.Disconnecting = (client, e) => { return EasyTask.CompletedTask; };//即将从端口断开连接。此处仅主动断开才有效。
            client.Disconnected = (client, e) => { return EasyTask.CompletedTask; };//从端口断开连接，当连接不成功时不会触发。
            client.Received = async (c, e) =>
            {
                await Console.Out.WriteLineAsync(Encoding.UTF8.GetString(e.ByteBlock, 0, e.ByteBlock.Len));
            };

            client.Setup(new TouchSocketConfig()
                .SetSerialPortOption(new SerialPortOption()
                {
                    BaudRate = 9600,//波特率
                    DataBits = 8,//数据位
                    Parity = System.IO.Ports.Parity.None,//校验位
                    PortName = "COM1",//COM
                    StopBits = System.IO.Ports.StopBits.One//停止位
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlugin>();
                }));

            client.Connect();

            //using (var receiver = client.CreateReceiver())
            //{
            //    while (true)
            //    {
            //        using (var receiverResult = await receiver.ReadAsync(CancellationToken.None))
            //        {
            //            if (receiverResult.IsClosed)
            //            {
            //                //断开
            //            }
            //            //按照适配器类型。此处可以获取receiverResult.ByteBlock或者receiverResult.RequestInfo
            //            await Console.Out.WriteLineAsync(Encoding.UTF8.GetString(receiverResult.ByteBlock, 0, receiverResult.ByteBlock.Len));
            //        }
            //    }
            //}

            Console.WriteLine("连接成功");

            while (true)
            {
                client.Send(Console.ReadLine());
            }
        }
    }

    class MyClassPlugin : PluginBase, ISerialConnectedPlugin
    {
        public async Task OnSerialConnected(ISerialPortClient client, ConnectedEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    public class MyPlugin : PluginBase, ISerialReceivedPlugin<ISerialPortClient>
    {
        public async Task OnSerialReceived(ISerialPortClient client, ReceivedDataEventArgs e)
        {
            //这里处理数据接收
            //根据适配器类型，e.ByteBlock与e.RequestInfo会呈现不同的值，具体看文档=》适配器部分。
            ByteBlock byteBlock = e.ByteBlock;
            IRequestInfo requestInfo = e.RequestInfo;

            //e.Handled = true;//表示该数据已经被本插件处理，无需再投递到其他插件。

            await e.InvokeNext();
        }
    }
}
