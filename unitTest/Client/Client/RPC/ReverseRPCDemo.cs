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
using RRQMProxy;
using System;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Run;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RRQMClient.RPC
{
    public static class ReverseRPCDemo
    {
        public static void Start()
        {
            Console.WriteLine("1.测试反向RPC性能");
            Console.WriteLine("2.测试RPC和反向RPC同时调用性能");
            switch (Console.ReadLine())
            {
                case "1":
                    {
                        TestPerformance();
                        break;
                    }
                case "2":
                    {
                        TestReversePerformance();
                        break;
                    }
                default:
                    break;
            }
        }

        private static void TestReversePerformance()
        {
            RpcStore service = new RpcStore(new Container());
            //service.ShareProxy(new IPHost(8848));//分享反向代理RPC代理文件，不使用代理时，可以不用。

            Console.WriteLine("输入测试连接数：");
            int count = int.Parse(Console.ReadLine());

            service.RegisterServer<ReverseCallbackServer>();//注册服务
            for (int i = 0; i < count; i++)
            {
                TcpTouchRpcClient client = new TcpTouchRpcClient();
                service.AddRpcParser($"client{i}", client);//添加解析

                client.Setup(new TouchSocketConfig()
                    .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                    .SetVerifyToken("123RPC"));
                client.Connect();
                Console.WriteLine($"{i}成功连接");

                EasyAction.TaskRun(client, a =>
                 {
                     int j = 0;
                     while (true)
                     {
                         if (j % 10000 == 0)
                         {
                             Console.WriteLine(j);
                         }
                         int value = a.Invoke<int>("ConPerformance", InvokeOption.WaitInvoke, j++);
                         if (value != j)
                         {
                             Console.WriteLine("调用结果不一致");
                         }
                     }
                 });
            }
        }

        private static void TestPerformance()
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .SetVerifyToken("123RPC")
                .ConfigureRpcStore((store) =>
                {
                    store.RegisterServer<ReverseCallbackServer>();
                }));
            client.Connect();
            Console.WriteLine("成功连接");
        }
    }

    public class ReverseCallbackServer : TouchSocket.Rpc.RpcServer
    {
        [TouchRpc]
        public int ConPerformance(int age)
        {
            return ++age;
        }
    }
}