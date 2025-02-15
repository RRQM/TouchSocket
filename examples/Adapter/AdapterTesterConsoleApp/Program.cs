//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace AdapterTesterConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var action = new ConsoleAction();
        action.OnException += Action_OnException;
        action.Add("1", "测试Tcp适配器", TcpDataAdapterTester);

        action.ShowAll();
        await action.RunCommandLineAsync();
    }

    private static void Action_OnException(Exception obj)
    {
        Console.WriteLine(obj.Message);
    }

    private static void TcpDataAdapterTester()
    {
        //Tcp适配器测试
        //bufferLength的作用是模拟tcp接收缓存区，例如：

        //发送数据为{0,1,2,3,4}时
        //当bufferLength=1时，会先接收一个字节，然后适配器判断无法解析，然后缓存，然后再接收下一个字节，直到成功解析。
        //该模式能很好的模拟网络很差的环境。
        //当bufferLength=8时，会先接收{0,1,2,3,4,0,1,2}，然后适配器判断解析前五字节，然后缓存后三字节，然后再接收下一个续包，直到解析结束。

        for (var bufferLength = 1; bufferLength < 1024 * 10; bufferLength += 1024)
        {
            var isSuccess = true;
            var data = new byte[] { 0, 1, 2, 3, 4 };
            var tester = TouchSocket.Sockets.TcpDataAdapterTester.CreateTester(new FixedHeaderPackageAdapter()
             , bufferLength, async (byteBlock, requestInfo) =>
             {
                 //此处就是接收，如果是自定义适配器，可以将requestInfo强制转换为实际对象，然后判断数据的确定性
                 if (byteBlock.Length != 5 || (!byteBlock.ToArray().SequenceEqual(data)))
                 {
                     isSuccess = false;
                 }

                 await EasyTask.CompletedTask;
             });

            //data是发送的数据，因为此处使用的是固定包头适配器，
            //发送前适配器会自动添加包头，所以，此处只发送数据即可。
            //如果测试的是自定义适配器，发送前没有封装的话，就需要自行构建发送数据。
            //随后的两个参数，10,10是测试次数，和期望次数，一般这两个值是相等的。
            //意为：本次数据将循环发送10次，且会接收10次。不然此处会一直阻塞。
            //最后一个参数是测试的最大超时时间。
            var time = tester.Run(data, 10, 10, 1000 * 10);
            Thread.Sleep(1000);
            Console.WriteLine($"测试结束，状态:{isSuccess}，用时：{time}");
        }
        Console.WriteLine("测试结束");
    }
}