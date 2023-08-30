using NewLife.Log;
using NewLife.Net;
using NewLife.Remoting;
using System.Net;
using TouchSocket.Core;

namespace RpcPerformanceConsoleApp
{
    public static class NewLifeRpc
    {
        public static void StartServer()
        {
            XTrace.UseConsole();
            var netUri = new NetUri(NetType.Tcp, IPAddress.Any, 5001);
            var server = new ApiServer(netUri)
            {
                //Log = XTrace.Log,
                //EncoderLog = XTrace.Log,
                //ShowError = true

                //不输出调用日志
            };
            server.Register<TestController>();
            server.Start();
            Console.WriteLine("NewLifeRpc启动成功");
        }

        public static void StartSumClient(int count)
        {
            var client = new ApiClient("tcp://127.0.0.1:5001")
            {
                //Log = XTrace.Log,
                //EncoderLog = XTrace.Log
            };

            var rs = client.Invoke<Int32>("Test/Sum", new { a = 10, b = 20 });//先试调一下，保证已经建立了完整的连接

            var timeSpan = TimeMeasurer.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    var rs = client.Invoke<Int32>("Test/Sum", new { a = i, b = i });
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
            var client = new ApiClient("tcp://127.0.0.1:5001")
            {
                //Log = XTrace.Log,
                //EncoderLog = XTrace.Log
            };


            var rs = client.Invoke<byte[]>("Test/GetBytes", new { a = 10 });//先试调一下，保证已经建立了完整的连接

            var timeSpan = TimeMeasurer.Run(() =>
            {
                for (var i = 1; i < count; i++)
                {
                    var rs = client.Invoke<byte[]>("Test/GetBytes", new { a = i });//测试10k数据
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
            var client = new ApiClient("tcp://127.0.0.1:5001")
            {
                //Log = XTrace.Log,
                //EncoderLog = XTrace.Log
            };


            var rs = client.Invoke<string>("Test/GetBigString");//先试调一下，保证已经建立了完整的连接
            var timeSpan = TimeMeasurer.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    var rs = client.Invoke<string>("Test/GetBigString");
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
