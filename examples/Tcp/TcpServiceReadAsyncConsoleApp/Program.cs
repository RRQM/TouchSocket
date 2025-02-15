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

using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpServiceReadAsyncConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = new TcpService();
        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .ConfigureContainer(a =>//容器的配置顺序应该在最前面
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })
             .ConfigurePlugins(a =>
             {
                 a.UseCheckClear()
                 .SetCheckClearType(CheckClearType.All)
                 .SetTick(TimeSpan.FromSeconds(60))
                 .SetOnClose((c, t) =>
                 {
                     c.TryShutdown();
                     c.SafeClose("超时无数据");
                 });

                 a.Add<TcpServiceReceiveAsyncPlugin>();
             }));
        await service.StartAsync();//启动

        Console.ReadKey();
    }
}

internal class TcpServiceReceiveAsyncPlugin : PluginBase, ITcpConnectedPlugin
{
    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        if (client is ITcpSessionClient sessionClient)
        {
            //receiver可以复用，不需要每次接收都新建
            using (var receiver = sessionClient.CreateReceiver())
            {
                receiver.CacheMode = true;
                receiver.MaxCacheSize = 1024 * 1024;

                var rn = Encoding.UTF8.GetBytes("\r\n");
                while (true)
                {
                    //receiverResult每次接收完必须释放
                    using (var receiverResult = await receiver.ReadAsync(CancellationToken.None))
                    {
                        //收到的数据，此处的数据会根据适配器投递不同的数据。
                        var byteBlock = receiverResult.ByteBlock;
                        var requestInfo = receiverResult.RequestInfo;

                        if (receiverResult.IsCompleted)
                        {
                            //断开连接了
                            Console.WriteLine($"断开信息：{receiverResult.Message}");
                            return;
                        }

                        //在CacheMode下，byteBlock将不可能为null

                        var index = 0;
                        while (true)
                        {
                            var r = byteBlock.Span.Slice(index).IndexOf(rn);
                            if (r < 0)
                            {
                                break;
                            }

                            var str = byteBlock.Span.Slice(index, r).ToString(Encoding.UTF8);
                            Console.WriteLine(str);

                            index += rn.Length;
                            index += r;
                        }

                        byteBlock.Seek(index);
                    }
                }
            }
        }

        await e.InvokeNext();
    }
}
