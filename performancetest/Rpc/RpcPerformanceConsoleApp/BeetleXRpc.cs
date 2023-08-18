using BeetleX.XRPC.Clients;
using BeetleX.XRPC.Hosting;
using BeetleX.XRPC.Packets;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace RpcPerformanceConsoleApp
{
    public static class BeetleXRpc
    {
        public static void StartServer()
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.UseXRPC(s =>
                    {
                        s.ServerOptions.LogLevel = BeetleX.EventArgs.LogType.Error;
                        s.ServerOptions.DefaultListen.Port = 9090;
                        s.RPCOptions.ParameterFormater = new MsgPacket();//default messagepack
                    },
                        typeof(Program).Assembly);
                });
            builder.Build().RunAsync();
        }

        public static void StartSumClient(int count)
        {
            XRPCClient client = new XRPCClient("127.0.0.1", 9090);
            client.Options.ParameterFormater = new MsgPacket();//default messagepack
            ITestTaskController testController = client.Create<ITestTaskController>();

            var rs = testController.Sum(10, 20);//试调一次，保持在线
            rs.Wait();

            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    var rs = testController.Sum(i, i);
                    rs.Wait();
                    if (rs.Result != i + i)
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
            XRPCClient client = new XRPCClient("127.0.0.1", 9090);
            client.Options.ParameterFormater = new MsgPacket();//default messagepack
            ITestTaskController testController = client.Create<ITestTaskController>();

            var rs = testController.GetBytes(10);//试调一次，保持在线
            rs.Wait();

            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int i = 1; i < count; i++)
                {
                    var rs = testController.GetBytes(i);//测试10k数据
                    rs.Wait();
                    if (rs.Result.Length != i)
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
            XRPCClient client = new XRPCClient("127.0.0.1", 9090);
            client.Options.ParameterFormater = new MsgPacket();//default messagepack
            ITestTaskController testController = client.Create<ITestTaskController>();

            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    var rs = testController.GetBigString();

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
