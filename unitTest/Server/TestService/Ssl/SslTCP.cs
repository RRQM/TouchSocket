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
using TouchSocket.Sockets;

namespace RRQMService.Ssl
{
    public static class SslTCP
    {
        public static void StartSslTcpService()
        {
            TcpService service = new TcpService();

            service.Connected += (client, e) => { Console.WriteLine($"客户端{client.IP}:{client.Port}连接"); };

            service.Disconnected += (client, e) => { Console.WriteLine($"客户端{client.IP}:{client.Port}断开连接，原因：{e.Message}"); };

            service.Received += (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                string mes = byteBlock.ToString();
                Console.WriteLine($"已从{client.IP}:{client.Port}接收到信息：{mes}");//Name即IP+Port
                client.Send(byteBlock);
            };

            //载入配置
            service.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })
                .SetServiceSslOption(new ServiceSslOption()
                {
                    Certificate = new X509Certificate2("RRQMSocket.pfx", "RRQMSocket"),
                    SslProtocols = SslProtocols.Tls12
                }))
                .Start();

            Console.WriteLine($"Ssl服务器启动成功");
        }
    }
}