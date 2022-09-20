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
using RRQMService.RPC.Server;
using System;
using System.Timers;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RRQMService.RPC
{
    public static class RpcDemo
    {
        public static void StartRPCService()
        {

            var rpcService = CreateRRQMTcpParser(7789);

            //注册服务
            rpcService.RegisterServer<TestRpcServer>();
            rpcService.RegisterServer<PerformanceRpcServer>();
            rpcService.RegisterServer<ElapsedTimeRpcServer>();
            rpcService.RegisterServer<InstanceRpcServer>();
            rpcService.RegisterServer<GetCallerRpcServer>();

            //注册当前程序集的所有服务
            //rpcService.RegisterAllServer();
            Console.WriteLine("RPC服务已启动");

            Console.ReadKey();

            //ServerCellCode[] serverCellCodes = rpcService.GetProxyInfo(typeof(TouchRpcAttribute));
            //string code = CodeGenerator.ConvertToCode("Test", serverCellCodes);

            //Console.WriteLine(code);

            Console.ReadKey();
        }

        private static TcpTouchRpcService tcpRpcParser;

        private static IRpcParser CreateRRQMTcpParser(int port)
        {
            tcpRpcParser = new TcpTouchRpcService();
            Timer timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            //声明配置
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetVerifyToken("123RPC")
                .SetVerifyTimeout(5 * 1000)
                //.UseDelaySender()
                .SetThreadCount(50);

            //载入配置
            tcpRpcParser.Setup(config);

            //启动服务
            tcpRpcParser.Start();

            Console.WriteLine($"TCP解析器添加完成，端口号：{port}，VerifyToken=123RPC");
            return tcpRpcParser;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"在线客户端：{tcpRpcParser.SocketClients.Count}");
        }
    }
}