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
using TouchSocket.Core.Run;
using TouchSocket.Rpc.WebApi;

namespace RRQMClient.WebApi
{
    public static class WebApiDemo
    {
        public static void Start()
        {
            Console.WriteLine("1.测试WebApiClient");
            Console.WriteLine("2.性能测试WebApiClient");
            switch (Console.ReadLine())
            {
                case "1":
                    {
                        TestClient();
                        break;
                    }
                case "2":
                    {
                        TestPerformance();
                        break;
                    }
                default:
                    break;
            }
        }

        private static void TestPerformance()
        {
            WebApiClient client = new WebApiClient();
            client.Setup("127.0.0.1:7801");
            client.Connect();
            Console.WriteLine("连接成功");

            int count = 0;
            Task.Run(() =>
            {
                while (true)
                {
                    int sum = client.Invoke<int>("GET:/XUnitTest/HttpGetSum?a={0}&b={1}", null, 10, 20);
                    count++;
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

        private static void TestClient()
        {
            WebApiClient client = new WebApiClient();
            client.Setup("127.0.0.1:7801");
            client.Connect();
            Console.WriteLine("连接成功");

            int sum = client.Invoke<int>("GET:/XUnitTest/HttpGetSum?a={0}&b={1}", null, 10, 20);
            Console.WriteLine($"调用成功，结果：{sum}");
        }
    }
}