using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var service = CreateService();
            Connect_1();
            await Connect_2();

            Console.ReadKey();
        }

        static async Task Connect_2()
        {
            using var tcpClient = new TcpClient();
            tcpClient.Received = (client, byteBlock, requestInfo) =>
            {
                var flags = byteBlock.ReadUInt16(bigEndian: true);
                var length = byteBlock.ReadInt32(bigEndian: true);

                var json = Encoding.UTF8.GetString(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);

                ConsoleLogger.Default.Info($"收到响应：flags={flags}，length={length}，json={json}");
            };

            tcpClient.Connect("127.0.0.1:7789");
            var json = @"{""Token"":""Dmtp"",""Metadata"":{""a"":""a""},""Id"":null,""Message"":null,""Sign"":1,""Status"":0}";

            var jsonBytes = Encoding.UTF8.GetBytes(json);

            using (var byteBlock = new ByteBlock())
            {
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)1));
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((int)jsonBytes.Length));
                byteBlock.Write(jsonBytes);

                tcpClient.Send(byteBlock);
            }

            await Task.Delay(2000);
        }

        /// <summary>
        /// 动态验证连接。
        /// </summary>
        static void Connect_1()
        {
            using var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetMetadata(new Metadata().Add("a", "a"))
                .SetVerifyToken("Dmtp"));
            client.Connect();

            client.Logger.Info($"{nameof(Connect_1)}连接成功");
        }

        static TcpDmtpService CreateService()
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
                   })
                   .SetVerifyToken("Dmtp");//设定连接口令，作用类似账号密码

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");

            return service;
        }
    }

    internal class MyVerifyPlugin : PluginBase, IDmtpHandshakingPlugin
    {
        async Task IDmtpHandshakingPlugin<IDmtpActorObject>.OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
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
}