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
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.RemoteStream;
using TouchSocket.Sockets;

namespace RemoteStreamConsoleApp;

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

        #region 远程流映射请求端
        //元数据可以传递一些字符串数据
        var metadata = new Metadata();
        metadata.Add("tag", "tag1");

        var remoteStream = await client.GetDmtpRemoteStreamActor().LoadRemoteStreamAsync(metadata, CancellationToken.None);
        #endregion

        client.Logger.Info("已经成功载入流，请输入任意字符");

        //可以持续写入流，但在此处只写入了一次
        var input = Console.ReadLine();
        if (input != null)
        {
            await remoteStream.WriteAsync(Encoding.UTF8.GetBytes(input));
        }

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
             .SetDmtpOption(options =>
             {
                 options.VerifyToken = "Dmtp";
             })
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .ConfigurePlugins(a =>
             {
                 a.UseDmtpRemoteStream();
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
                   #region 远程流映射启用插件
                   a.UseDmtpRemoteStream();//必须添加远程流访问插件
                   #endregion
                   a.Add<MyRemoteStreamPlugin>();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Dmtp";//连接验证口令。
               })
               .BuildServiceAsync<TcpDmtpService>();//此处build相当于new TcpDmtpService，然后SetupAsync，然后StartAsync。
        service.Logger.Info("服务器成功启动");
        return service;
    }

    #region 远程流映射读写操作
    private static async Task RemoteStreamReadWrite(TcpDmtpClient client)
    {
        var metadata = new Metadata();
        metadata.Add("tag", "tag1");
        var remoteStream = await client.GetDmtpRemoteStreamActor().LoadRemoteStreamAsync(metadata, CancellationToken.None);

        byte[] data = new byte[] { 0, 1, 2, 3, 4 };
        await remoteStream.WriteAsync(data);

        remoteStream.Position = 0;
        byte[] buffer = new byte[5];
        await remoteStream.ReadAsync(buffer);

        remoteStream.Dispose();
    }
    #endregion
}

#region 远程流映射响应端插件
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

            this.m_logger.Info("开始载入流");
            //当请求方请求映射流的时候，会触发此方法。
            //此处加载的是一个内存流，实际上只要是Stream，都可以，例如：FileStream
            using (var stream = new MemoryStream())
            {
                await e.WaitingLoadStreamAsync(stream, CancellationToken.None);

                this.m_logger.Info($"载入的流已被释放，流中信息：{Encoding.UTF8.GetString(stream.ToArray())}");
            }

            return;
        }

        //如果不满足，调用下一个插件
        await e.InvokeNext();
    }
}
#endregion