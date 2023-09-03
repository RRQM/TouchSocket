using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace DmtpConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var service = CreateTcpDmtpService();
            Connect_1();
            await Connect_2();

            Console.ReadKey();
        }

        static async Task Connect_2()
        {
            using var tcpClient = new TcpClient();//创建一个普通的tcp客户端。
            tcpClient.Received = (client, byteBlock, requestInfo) =>
            {
                //此处接收服务器返回的消息
                var flags = byteBlock.ReadUInt16(bigEndian: true);
                var length = byteBlock.ReadInt32(bigEndian: true);

                var json = Encoding.UTF8.GetString(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);

                ConsoleLogger.Default.Info($"收到响应：flags={flags}，length={length}，json={json}");
            };

            //开始链接服务器
            tcpClient.Connect("127.0.0.1:7789");

            //以json的数据方式。
            //其中Token、Metadata为连接的验证数据，分别为字符串、字符串字典类型。
            //Id则表示指定的默认id，字符串类型。
            //Sign为本次请求的序号，一般在连接时指定一个大于0的任意数字即可。
            var json = @"{""Token"":""Dmtp"",""Metadata"":{""a"":""a""},""Id"":null,""Sign"":1}";

            //将json转为utf-8编码。
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            using (var byteBlock = new ByteBlock())
            {
                //按照Flags+Length+Data的格式。
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)1));
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((int)jsonBytes.Length));
                byteBlock.Write(jsonBytes);

                tcpClient.Send(byteBlock);
            }

            await Task.Delay(2000);
        }

        /// <summary>
        /// 使用已封装的客户端执行：
        /// 1、设置Token验证连接。
        /// 2、设置Metadata，可以传递更多的验证信息。
        /// 3、设置默认Id。
        /// </summary>
        static void Connect_1()
        {
            using var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetDefaultId("defaultId")//设置默认Id
                .SetMetadata(new Metadata().Add("a", "a"))//设置Metadata，可以传递更多的验证信息
                .SetVerifyToken("Dmtp"));//设置Token验证连接
            client.Connect();

            client.Logger.Info($"{nameof(Connect_1)}连接成功，Id={client.Id}");
        }

        static TcpDmtpService CreateTcpDmtpService()
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
            if (e.DmtpMessage.ProtocolFlags == 10)
            {
                //判断完协议以后，从 e.DmtpMessage.BodyByteBlock可以拿到实际的数据
                string msg = e.DmtpMessage.BodyByteBlock.ToString();

                return;
            }

            //flags不满足，调用下一个插件
            await e.InvokeNext();
        }
    }
}