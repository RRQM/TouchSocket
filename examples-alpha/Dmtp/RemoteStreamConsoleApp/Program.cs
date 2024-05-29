using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.RemoteStream;
using TouchSocket.Sockets;

namespace RemoteStreamConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var service = await GetTcpDmtpService();

            var client = await GetTcpDmtpClient();

            //元数据可以传递一些字符串数据
            var metadata = new Metadata();
            metadata.Add("tag", "tag1");

            var remoteStream = await client.GetDmtpRemoteStreamActor().LoadRemoteStreamAsync(metadata);

            client.Logger.Info("已经成功载入流，请输入任意字符");

            //可以持续写入流，但在此处只写入了一次
            remoteStream.Write(Encoding.UTF8.GetBytes(Console.ReadLine()));

            //可以使用下列代码完成持续读流
            //while (true)
            //{
            //    byte[] buffer = new byte[1024*64];
            //    int r = remoteStream.Read(buffer);
            //    if (r==0)
            //    {
            //        break;
            //    }
            //}

            //使用完在此处直接释放
            //remoteStream.Close();
            remoteStream.Dispose();

            while (true)
            {
                Console.ReadKey();
            }
        }

        private static async Task<TcpDmtpClient> GetTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
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
                     a.UseDmtpRemoteStream();

                     a.UseDmtpHeartbeat()//使用Dmtp心跳
                     .SetTick(TimeSpan.FromSeconds(3))
                     .SetMaxFailCount(3);
                 }));
            await client.ConnectAsync();
            return client;
        }

        private static async Task<TcpDmtpService> GetTcpDmtpService()
        {
            var service = await new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRemoteStream();//必须添加远程流访问插件
                       a.Add<MyRemoteStreamPlugin>();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"//连接验证口令。
                   })
                   .BuildServiceAsync<TcpDmtpService>();//此处build相当于new TcpDmtpService，然后SetupAsync，然后StartAsync。
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }

    internal class MyRemoteStreamPlugin : PluginBase, IDmtpRemoteStreamPlugin
    {
        private readonly ILog m_logger;

        public MyRemoteStreamPlugin(ILog logger)
        {
            this.m_logger = logger;
        }
        public async Task OnLoadingStream(IDmtpActorObject client, LoadingStreamEventArgs e)
        {
            if (e.Metadata["tag"] == "tag1")
            {
                e.IsPermitOperation = true;//需要允许操作

                m_logger.Info("开始载入流");
                //当请求方请求映射流的时候，会触发此方法。
                //此处加载的是一个内存流，实际上只要是Stream，都可以，例如：FileStream
                using (var stream = new MemoryStream())
                {
                    await e.WaitingLoadStreamAsync(stream, TimeSpan.FromSeconds(60));

                    m_logger.Info($"载入的流已被释放，流中信息：{Encoding.UTF8.GetString(stream.ToArray())}");
                }

                return;
            }

            //如果不满足，调用下一个插件
            await e.InvokeNext();
        }
    }
}