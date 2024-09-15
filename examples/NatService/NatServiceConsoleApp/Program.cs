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

using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace NatServiceConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var service = new MyNatService();
            await service.SetupAsync(new TouchSocketConfig()
                 .SetListenIPHosts(7788)
                 .ConfigureContainer(a =>
                 {
                     a.AddLogger(logger =>
                     {
                         logger.AddConsoleLogger();
                         logger.AddFileLogger();
                     });
                 }));

            await service.StartAsync();

            service.Logger.Info("转发服务器已启动。已将7788端口转发到127.0.0.1:7789与127.0.0.1:7790地址");
            while (true)
            {
                Console.ReadKey();
            }
        }
    }

    internal class MyNatService : NatService<MyNatSessionClient>
    {
        protected override MyNatSessionClient NewClient()
        {
            return new MyNatSessionClient();
        }
    }

    class MyNatSessionClient : NatSessionClient
    {
        #region 抽象类必须实现
        protected override async Task OnNatConnected(ConnectedEventArgs e)
        {
            try
            {
                await this.AddTargetClientAsync(config =>
                {
                    config.SetRemoteIPHost("127.0.0.1:7789");
                    //还可以配置其他，例如断线重连，具体可看文档tcpClient部分
                });

                //也可以再添加个转发端，实现一对多转发
                await this.AddTargetClientAsync(config =>
                {
                    config.SetRemoteIPHost("127.0.0.1:7790");
                    //还可以配置其他，例如断线重连，具体可看文档tcpClient部分
                });
            }
            catch (Exception ex)
            {
                //目标客户端无法连接，也就是无法转发
                this.Logger.Exception(ex);
            }
        }

        protected override async Task OnTargetClientClosed(NatTargetClient client, ClosedEventArgs e)
        {
            //可以自己重连，或者其他操作

            //或者直接移除
            this.RemoveTargetClient(client);
            await EasyTask.CompletedTask;
        }
        #endregion

        #region 可选重写方法
        protected override Task OnNatReceived(ReceivedDataEventArgs e)
        {
            return base.OnNatReceived(e);
        }


        protected override Task OnTargetClientReceived(NatTargetClient client, ReceivedDataEventArgs e)
        {
            return base.OnTargetClientReceived(client, e);
        }
        #endregion
    }

    class MultipleToOneNatSessionClient : NatSessionClient
    {
        protected override async Task OnNatConnected(ConnectedEventArgs e)
        {
            await this.AddTargetClientAsync(MyClientClass.TargetClient);
        }

        protected override async Task OnTargetClientClosed(NatTargetClient client, ClosedEventArgs e)
        {
            //不做任何处理
            await e.InvokeNext();
        }
    }

    static class MyClientClass
    {
        public static NatTargetClient TargetClient { get; }

        //初始化步骤可以在任意地方先调用
        public static async Task InitAsync()
        {
            //使用独立模式初始化，这样当NatSessionClient断开时不会释放该资源
            var client = new NatTargetClient(true);
            await client.ConnectAsync("127.0.0.1:7789");
        }
    }
}