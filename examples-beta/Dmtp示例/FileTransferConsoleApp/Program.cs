using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Sockets;

namespace FileTransferConsoleApp
{

    internal class Program
    {
        /// <summary>
        /// 测试文件大小
        /// </summary>
        const long FileLength = 1024 * 1024 * 10L;

        /// <summary>
        /// 传输限速
        /// </summary>
        const int MaxSpeed = 1024 * 1024;

        private static void Main(string[] args)
        {
            var service = GetTcpDmtpService();
            var client = GetTcpDmtpClient();

            ClientPullFileFromService(client);
            ClientPushFileFromService(client);

            //此处，因为Dmtp组件，在客户端连接上服务器之后，客户端会与服务器的SocketClient同步Id。
            //详情：http://rrqm_home.gitee.io/touchsocket/docs/dmtpbaseconnection
            //所以此处直接使用客户端Id。

            ServicePullFileFromClient(service, client.Id);
            ServicePushFileFromClient(service, client.Id);
            Console.ReadKey();
        }

        /// <summary>
        /// 服务器往客户端主动推送文件
        /// </summary>
        /// <param name="service">服务器</param>
        /// <param name="targetId">服务器要请求的客户端Id</param>
        static void ServicePushFileFromClient(TcpDmtpService service, string targetId)
        {
            if (!service.TryGetSocketClient(targetId, out var socketClient))
            {
                throw new Exception($"没有找到Id={targetId}的客户端");
            }

            ConsoleLogger.Default.Info("开始从客户端下载文件");
            var filePath = "ServicePushFileFromClient.Test";
            var saveFilePath = "SaveServicePushFileFromClient.Test";
            if (!File.Exists(filePath))//创建服务器的测试文件
            {
                using (var stream = File.OpenWrite(filePath))
                {
                    stream.SetLength(FileLength);
                }
            }

            var metadata = new Metadata();//传递到客户端的元数据
            metadata.Add("1", "1");
            metadata.Add("2", "2");

            var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
            {
                SavePath = saveFilePath,//客户端本地保存路径
                ResourcePath = filePath,//服务器文件的资源路径
                Metadata = metadata,//传递到客户端的元数据
                Timeout = TimeSpan.FromSeconds(60),//传输超时时长
                TryCount = 10,//当遇到失败时，尝试次数
                FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
            };

            fileOperator.SetMaxSpeed(MaxSpeed);//设置最大限速为1Mb。

            //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
            var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
            {
                if (fileOperator.Result.ResultCode != ResultCode.Default)
                {
                    loop.Dispose();
                }
                socketClient.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
            });

            loopAction.RunAsync();

            //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
            IResult result = socketClient.GetDmtpFileTransferActor().PushFile(fileOperator);

            ConsoleLogger.Default.Info("服务器主动推送客户端文件结束");
            socketClient.Logger.Info(result.ToString());

            File.Delete(filePath);
            File.Delete(saveFilePath);
        }

        /// <summary>
        /// 服务器从客户端下载文件
        /// </summary>
        /// <param name="service">服务器</param>
        /// <param name="targetId">服务器要请求的客户端Id</param>
        static void ServicePullFileFromClient(TcpDmtpService service, string targetId)
        {
            if (!service.TryGetSocketClient(targetId, out var socketClient))
            {
                throw new Exception($"没有找到Id={targetId}的客户端");
            }

            ConsoleLogger.Default.Info("开始从客户端下载文件");
            var filePath = "ServicePullFileFromClient.Test";
            var saveFilePath = "SaveServicePullFileFromClient.Test";
            if (!File.Exists(filePath))//创建客户端的测试文件
            {
                using (var stream = File.OpenWrite(filePath))
                {
                    stream.SetLength(FileLength);
                }
            }

            var metadata = new Metadata();//传递到客户端的元数据
            metadata.Add("1", "1");
            metadata.Add("2", "2");

            var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
            {
                SavePath = saveFilePath,//服务器本地保存路径
                ResourcePath = filePath,//请求客户端文件的资源路径
                Metadata = metadata,//传递到客户端的元数据
                Timeout = TimeSpan.FromSeconds(60),//传输超时时长
                TryCount = 10,//当遇到失败时，尝试次数
                FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
            };

            fileOperator.SetMaxSpeed(MaxSpeed);//设置最大限速为1Mb。

            //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
            var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
            {
                if (fileOperator.Result.ResultCode != ResultCode.Default)
                {
                    loop.Dispose();
                }
                socketClient.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
            });

            loopAction.RunAsync();

            //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
            IResult result = socketClient.GetDmtpFileTransferActor().PullFile(fileOperator);

            ConsoleLogger.Default.Info("从客户端下载文件结束");
            socketClient.Logger.Info(result.ToString());

