using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Sockets;

namespace FileTransferConsoleApp
{
    internal class MyPlugin : PluginBase, IDmtpFileTransferingPlugin<ITcpDmtpSocketClient>, IDmtpFileTransferedPlugin<ITcpDmtpSocketClient>
    {
        async Task IDmtpFileTransferedPlugin<ITcpDmtpSocketClient>.OnDmtpFileTransfered(ITcpDmtpSocketClient client, FileTransferedEventArgs e)
        {
            //传输结束，但是不一定成功，甚至该方法都不一定会被触发，具体信息需要从e.Result判断状态。
            client.Logger.Info($"客户端传输文件结束，ID={client.Id}，请求类型={e.TransferType}，文件名={e.ResourcePath}，请求状态={e.Result}");
            await e.InvokeNext();
        }

        async Task IDmtpFileTransferingPlugin<ITcpDmtpSocketClient>.OnDmtpFileTransfering(ITcpDmtpSocketClient client, FileTransferingEventArgs e)
        {
            foreach (string item in e.Metadata.Keys)
            {
                Console.WriteLine($"Key={item},Value={e.Metadata[item]}");
            }
            e.IsPermitOperation = true;//每次传输都需要设置true，表示允许传输
            //有可能是上传，也有可能是下载
            client.Logger.Info($"有客户端请求传输文件，ID={client.Id}，请求类型={e.TransferType}，请求文件名={e.ResourcePath}");
            await e.InvokeNext();
        }
    }

    internal class Program
    {
        private static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TouchSocketConfig()
                   .SetRemoteIPHost("127.0.0.1:7789")
                   .SetVerifyToken("File")
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpFileTransfer()//必须添加文件传输插件
                       .SetProtocolFlags(30);//当协议和其他功能插件有冲突的时候，可以考虑重新指定协议，但需要注意的是，必须和服务器一致
                   })
                   .BuildWithTcpDmtpClient();

            client.Logger.Info("连接成功");
            return client;
        }

        private static TcpDmtpService GetTcpDmtpService()
        {
            var service = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddFileLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpFileTransfer()//必须添加文件传输插件
                      .SetProtocolFlags(30);//当协议和其他功能插件有冲突的时候，可以考虑重新指定协议，但需要注意的是，必须和客户端一致
                       a.Add<MyPlugin>();
                   })
                   .SetVerifyToken("File")//连接验证口令。
                   .BuildWithTcpDmtpService();//此处build相当于new TcpDmtpService，然后Setup，然后Start。
            service.Logger.Info("服务器成功启动");
            return service;
        }

        private static void Main(string[] args)
        {
            var service = GetTcpDmtpService();
            var client = GetTcpDmtpClient();

            var filePath = "File.Test";
            var saveFilePath = "SaveFile.Test";
            if (!File.Exists(filePath))//创建测试文件
            {
                using (var stream = File.OpenWrite(filePath))
                {
                    stream.SetLength(1024 * 1024 * 10);
                }
            }

            var metadata = new Metadata();//传递到服务器的元数据
            metadata.Add("1", "1");
            metadata.Add("2", "2");

            var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
            {
                SavePath = saveFilePath,//保存路径
                ResourcePath = filePath,//请求路径
                Metadata = metadata,//传递到服务器的元数据
                Timeout = TimeSpan.FromSeconds(60),//传输超时时长
                TryCount = 10,//当遇到失败时，尝试次数
                FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
            };

            fileOperator.SetMaxSpeed(1024 * 1024);

            //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
            var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
            {
                if (fileOperator.Result.ResultCode != ResultCode.Default)
                {
                    loop.Dispose();
                }
                client.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
            });

            loopAction.RunAsync();

            //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
            IResult result = client.GetDmtpFileTransferActor().PullFile(fileOperator);

            client.Logger.Info(result.ToString());
            Console.ReadKey();
        }
    }
}