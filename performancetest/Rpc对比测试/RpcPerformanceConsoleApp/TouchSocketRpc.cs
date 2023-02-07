using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RpcPerformanceConsoleApp
{
    public static class TouchSocketRpc
    {
        public static void StartServer()
        {
            var service = new TcpTouchRpcService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigureRpcStore(a =>
                   {
                       a.RegisterServer<TestController>();
                   })
                   .SetVerifyToken("TouchRpc");//设定连接口令，作用类似账号密码

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");
        }

        public static void StartSumClient(int count)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();

            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    var rs = client.Invoke<Int32>("Sum", InvokeOption.WaitInvoke, i, i);
                    if (rs != i + i)
                    {
                        Console.WriteLine("调用结果不一致");
                    }
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }

        public static void StartGetBytesClient(int count)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();

            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int i = 1; i < count; i++)
                {
                    var rs = client.Invoke<byte[]>("GetBytes", InvokeOption.WaitInvoke, i);//测试10k数据
                    if (rs.Length != i)
                    {
                        Console.WriteLine("调用结果不一致");
                    }
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }

        public static void StartBigStringClient(int count)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();


            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    var rs = client.Invoke<string>("GetBigString", InvokeOption.WaitInvoke);
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }
    }
}
