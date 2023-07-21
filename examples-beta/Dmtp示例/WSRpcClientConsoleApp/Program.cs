using RpcProxy;
using System;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
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
                for (int i = 0; i < 1000; i++)
                {
                    client.Performance(i);
                    if (i % 10 == 0)
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
                Console.WriteLine("请输入账号，密码，中间使用空格");
                string[] strs = Console.ReadLine().Split(' ');
                if (strs.Length != 2)
                {
                    Console.WriteLine("参数不对，请输入账号，密码，中间使用空格");
                    continue;
                }
                Console.WriteLine(client.Login(strs[0], strs[1]));
                Console.ReadKey();
            }
        }
    }
}