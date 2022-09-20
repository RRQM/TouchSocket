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
using RRQMClient.RPC;
using RRQMClient.Ssl;
using RRQMClient.TCP;
using RRQMClient.UDP;
using System;
using TouchSocket.Core;
using TouchSocket.Core.IO;

namespace RRQMClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch
            {
            }
            ConsoleAction consoleAction = new ConsoleAction();
            consoleAction.OnException += ConsoleAction_OnException;
            consoleAction.Add("1.1", "TCP简单客户端", TCPDemo.StartSimpleTcpClient);
            consoleAction.Add("1.2", "TCP流量性能测试客户端", TCPDemo.StartFlowPerformanceTcpClient);
            consoleAction.Add("1.3", "TCP断线重连性能测试客户端", TCPDemo.CreateBreakResumeTcpClient);
            consoleAction.Add("1.4", "TCP连接性能测试客户端", TCPDemo.StartConnectPerformanceTcpClient);
            consoleAction.Add("1.5", "TCP同步返回客户端（服务器需及时返回）", TCPDemo.CreateSendThenReturnTcpClient);
            consoleAction.Add("1.6", "TCP轮询断线重连性能测试客户端", TCPDemo.CreatePollBreakResumeTcpClient);

            consoleAction.Add("2.1", "UDP简单客户端", UDPDemo.TestUdpSession);
            consoleAction.Add("2.2", "UDP性能测试客户端", UDPDemo.TestUdpPerformance);
            consoleAction.Add("2.3", "UDP大数据包客户端", UDPDemo.TestUdpPackage);

            consoleAction.Add("3.1", "Ssl客户端", SslTCP.StartSimpleTcpClient);

            consoleAction.Add("4.1", "TcpRpc 性能测试 ", RPCDemo.Test_PerformanceRpcServer);
            consoleAction.Add("4.2", "TcpRpc 单客户端性能测试 ", RPCDemo.Test_ConPerformance);
            consoleAction.Add("4.3", "TcpRpc 反向RPC客户端性能测试 ", RPCDemo.Test_IDInvoke);
            consoleAction.Add("4.4", "TcpRpc 客户端连接性能测试 ", RPCDemo.StartConnectPerformanceTokenClient);
            consoleAction.Add("4.5", "HttpRpc 客户端连接性能测试 ", HttpRPCDemo.StartConnectPerformanceTokenClient);

            consoleAction.Add("5.1", "WebSocket 客户端连接测试 ", WebSocket.WebSocketDemo.TestClient);
            consoleAction.Add("5.2", "WebSocket 代理客户端连接测试 ", WebSocket.WebSocketDemo.TestWSProxyClient);

            consoleAction.Add("6.1", "Http 客户端连接测试 ", Http.HttpDemo.TestHttp);

            consoleAction.ShowAll();

            while (!consoleAction.Run(Console.ReadLine()))
            {
                Console.WriteLine("没有这个指令");
            }

            Console.ReadKey();
        }

        private static void ConsoleAction_OnException(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }
    }
}