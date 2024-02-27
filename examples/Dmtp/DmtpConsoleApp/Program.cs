using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace DmtpConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleAction action = new ConsoleAction();
            action.Add("1", "测试连接", Connect_1);
            action.Add("2", "测试以普通Tcp连接", Connect_2);
            action.Add("3", "发送消息", Send);
            action.OnException += Action_OnException;
            var service = CreateTcpDmtpService();

            action.ShowAll();

            action.RunCommandLine();
        }

        private static void Action_OnException(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        static void Send()
        {
            using var client = new TcpDmtpClient();

            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    //此处使用委托注册插件。和类插件功能一样
                    a.Add(nameof(IDmtpReceivedPlugin.OnDmtpReceived), async (object s, DmtpMessageEventArgs e) =>
                    {
                        string msg = e.DmtpMessage.BodyByteBlock.ToString();
                        await Console.Out.WriteLineAsync($"收到服务器回信，协议{e.DmtpMessage.ProtocolFlags}收到信息，内容：{msg}");
                        await e.InvokeNext();
                    });
                })
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp",//设置Token验证连接
                    Id = "defaultId",//设置默认Id
                    Metadata = new Metadata().Add("a", "a")//设置Metadata，可以传递更多的验证信息
                }));
            client.Connect();

            client.Logger.Info($"{nameof(Connect_1)}连接成功，Id={client.Id}");

            //使用Dmtp送消息。必须指定一个protocol，这是一个ushort类型的值。
            //20以内的值，框架在使用，所以在发送时要指定一个大于20的值
            //同时需要注意，当Dmtp添加其他功能组件的时候，可能也会占用协议。
            //例如：
            //DmtpRpc会用[20,25)的协议。
            //文件传输会用[25,35)的协议。

            //此处使用1000，基本就不会冲突。
            client.Send(1000, Encoding.UTF8.GetBytes("hello"));
        }

        /// <summary>
        /// 使用普通tcp连接
        /// </summary>
        /// <returns></returns>
        static async void Connect_2()
        {
            using var tcpClient = new TcpClient();//创建一个普通的tcp客户端。
            tcpClient.Received = (client, e) =>
            {
                //此处接收服务器返回的消息

                var head = e.ByteBlock.ToArray(0, 2);
                e.ByteBlock.Seek(2, SeekOrigin.Begin);
                var flags = e.ByteBlock.ReadUInt16(EndianType.Big);
                var length = e.ByteBlock.ReadInt32(EndianType.Big);

                var json = Encoding.UTF8.GetString(e.ByteBlock.Buffer, e.ByteBlock.Pos, e.ByteBlock.CanReadLen);

                ConsoleLogger.Default.Info($"收到响应：flags={flags},length={length},json={json.Replace("\r\n", string.Empty).Replace(" ", string.Empty)}");
                return Task.CompletedTask;
            };

            #region 基础Flag协议
            Console.WriteLine($"{nameof(DmtpActor.P0_Close)}-flag-->{DmtpActor.P0_Close}");
            Console.WriteLine($"{nameof(DmtpActor.P1_Handshake_Request)}-flag-->{DmtpActor.P1_Handshake_Request}");
            Console.WriteLine($"{nameof(DmtpActor.P2_Handshake_Response)}-flag-->{DmtpActor.P2_Handshake_Response}");
            Console.WriteLine($"{nameof(DmtpActor.P3_ResetId_Request)}-flag-->{DmtpActor.P3_ResetId_Request}");
            Console.WriteLine($"{nameof(DmtpActor.P4_ResetId_Response)}-flag-->{DmtpActor.P4_ResetId_Response}");
            Console.WriteLine($"{nameof(DmtpActor.P5_Ping_Request)}-flag-->{DmtpActor.P5_Ping_Request}");
            Console.WriteLine($"{nameof(DmtpActor.P6_Ping_Response)}-flag-->{DmtpActor.P6_Ping_Response}");
            Console.WriteLine($"{nameof(DmtpActor.P7_CreateChannel_Request)}-flag-->{DmtpActor.P7_CreateChannel_Request}");
            Console.WriteLine($"{nameof(DmtpActor.P8_CreateChannel_Response)}-flag-->{DmtpActor.P8_CreateChannel_Response}");
            Console.WriteLine($"{nameof(DmtpActor.P9_ChannelPackage)}-flag-->{DmtpActor.P9_ChannelPackage}");
            #endregion


            #region 连接
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
                //按照Head+Flags+Length+Data的格式。
                byteBlock.Write(Encoding.ASCII.GetBytes("dm"));
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)1));
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((int)jsonBytes.Length));
                byteBlock.Write(jsonBytes);

                tcpClient.Send(byteBlock);
            }
            #endregion

            #region Ping

            json = "{\"Sign\":2,\"Route\":false,\"SourceId\":null,\"TargetId\":null}";
            jsonBytes = Encoding.UTF8.GetBytes(json);

            using (var byteBlock = new ByteBlock())
            {
                //按照Head+Flags+Length+Data的格式。
                byteBlock.Write(Encoding.ASCII.GetBytes("dm"));
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)5));
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((int)jsonBytes.Length));
                byteBlock.Write(jsonBytes);

                tcpClient.Send(byteBlock);
            }
            #endregion

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
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp",//设置Token验证连接
                    Id = "defaultId",//设置默认Id
                    Metadata = new Metadata().Add("a", "a")//设置Metadata，可以传递更多的验证信息
                }));
            client.Connect();

            client.Ping();

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
                       a.Add<MyFlagsPlugin>();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"//设定连接口令，作用类似账号密码
                   });

            service.Setup(config);
            service.Start();

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
            if (e.DmtpMessage.ProtocolFlags == 1000)
            {
                //判断完协议以后，从 e.DmtpMessage.BodyByteBlock可以拿到实际的数据
                string msg = e.DmtpMessage.BodyByteBlock.ToString();
                await Console.Out.WriteLineAsync($"从协议{e.DmtpMessage.ProtocolFlags}收到信息，内容：{msg}");

                //向客户端回发消息
                client.Send(1001, Encoding.UTF8.GetBytes("收到"));
                return;
            }

            //flags不满足，调用下一个插件
            await e.InvokeNext();
        }
    }
}