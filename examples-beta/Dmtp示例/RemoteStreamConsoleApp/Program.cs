using TouchSocket.Core;
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;
using TouchSocket.Dmtp.RemoteStream;
using System.Text;

namespace RemoteStreamConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var service = GetTcpDmtpService();

            var client = GetTcpDmtpClient();

            Metadata metadata = new Metadata();
            metadata.Add("tag", "tag1");

            var remoteStream = client.GetDmtpRemoteStreamActor().LoadRemoteStream(metadata);

            client.Logger.Info("已经成功载入流，请输入任意字符");
            remoteStream.Write(Encoding.UTF8.GetBytes(Console.ReadLine()));
            //remoteStream.Close();
            remoteStream.Dispose();
            Console.ReadKey();
        }

        static TcpDmtpClient GetTcpDmtpClient()
        {
            TcpDmtpClient client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7789")
               .SetVerifyToken("Dmtp")
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRemoteStream();
               }));
            client.Connect();
            return client;
        }

        static TcpDmtpService GetTcpDmtpService()
        {
            var service = new TouchSocketConfig()//配置
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
                   .SetVerifyToken("Dmtp")//连接验证口令。
                   .BuildWithTcpDmtpService();//此处build相当于new TcpDmtpService，然后Setup，然后Start。
            service.Logger.Info("服务器成功启动");
            return service;
        }

    }

    class MyRemoteStreamPlugin : PluginBase, IDmtpRemoteStreamPlugin<ITcpDmtpSocketClient>
    {
        async Task IDmtpRemoteStreamPlugin<ITcpDmtpSocketClient>.OnLoadingStream(ITcpDmtpSocketClient client, LoadingStreamEventArgs e)
        {
            if (e.Metadata["tag"] == "tag1")
            {
                e.IsPermitOperation = true;//需要运行操作

                client.Logger.Info("开始载入流");
                //当请求方请求映射流的时候，会触发此方法。
                using (var stream = new MemoryStream())
                {
                    await e.WaitingLoadStreamAsync(stream, TimeSpan.FromSeconds(60));

                    client.Logger.Info($"载入的流已被释放，流中信息：{Encoding.UTF8.GetString(stream.ToArray())}");
                }
               
                return;
            }

            //如果不满足，调用下一个插件
            await e.InvokeNext();
        }
    }
}