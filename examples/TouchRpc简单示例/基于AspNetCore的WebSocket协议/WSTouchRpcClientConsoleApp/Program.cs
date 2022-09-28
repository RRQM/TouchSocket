using System;
using TouchSocket.Core.Config;
using TouchSocket.Core.Diagnostics;
using TouchSocket.Core.IO;
using TouchSocket.Rpc.TouchRpc.AspNetCore;
using TouchSocket.Sockets;

namespace WSTouchRpcClientConsoleApp
{
    internal class Program
    {
        private static WSTouchRpcClient client;

        private static void Main(string[] args)
        {
            client = new WSTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:5000/wstouchrpc"));
            client.ConnectAsync();

            ConsoleAction consoleAction = new ConsoleAction();
            consoleAction.Add("1", "测试Login", TestLogin);
            consoleAction.Add("2", "测试性能", TestPerformance);

            consoleAction.ShowAll();

            while (true)
            {
                consoleAction.Run(Console.ReadLine());
            }
        }

        private static void TestPerformance()
        {
            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                string key = "TouchRpcWebApplication.RpcProviders.TestServerProvider.Performance".ToLower();
                for (int i = 0; i < 1000; i++)
                {
                    client.Invoke<int>(key, null, i);
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }

        private static void TestLogin()
        {
            while (true)
            {
                string[] strs = Console.ReadLine().Split(' ');
                if (strs.Length != 2)
                {
                    Console.WriteLine("参数不对");
                    continue;
                }
                Console.WriteLine(client.Invoke<bool>("TouchRpcWebApplication.RpcProviders.TestServerProvider.Login".ToLower(), null, strs[0], strs[1]));
                Console.ReadKey();
            }
        }
    }
}