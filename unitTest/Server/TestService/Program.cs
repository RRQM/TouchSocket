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
using RRQMService.HTTP;
using RRQMService.JsonRpc;
using RRQMService.NAT;
using RRQMService.RPC;
using RRQMService.Ssl;
using RRQMService.TCP;
using RRQMService.UDP;
using RRQMService.XUnitTest;
using System;
using System.Collections.Specialized;
using TouchSocket.Core;
using TouchSocket.Core.IO;

namespace RRQMService
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
            NameValueCollection nameValue = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            nameValue.Add("a", "1");
            nameValue.Add("A", "2");
            var s = nameValue["2"];
            ConsoleAction consoleAction = new ConsoleAction();

            consoleAction.OnException += ConsoleAction_OnException;

            consoleAction.Add("0", "单元测试服务器", XUnitTestDemo.Start);

            consoleAction.Add("1.1", "TCP简单服务器", TCPDemo.StartSimpleTcpService);
            consoleAction.Add("1.2", "TCP流量性能测试服务器", TCPDemo.StartFlowPerformanceTcpService);
            consoleAction.Add("1.3", "TCP断线重连测试服务器", TCPDemo.StartBreakoutResumeTcpService);
            consoleAction.Add("1.4", "TCP连接性能测试服务器", TCPDemo.StartConnectPerformanceTcpService);
            consoleAction.Add("1.5", "TCP命令行服务器", TCPDemo.StartCommandLineTcpService);
            consoleAction.Add("1.6", "TCP端口复用服务器", TCPDemo.StartReuseAddressTcpService);
            consoleAction.Add("1.7", "TCPPipeline服务器", TCPDemo.StartPipelineTcpService);

            consoleAction.Add("2.1", "UDP简单服务器", UDPDemo.TestUdpSession);
            consoleAction.Add("2.2", "UDP组播服务器", UDPDemo.TestUdpMulticastGroup);
            consoleAction.Add("2.3", "UDP性能服务器", UDPDemo.TestUdpPerformance);
            consoleAction.Add("2.4", "UDP大数据包服务器", UDPDemo.TestUdpPackage);

            consoleAction.Add("3.1", "Ssl服务器", SslTCP.StartSslTcpService);

            consoleAction.Add("4.1", "HTTP、WebSocket、静态网页托管服务器", HttpDemo.Start);

            consoleAction.Add("5.1", "RRQM TcpRPC服务器", RpcDemo.StartRPCService);
            consoleAction.Add("5.2", "RRQM HttpRPC服务器", HttpRpcDemo.CreateRRQMHttpParser);

            consoleAction.Add("6.1", "测试反向RPC性能", ReverseRpcDemo.TestPerformance);
            consoleAction.Add("6.2", "测试反向RPC,与正向RPC同时调用性能", ReverseRpcDemo.TestSleepPerformance);

            consoleAction.Add("7.1", "测试JsonRpc调用性能", JsonRpcDemo.Start);
            consoleAction.Add("8.1", "测试NAT", NATDemo.Start);

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