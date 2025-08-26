// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace WaitingClientConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }



    private static async Task TcpService_WaitingClient()
    {
        #region Tcp服务器WaitingClient示例
        var service = new TcpService();
        await service.StartAsync(7789);//启动服务器

        //在服务器中，找到指定Id的会话客户端
        if (service.TryGetClient("targetId", out var tcpSessionClient))
        {
            //调用CreateWaitingClient获取到IWaitingClient的对象。
            var waitClient = tcpSessionClient.CreateWaitingClient(new WaitingOptions()
            {
                FilterFunc = response => //设置用于筛选的fun委托，当返回为true时，才会响应返回
                {
                    return true;
                }
            });

            //发送数据，并等待响应
            using (var responsedData = await waitClient.SendThenResponseAsync(Encoding.UTF8.GetBytes("RRQM")))
            {
                var memory = responsedData.Memory;
                Console.WriteLine(memory.Span.ToString(Encoding.UTF8));

                var requestInfo = responsedData.RequestInfo;//如果适配器收到数据后，返回的并不是字节，而是IRequestInfo对象时
            }
        }
        #endregion
    }
    private static async Task TcpClient_WaitingClient()
    {
        #region Tcp客户端WaitingClient示例
        var client = new TcpClient();
        await client.ConnectAsync("tcp://127.0.0.1:7789");

        //调用CreateWaitingClient获取到IWaitingClient的对象。

        #region WaitingClient设置筛选函数
        var waitingClient = client.CreateWaitingClient(new WaitingOptions()
        {
            FilterFunc = response => //设置用于筛选的fun委托，当返回为true时，才会响应返回
            {
                var requestInfo = response.RequestInfo;
                var memory = response.Memory;

                //这里可以根据服务端返回的信息，判断是否响应
                return true;
            }
        });
        #endregion



        //发送数据，并等待响应
        using (var responsedData = await waitingClient.SendThenResponseAsync(Encoding.UTF8.GetBytes("RRQM")))
        {
            var memory = responsedData.Memory;
            Console.WriteLine(memory.Span.ToString(Encoding.UTF8));

            var requestInfo = responsedData.RequestInfo;//如果适配器收到数据后，返回的并不是字节，而是IRequestInfo对象时
        }
        #endregion

        #region WaitingClient默认超时示例
        using var responsed = await waitingClient.SendThenResponseAsync("hello");//默认5秒超时
        #endregion

        #region WaitingClient指定超时取消示例
        var cts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            await Task.Delay(5000);
            cts.Cancel();//5秒后取消等待，不再等待服务端的消息。这里模拟的是客户端主动取消等待
        });
        using var responsed2 = await waitingClient.SendThenResponseAsync("hello", cts.Token);
        #endregion

    }
}
