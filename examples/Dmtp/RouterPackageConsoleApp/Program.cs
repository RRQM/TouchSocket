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
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.RouterPackage;
using TouchSocket.Sockets;

namespace RouterPackageConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            //包模式功能是企业版功能
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            ConsoleLogger.Default.Info(ex.Message);
        }
        var service =await GetTcpDmtpService();

        var consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;

        consoleAction.Add("1", "测试自定义数据包", RequestMyResponsePackage);
        consoleAction.Add("2", "测试返回Result", RequestResult);

        consoleAction.ShowAll();
        await consoleAction.RunCommandLineAsync();
    }

    private static async Task RequestResult()
    {
        using var client = await GetTcpDmtpClient();
        using (var byteBlock = new ByteBlock(1024 * 512))
        {
            //此处模拟一个大数据块，实际情况中请使用write写入实际数据。
            byteBlock.SetLength(byteBlock.Capacity);
            var requestPackage = new MyRequestPackage()
            {
                ByteBlock = byteBlock,
                Metadata = new Metadata() { { "b", "b" } }//传递一个元数据，用于传递一些字符串信息
            };

            //发起请求，然后等待一个自定义的响应包。
            var response = await client.GetDmtpRouterPackageActor().RequestAsync(requestPackage);

            client.Logger.Info($"自定义响应成功，{response}");
        }
    }

    private static async Task RequestMyResponsePackage()
    {
        using var client = await GetTcpDmtpClient();
        using (var byteBlock = new ByteBlock(1024 * 512))
        {
            //此处模拟一个大数据块，实际情况中请使用write写入实际数据。
            byteBlock.SetLength(byteBlock.Capacity);
            var requestPackage = new MyRequestPackage()
            {
                ByteBlock = byteBlock,
                Metadata = new Metadata() { { "a", "a" } }//传递一个元数据，用于传递一些字符串信息
            };

            //发起请求，然后等待一个自定义的响应包。
            var response = await client.GetDmtpRouterPackageActor().RequestAsync<MyResponsePackage>(requestPackage);

            client.Logger.Info($"自定义响应成功，{response.Message}");
        }
    }

    private static void ConsoleAction_OnException(Exception obj)
    {
        Console.WriteLine(obj.Message);
    }

    private static async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var client = await new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7789")
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Dmtp"
               })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRouterPackage();//添加路由包功能插件
               })
               .BuildClientAsync<TcpDmtpClient>();

        client.Logger.Info("连接成功");
        return client;
    }

    private static async Task<TcpDmtpService> GetTcpDmtpService()
    {
        var service = new TcpDmtpService();

        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();

                   a.AddDmtpRouteService();//添加路由策略
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRouterPackage();//添加路由包功能插件

                   a.Add<MyPlugin1>();
                   a.Add<MyPlugin2>();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Dmtp"//连接验证口令。
               });

        await service.SetupAsync(config);
        await service.StartAsync();
        service.Logger.Info("服务器成功启动");
        return service;
    }

    /// <summary>
    /// 定义请求包
    /// </summary>
    private class MyRequestPackage : DmtpRouterPackage
    {
        /// <summary>
        /// 包尺寸大小。此值并非需要精准数值，只需要估计数据即可。其作用为申请内存池。所以数据应当大小合适。
        /// </summary>
        public override int PackageSize => 1024 * 1024;

        /// <summary>
        /// 自定义一个内存属性。
        /// </summary>
        public ByteBlock ByteBlock { get; set; }

        public override void PackageBody<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.PackageBody(ref byteBlock);
            WriterExtension.WriteByteBlock(ref byteBlock, this.ByteBlock);
        }

        public override void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.UnpackageBody(ref byteBlock);

            this.ByteBlock =ReaderExtension.ReadByteBlock(ref byteBlock);
        }
    }

    /// <summary>
    /// 定义响应包
    /// </summary>
    private class MyResponsePackage : DmtpRouterPackage
    {
        /// <summary>
        /// 包尺寸大小。此值并非需要精准数值，只需要估计数据即可。其作用为申请内存池。所以数据应当大小合适。
        /// </summary>
        public override int PackageSize => 1024;
    }

    private class MyPlugin1 : PluginBase, IDmtpRouterPackagePlugin
    {
        private readonly ILog m_logger;

        public MyPlugin1(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnReceivedRouterPackage(IDmtpActorObject client, RouterPackageEventArgs e)
        {
            if (e.Metadata?["a"] == "a")
            {
                this.m_logger.Info($"收到包请求");

                /*此处即可以获取到请求的包*/
                var response = e.ReadRouterPackage<MyRequestPackage>();
                response.ByteBlock.SafeDispose();//将使用完成的内存池回收。
                /*此处即可以获取到请求的包*/

                await e.ResponseAsync(new MyResponsePackage()
                {
                    Message = "Success"
                });
                this.m_logger.Info($"已响应包请求");
            }

            //一般在当前插件无法处理时调用下一插件。
            await e.InvokeNext();
        }
    }

    private class MyPlugin2 : PluginBase, IDmtpRouterPackagePlugin
    {
        private readonly ILog m_logger;

        public MyPlugin2(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnReceivedRouterPackage(IDmtpActorObject client, RouterPackageEventArgs e)
        {
            if (e.Metadata?["b"] == "b")
            {
                this.m_logger.Info($"收到包请求");

                /*此处即可以获取到请求的包*/
                var response = e.ReadRouterPackage<MyRequestPackage>();
                response.ByteBlock.SafeDispose();//将使用完成的内存池回收。
                /*此处即可以获取到请求的包*/

                await e.ResponseSuccessAsync();
                this.m_logger.Info($"已响应包请求");
            }

            //一般在当前插件无法处理时调用下一插件。
            await e.InvokeNext();
        }
    }
}