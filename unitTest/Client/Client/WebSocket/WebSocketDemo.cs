//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace RRQMClient.WebSocket
{
    public static class WebSocketDemo
    {
        private static void TestSubpackageClient()
        {
            WebSocketClient myWSClient = new WebSocketClient();
            //myWSClient.UseReconnection(-1,true);
            myWSClient.Setup("http://127.0.0.1:7789/ws");
            myWSClient.Connect();
            Console.WriteLine("连接成功");

            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            while (true)
            {
                Console.ReadKey();
                myWSClient.SubSendWithWS(data, 1, 8, 4);
            }
        }

        private static void TestWSsClient()
        {
            WebSocketClient myWSClient = new WebSocketClient();

            myWSClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("wss://127.0.0.1:7789/ws"))
                .SetClientSslOption(
                new ClientSslOption()
                {
                    ClientCertificates = new X509CertificateCollection() { new X509Certificate2("RRQMSocket.pfx", "RRQMSocket") },
                    SslProtocols = SslProtocols.Tls12,
                    TargetHost = "127.0.0.1",
                    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; }
                })
                .ConfigureContainer(a=> 
                {
                    a.SetSingletonLogger<ConsoleLogger>();
                }))
                .Connect();

            Console.WriteLine("连接成功");
            while (true)
            {
                myWSClient.SendWithWS(Console.ReadLine());
            }
        }

       public static void TestWSProxyClient()
        {
            WebSocketClient myWSClient = new WebSocketClient();
            myWSClient.Received += MyWSClient_Received;
            myWSClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("ws://121.40.165.18:8800"))
                .SetHttpProxy(new TouchSocket.Http.HttpProxy(new IPHost("http://113.125.75.203:8888")))
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger<ConsoleLogger>();
                }));
            myWSClient.Connect(1000*60);
            Console.WriteLine("连接成功");
            while (true)
            {
                myWSClient.SendWithWS(Console.ReadLine());
            }
        }


        public static void TestClient()
        {
            WebSocketClient myWSClient = new WebSocketClient();
            myWSClient.Handshaked += (client, e) =>
            {
                Console.WriteLine("成功连接");
            };
            myWSClient.Received += MyWSClient_Received;
            myWSClient.Setup("ws://121.40.165.18:8800");
            myWSClient.Connect();
            while (true)
            {
                myWSClient.SendWithWS(Console.ReadLine());
            }
        }

        private static void MyWSClient_Received(WebSocketClient client, WSDataFrame dataFrame)
        {
            switch (dataFrame.Opcode)
            {
                case WSDataType.Cont:
                    Console.WriteLine($"收到中间数据，长度为：{dataFrame.PayloadLength}");
                    break;

                case WSDataType.Text:
                    Console.WriteLine(dataFrame.ToText());
                    break;

                case WSDataType.Binary:
                    if (dataFrame.FIN)
                    {
                        Console.WriteLine($"收到二进制数据，长度为：{dataFrame.PayloadLength}");
                    }
                    else
                    {
                        Console.WriteLine($"收到未结束的二进制数据，长度为：{dataFrame.PayloadLength}");
                    }
                    break;

                case WSDataType.Close:
                    break;

                case WSDataType.Ping:
                    break;

                case WSDataType.Pong:
                    break;

                default:
                    break;
            }
        }
    }
}