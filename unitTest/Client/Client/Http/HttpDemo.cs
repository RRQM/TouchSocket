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
using System.IO;
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;
using TouchSocket.Http;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace RRQMClient.Http
{
    public class HttpDemo
    {
        public static void Start()
        {
            Console.WriteLine("1.简单Http测试");
            switch (Console.ReadLine())
            {
                case "1":
                    {
                        TestHttp();
                        break;
                    }
                default:
                    break;
            }
        }

        public static void TestHttp()
        {
            HttpClient client = new HttpClient();
            client.Setup(new TouchSocketConfig()
                .UsePlugin()
                .SetMaxPackageSize(1024 * 1024 * 10)
                .SetRemoteIPHost(new IPHost("http://127.0.0.1:8848")))
                .Connect();

            HttpRequest request = new HttpRequest();
            request
                .InitHeaders()
                .SetUrl("/home/DownloadBigFile")
                .SetHost(client.RemoteIPHost.Host)
                .AsGet();

            var respose = client.Request(request, timeout: 1000000);

            using (FileStream stream = new FileStream(@"C:\Users\17516\Desktop\新建文件夹\1.iso", FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = new byte[1024];
                long len = 0;
                while (true)
                {
                    int r = respose.Read(buffer, 0, buffer.Length);
                    len += r;
                    stream.Write(buffer, 0, r);
                    stream.Flush();
                    if (r == 0)
                    {
                        break;
                    }
                }
            }

            //Console.WriteLine(respose.GetBody());
        }
    }

    internal class MyClass : TcpPluginBase
    {
        protected override void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            Console.WriteLine(e.RequestInfo);
            base.OnReceivedData(client, e);
        }
    }
}