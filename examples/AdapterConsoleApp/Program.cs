using System;
using System.IO;
using RRQMCore.ByteManager;
using RRQMCore.IO;
using RRQMSocket;
using RRQMCore.Extensions;
using System.Text;

namespace AdapterConsoleApp
{
    class Program
    {
        /// <summary>
        /// Tcp内置适配器介绍，请看说明文档<see href="https://www.yuque.com/eo2w71/rrqm/dd2d6d011491561c2c0d6f9b904aad98"/>
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 1.黏、分包问题使用内置适配器即可解决。
            //● 正常数据处理适配器(NormalDataHandlingAdapter)
            //● 固定包头数据处理适配器(FixedHeaderPackageAdapter)
            //● 固定长度数据处理适配器(FixedSizePackageAdapter)
            //● 终止因子分割数据处理适配器(TerminatorPackageAdapter)


            ConsoleAction consoleAction = new ConsoleAction();
            consoleAction.OnException += ConsoleAction_OnException;
            consoleAction.Add("0", "启动服务器测试适配器", StartTcpService);
            consoleAction.Add("1", "原始适配器实现demo", TestRawDataHandlingAdapter);
            consoleAction.Add("2", "SGCC适配器实现demo", TestSGCCCustomDataHandlingAdapter);


            consoleAction.ShowAll();
            while (true)
            {
                if (!consoleAction.Run(Console.ReadLine()))
                {
                    Console.WriteLine("指令不正确。");
                }
            }
        }

        static void StartTcpService()
        {
            TcpService service = new TcpService();
            service.Connecting += (client, e) =>
            {
                //有客户端正在连接
                client.SetDataHandlingAdapter(new MyCustomBetweenAndDataHandlingAdapter());
            };
            service.Connected += (client, e) => { };//有客户端连接
            service.Disconnected += (client, e) => { };//有客户端断开连接
            service.Received += (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                if (requestInfo is MyBetweenAndRequestInfo info)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(info.Body));
                }
            };

            service.Setup(new RRQMConfig()//载入配置     
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .SetMaxCount(10000)
                .SetThreadCount(10))
                .Start();//启动

        }

        private static void ConsoleAction_OnException(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        static void TestRawDataHandlingAdapter()
        {
            for (int i = 0; i < 10; i++)
            {
                DataAdapterTester tester = DataAdapterTester.CreateTester(new RawDataHandlingAdapter(), new Random().Next(1, 1024));//用BufferLength模拟粘包，分包
                using ByteBlock block = new ByteBlock();
                block.Write((byte)1);//写入数据类型   这里并未写入数据长度，因为这个适配器在发送前会再封装一次。
                block.Write((byte)1);//写入数据指令
                byte[] buffer = new byte[100];
                new Random().NextBytes(buffer);
                block.Write(buffer);//写入数据
                byte[] data = block.ToArray();

                // 输出测试时间，用于衡量适配性能.
                // 测试100次，限时2秒完成
                Console.WriteLine(tester.Run(data, 100, 100, 1000 * 2).ToString());
            }

        }

        static void TestSGCCCustomDataHandlingAdapter()
        {
            string[] lines = File.ReadAllLines("SGCC测试数据.txt");
            foreach (var item in lines)
            {
                DataAdapterTester tester = DataAdapterTester.CreateTester(new SGCCCustomDataHandlingAdapter(), new Random().Next(1, 1024));//用BufferLength模拟粘包，分包
                using ByteBlock block = new ByteBlock();

                byte[] data = item.ByHexStringToBytes(" ");
                // 输出测试时间，用于衡量适配性能.
                // 测试100次，限时2秒完成
                Console.WriteLine(tester.Run(data, 100, 100, 1000 * 2).ToString());
            }

        }
    }
}
