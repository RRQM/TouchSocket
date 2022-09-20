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
using System.Timers;
using TouchSocket.Core.Config;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RRQMService.RPC
{
    public static class HttpRpcDemo
    {
        private static HttpTouchRpcService httpRpcService;

        public static void CreateRRQMHttpParser()
        {
            httpRpcService = new HttpTouchRpcService();
            Timer timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            //声明配置
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .SetVerifyToken("123RPC")
                .SetVerifyTimeout(5 * 1000)
                .SetThreadCount(50);

            //载入配置
            httpRpcService.Setup(config);

            //启动服务
            httpRpcService.Start();

            Console.WriteLine($"Http解析器添加完成，VerifyToken=123RPC");
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"在线客户端：{httpRpcService.SocketClients.Count}");
        }
    }
}