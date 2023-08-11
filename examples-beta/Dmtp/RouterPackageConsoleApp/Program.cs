using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.RouterPackage;
using TouchSocket.Sockets;

namespace RouterPackageConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
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
            var service = GetTcpDmtpService();
            var client = GetTcpDmtpClient();

            for (int i = 0; i < 100000; i++)
            {
                using (ByteBlock byteBlock = new ByteBlock(1024 * 512))
                {
                    //此处模拟一个大数据块，实际情况中请使用write写入实际数据。
                    byteBlock.SetLength(byteBlock.Capacity);
                    MyRequestPackage requestPackage = new MyRequestPackage()
                    {
                        ByteBlock = byteBlock,
                        Metadata = new Metadata() { { "a", "a" } }//传递一个元数据，用于传递一些字符串信息
                    };
                    var response = client.GetDmtpRouterPackageActor().Request<MyResponsePackage>(requestPackage);

                    client.Logger.Info(response.Message);
                }
            }


            Console.ReadKey();
        }

        private static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TouchSocketConfig()
                   .SetRemoteIPHost("127.0.0.1:7789")
                   .SetVerifyToken("Dmtp")
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRouterPackage();//添加路由包功能插件

                       a.UseAutoBufferLength();
                   })
                   .BuildWithTcpDmtpClient();

            client.Logger.Info("连接成功");
            return client;
        }

        private static TcpDmtpService GetTcpDmtpService()
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

                       a.Add<MyPlugin>();
                       a.UseAutoBufferLength();
                   })
                   .SetVerifyToken("Dmtp");//连接验证口令。

            service.Setup(config)
                .Start();
            service.Logger.Info("服务器成功启动");
            return service;
        }

        /// <summary>
        /// 定义请求包
        /// </summary>
        class MyRequestPackage : DmtpRouterPackage
        {
            /// <summary>
            /// 包尺寸大小。此值并非需要精准数值，只需要估计数据即可。其作用为申请内存池。所以数据应当大小合适。
            /// </summary>
            public override int PackageSize => 1024 * 1024;

            /// <summary>
            /// 自定义一个内存属性。
            /// </summary>
            public ByteBlock ByteBlock { get; set; }

            public override void PackageBody(ByteBlock byteBlock)
            {
                base.PackageBody(byteBlock);
                byteBlock.WriteByteBlock(this.ByteBlock);
            }

            public override void UnpackageBody(ByteBlock byteBlock)
            {
                base.UnpackageBody(byteBlock);
                this.ByteBlock = byteBlock.ReadByteBlock();
            }
        }

        /// <summary>
        /// 定义响应包
        /// </summary>
        class MyResponsePackage : DmtpRouterPackage
        {
            /// <summary>
            /// 包尺寸大小。此值并非需要精准数值，只需要估计数据即可。其作用为申请内存池。所以数据应当大小合适。
            /// </summary>
            public override int PackageSize => 1024;
        }

        class MyPlugin : PluginBase, IDmtpRouterPackagePlugin
        {
            private readonly ILog m_logger;

            public MyPlugin(ILog logger)
            {
                this.m_logger = logger;
            }
            async Task IDmtpRouterPackagePlugin<IDmtpActorObject>.OnReceivedRouterPackage(IDmtpActorObject client, RouterPackageEventArgs e)
            {
                m_logger.Info($"收到包请求");
               
                /*此处即可以获取到请求的包*/
                var response = e.ReadRouterPackage<MyRequestPackage>();
                response.ByteBlock.SafeDispose();//将使用完成的内存池回收。
                /*此处即可以获取到请求的包*/

                await e.ResponseAsync(new MyResponsePackage()
                {
                    Message = "Success"
                });
                m_logger.Info($"已响应包请求");

                //一般在当前插件无法处理时调用下一插件。此处则不应该调用
                //await e.InvokeNext();
            }
        }
    }
}