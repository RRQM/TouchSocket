using System.Text;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace SerialPortClientConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new SerialPortClient();
            var tcpClient = new TcpClient();
            tcpClient.Connecting = (client, e) => { return EasyTask.CompletedTask; };//即将连接到服务器，此时已经创建socket，但是还未建立tcp
            tcpClient.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到服务器
            tcpClient.Disconnecting = (client, e) => { return EasyTask.CompletedTask; };//即将从服务器断开连接。此处仅主动断开才有效。
            tcpClient.Disconnected = (client, e) => { return EasyTask.CompletedTask; };//从服务器断开连接，当连接不成功时不会触发。
            tcpClient.Received = (client, e) =>
            {
                //从服务器收到信息。但是一般byteBlock和requestInfo会根据适配器呈现不同的值。
                var mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
                tcpClient.Logger.Info($"客户端接收到信息：{mes}");
                return EasyTask.CompletedTask;
            };
            client.Setup(new TouchSocket.Core.TouchSocketConfig()
                .SetSerialPortOption(new SerialPortOption()
                {
                    BaudRate = 9600,//波特率
                    DataBits = 8,//数据位
                    Parity = System.IO.Ports.Parity.None,//校验位
                    PortName = "COM1",//COM
                    StopBits = System.IO.Ports.StopBits.One//停止位
                }));

            client.Received = async (c, e) =>
            {
                await Console.Out.WriteLineAsync(Encoding.UTF8.GetString(e.ByteBlock,0,e.ByteBlock.Len));
            };

            client.Connect();
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
}