            File.Delete(filePath);
            File.Delete(saveFilePath);
        }

        /// <summary>
        /// 客户端从服务器下载文件。
        /// </summary>
        /// <param name="client"></param>
        static void ClientPullFileFromService(TcpDmtpClient client)
        {
            ConsoleLogger.Default.Info("开始从服务器下载文件");
            var filePath = "ClientPullFileFromService.Test";
            var saveFilePath = "SaveClientPullFileFromService.Test";
            if (!File.Exists(filePath))//创建服务器端的测试文件
            {
                using (var stream = File.OpenWrite(filePath))
                {
                    stream.SetLength(FileLength);
                }
            }

            var metadata = new Metadata();//传递到服务器的元数据
            metadata.Add("1", "1");
            metadata.Add("2", "2");

            var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
            {
                SavePath = saveFilePath,//客户端本地保存路径
                ResourcePath = filePath,//请求文件的资源路径
                Metadata = metadata,//传递到服务器的元数据
                Timeout = TimeSpan.FromSeconds(60),//传输超时时长
                TryCount = 10,//当遇到失败时，尝试次数
                FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
            };

            fileOperator.SetMaxSpeed(MaxSpeed);//设置最大限速为1Mb。

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

            ConsoleLogger.Default.Info("从服务器下载文件结束");
            client.Logger.Info(result.ToString());

            File.Delete(filePath);
            File.Delete(saveFilePath);
        }

        /// <summary>
        /// 客户端上传文件到服务器。
        /// </summary>
        /// <param name="client"></param>
        static void ClientPushFileFromService(TcpDmtpClient client)
        {
            ConsoleLogger.Default.Info("上传文件到服务器");
            var filePath = "ClientPushFileFromService.Test";
            var saveFilePath = "SaveClientPushFileFromService.Test";
            if (!File.Exists(filePath))//创建客户端的测试文件
            {
                using (var stream = File.OpenWrite(filePath))
                {
                    stream.SetLength(FileLength);
                }
            }

            var metadata = new Metadata();//传递到服务器的元数据
            metadata.Add("1", "1");
            metadata.Add("2", "2");

            var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
            {
                SavePath = saveFilePath,//服务器本地保存路径
                ResourcePath = filePath,//客户端本地即将上传文件的资源路径
                Metadata = metadata,//传递到服务器的元数据
                Timeout = TimeSpan.FromSeconds(60),//传输超时时长
                TryCount = 10,//当遇到失败时，尝试次数
                FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
            };

            fileOperator.SetMaxSpeed(MaxSpeed);//设置最大限速为1Mb。

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

            //此方法会阻塞，直到传输结束，也可以使用PushFileAsync
            IResult result = client.GetDmtpFileTransferActor().PushFile(fileOperator);

            ConsoleLogger.Default.Info("上传文件到服务器结束");
            client.Logger.Info(result.ToString());

            File.Delete(filePath);
            File.Delete(saveFilePath);
        }

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

                       a.Add<MyPlugin>();
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
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpFileTransfer()//必须添加文件传输插件
                      .SetProtocolFlags(30);//当协议和其他功能插件有冲突的时候，可以考虑重新指定协议，但需要注意的是，必须和客户端一致
                       a.Add<MyPlugin>();
                   })
                   .SetVerifyToken("File");//连接验证口令。

            service.Setup(config)
                .Start();
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }



    internal class MyPlugin : PluginBase, IDmtpFileTransferingPlugin, IDmtpFileTransferedPlugin
    {
        private readonly ILog m_logger;

        public MyPlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        /// <summary>
        /// 该方法，会在每个文件被请求（推送）结束时触发。传输不一定成功，具体信息需要从e.Result判断状态。
        /// 其次，该方法也不一定会被执行，例如：在传输过程中，直接断网，则该方法将不会执行。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        async Task IDmtpFileTransferedPlugin<IDmtpActorObject>.OnDmtpFileTransfered(IDmtpActorObject client, FileTransferedEventArgs e)
        {
            //传输结束，但是不一定成功，甚至该方法都不一定会被触发，具体信息需要从e.Result判断状态。
            m_logger.Info($"传输文件结束，请求类型={e.TransferType}，文件名={e.ResourcePath}，请求状态={e.Result}");
            await e.InvokeNext();
        }


        /// <summary>
        /// 该方法，会在每个文件被请求（推送）时第一时间触发。
        /// 当请求文件时，可以重新指定请求的文件路径，即对e.ResourcePath直接赋值。
        /// 当推送文件时，可以重新指定保存文件路径，即对e.SavePath直接赋值。
        /// 
        /// 注意：当文件夹不存在时，需要手动创建。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        async Task IDmtpFileTransferingPlugin<IDmtpActorObject>.OnDmtpFileTransfering(IDmtpActorObject client, FileTransferingEventArgs e)
        {
            foreach (var item in e.Metadata.Keys)
            {
                Console.WriteLine($"Key={item},Value={e.Metadata[item]}");
            }
            e.IsPermitOperation = true;//每次传输都需要设置true，表示允许传输
            //有可能是上传，也有可能是下载
            m_logger.Info($"请求传输文件，请求类型={e.TransferType}，请求文件名={e.ResourcePath}");
            await e.InvokeNext();
        }
    }

}