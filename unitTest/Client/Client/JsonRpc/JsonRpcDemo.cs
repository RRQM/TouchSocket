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
using System.Threading.Tasks;
using TouchSocket.Core.Config;
using TouchSocket.Core.Run;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RRQMClient.JsonRpc
{
    public static class JsonRpcDemo
    {
        public static void Start()
        {
            Console.WriteLine("1.测试Tcp_JsonRpc");
            Console.WriteLine("2.测试Http_JsonRpc");
            Console.WriteLine("3.性能测试");
            switch (Console.ReadLine())
            {
                case "1":
                    {
                        TestTcpJsonRpcParser();
                        break;
                    }
                case "2":
                    {
                        TestHttpJsonRpcParser();
                        break;
                    }
                case "3":
                    {
                        Test_PerformanceRpcServer();
                        break;
                    }
                default:
                    break;
            }
        }

        private static void Test_PerformanceRpcServer()
        {
            var jsonRpcClient = new TouchSocketConfig()
                  .SetJRPT(JRPT.Tcp)
                  .SetRemoteIPHost("127.0.0.1:7705")
                  .BuildWithJsonRpcClient();

            Console.WriteLine("连接成功");

            int count = 0;

            Task.Run(() =>
            {
                while (true)
                {
                    int p = count++;
                    int result = jsonRpcClient.Invoke<int>("Performance", InvokeOption.WaitInvoke, p);
                    if (result != p + 1)
                    {
                        Console.WriteLine("调用不一致。");
                    }
                }
            });
            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
            {
                Console.WriteLine($"调用{count}次");
                count = 0;
            });
            loopAction.RunAsync();
            Console.ReadKey();
        }

        private static void TestHttpJsonRpcParser()
        {
            var jsonRpcClient = new TouchSocketConfig()
                  .SetJRPT(JRPT.Tcp)
                  .SetRemoteIPHost("http://127.0.0.1:7706/jsonrpc")
                  .BuildWithJsonRpcClient();
            Console.WriteLine("连接成功");

            while (true)
            {
                string result = jsonRpcClient.Invoke<string>("TestJsonRpc", InvokeOption.WaitInvoke, Console.ReadLine());
                Console.WriteLine($"返回结果:{result}");
            }
        }

        private static void TestTcpJsonRpcParser()
        {
            var jsonRpcClient = new TouchSocketConfig()
                 .SetJRPT(JRPT.Tcp)
                 .SetRemoteIPHost("127.0.0.1:7705")
                 .BuildWithJsonRpcClient();
            jsonRpcClient.Connect();
            Console.WriteLine("连接成功");

            while (true)
            {
                string result = jsonRpcClient.Invoke<string>("TestJsonRpc", InvokeOption.WaitInvoke, Console.ReadLine());
                Console.WriteLine($"返回结果:{result}");
            }
        }
    }
}