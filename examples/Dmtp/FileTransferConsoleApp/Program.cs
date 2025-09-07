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
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Sockets;

namespace FileTransferConsoleApp;

internal class Program
{
    /// <summary>
    /// 测试文件大小
    /// </summary>
    public const long FileLength = 1024 * 1024 * 10L;

    /// <summary>
    /// 传输限速
    /// </summary>
    public const long MaxSpeed = 1024 * 1024L;

    private static async Task Main(string[] args)
    {
        var service = await GetTcpDmtpService();
        var client = await GetTcpDmtpClient();

        var consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;
        consoleAction.Add("1", "测试客户端向服务器请求文件", async () => await ClientPullFileFromService(client));
        consoleAction.Add("2", "测试客户端向服务器推送文件", async () => await ClientPushFileFromService(client));

        //此处，因为Dmtp组件，在客户端连接上服务器之后，客户端会与服务器的SessionClient同步Id。
        //详情：http://rrqm_home.gitee.io/touchsocket/docs/dmtpbaseconnection
        //所以此处直接使用客户端Id。
        consoleAction.Add("3", "测试服务器向客户端请求文件", async () => await ServicePullFileFromClient(service, client.Id));
        consoleAction.Add("4", "测试服务器向客户端推送文件", async () => await ServicePushFileFromClient(service, client.Id));

        consoleAction.Add("5", "测试客户端向其他客户端请求文件", async () => await ClientPullFileFromClient());
        consoleAction.Add("6", "测试客户端向其他客户端推送文件", async () => await ClientPushFileFromClient());

        consoleAction.Add("7", "测试客户端向服务器请求小文件", async () => await PullSmallFileFromService(client));
        consoleAction.Add("8", "测试客户端向服务器推送小文件", async () => await PushSmallFileFromService(client));

        consoleAction.Add("9", "测试客户端工厂向服务器请求文件", async () => await MultithreadingClientPullFileFromService());
        consoleAction.Add("10", "测试客户端工厂向服务器推送文件", async () => await MultithreadingClientPushFileFromService());

        consoleAction.ShowAll();

        await consoleAction.RunCommandLineAsync();
    }

    /// <summary>
    /// 多线程推送文件
    /// </summary>
    private static async Task MultithreadingClientPushFileFromService()
    {
        using var clientFactory = CreateClientFactory();

        ConsoleLogger.Default.Info("开始向服务器推送文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "MultithreadingClientPushFileFromService.Test";
        var saveFilePath = "SaveMultithreadingClientPushFileFromService.Test";

        if (!File.Exists(filePath))//创建服务器端的测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new MultithreadingFileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//客户端本地保存路径
            ResourcePath = filePath,//请求文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512,//分包大小，当网络较差时，应该适当减小该值
            MultithreadingCount = 10//多线程数量
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            ConsoleLogger.Default.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会等待，直到传输结束
        var result = await clientFactory.PushFileAsync(fileOperator);

        ConsoleLogger.Default.Info($"向服务器推送文件结束，{result}");

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 多线程请求文件
    /// </summary>
    private static async Task MultithreadingClientPullFileFromService()
    {
        using var clientFactory = CreateClientFactory();

        ConsoleLogger.Default.Info("开始从服务器下载文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "MultithreadingClientPullFileFromService.Test";
        var saveFilePath = "SaveMultithreadingClientPullFileFromService.Test";
        if (!File.Exists(filePath))//创建服务器端的测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new MultithreadingFileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//客户端本地保存路径
            ResourcePath = filePath,//请求文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512,//分包大小，当网络较差时，应该适当减小该值
            MultithreadingCount = 10//多线程数量
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            ConsoleLogger.Default.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会等待，直到传输结束
        var result = await clientFactory.PullFileAsync(fileOperator);

        ConsoleLogger.Default.Info($"从服务器下载文件结束，{result}");

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 推送小文件
    /// </summary>
    /// <param name="client"></param>
    private static async Task PushSmallFileFromService(TcpDmtpClient client)
    {
        ConsoleLogger.Default.Info("开始从服务器下载小文件");
        var filePath = "PushSmallFileFromService.Test";
        var saveFilePath = "SavePushSmallFileFromService.Test";
        if (!File.Exists(filePath))//创建测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(1024);
            }
        }

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        //此方法会阻塞，直到传输结束，也可以使用PullSmallFileAsync
        var result = await client.GetDmtpFileTransferActor().PushSmallFileAsync(saveFilePath, new FileInfo(filePath), metadata, CancellationToken.None);
        if (result.IsSuccess)
        {
            //成功
        }

        ConsoleLogger.Default.Info($"从服务器下载小文件结束，结果：{result}");
        client.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 请求小文件
    /// </summary>
    /// <param name="client"></param>
    private static async Task PullSmallFileFromService(TcpDmtpClient client)
    {
        ConsoleLogger.Default.Info("开始从服务器下载小文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "PullSmallFileFromService.Test";
        var saveFilePath = "SavePullSmallFileFromService.Test";
        if (!File.Exists(filePath))//创建测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(1024);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        //此方法会阻塞，直到传输结束，也可以使用PullSmallFileAsync
        var result = await client.GetDmtpFileTransferActor().PullSmallFileAsync(filePath, metadata, CancellationToken.None);
        var data = result.Value;//此处即是下载的小文件的实际数据
        result.Save(saveFilePath, overwrite: true);//将数据保存到指定路径。

        ConsoleLogger.Default.Info("从服务器下载小文件结束");
        client.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    private static void ConsoleAction_OnException(Exception obj)
    {
        Console.WriteLine(obj.Message);
    }

    /// <summary>
    /// 客户端向其他客户端推送文件。
    /// </summary>
    private static async Task ClientPushFileFromClient()
    {
        using var client1 = await GetTcpDmtpClient();
        using var client2 = await GetTcpDmtpClient();

        ConsoleLogger.Default.Info("开始从其他客户端下载文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "ClientPushFileFromClient.Test";
        var saveFilePath = "SaveClientPushFileFromClient.Test";
        if (!File.Exists(filePath))//创建测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//客户端本地保存路径
            ResourcePath = filePath,//请求文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client1.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
        var result = await client1.GetDmtpFileTransferActor().PushFileAsync(client2.Id, fileOperator);

        ConsoleLogger.Default.Info("从其他客户端下载文件结束");
        client1.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 客户端从其他客户端下载文件。
    /// </summary>
    private static async Task ClientPullFileFromClient()
    {
        using var client1 = await GetTcpDmtpClient();
        using var client2 = await GetTcpDmtpClient();

        ConsoleLogger.Default.Info("开始从其他客户端下载文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "ClientPullFileFromClient.Test";
        var saveFilePath = "SaveClientPullFileFromClient.Test";
        if (!File.Exists(filePath))//创建测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//客户端本地保存路径
            ResourcePath = filePath,//请求文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client1.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
        var result = await client1.GetDmtpFileTransferActor().PullFileAsync(client2.Id, fileOperator);

        ConsoleLogger.Default.Info("从其他客户端下载文件结束");
        client1.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 服务器往客户端主动推送文件
    /// </summary>
    /// <param name="service">服务器</param>
    /// <param name="targetId">服务器要请求的客户端Id</param>
    private static async Task ServicePushFileFromClient(TcpDmtpService service, string targetId)
    {
        if (!service.TryGetClient(targetId, out var socketClient))
        {
            throw new Exception($"没有找到Id={targetId}的客户端");
        }

        ConsoleLogger.Default.Info("开始从客户端下载文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "ServicePushFileFromClient.Test";
        var saveFilePath = "SaveServicePushFileFromClient.Test";
        if (!File.Exists(filePath))//创建服务器的测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到客户端的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//客户端本地保存路径
            ResourcePath = filePath,//服务器文件的资源路径
            Metadata = metadata,//传递到客户端的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            socketClient.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PushFileAsync
        var result = await socketClient.GetDmtpFileTransferActor().PushFileAsync(fileOperator);

        ConsoleLogger.Default.Info("服务器主动推送客户端文件结束");
        socketClient.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 服务器从客户端下载文件
    /// </summary>
    /// <param name="service">服务器</param>
    /// <param name="targetId">服务器要请求的客户端Id</param>
    private static async Task ServicePullFileFromClient(TcpDmtpService service, string targetId)
    {
        if (!service.TryGetClient(targetId, out var socketClient))
        {
            throw new Exception($"没有找到Id={targetId}的客户端");
        }

        ConsoleLogger.Default.Info("开始从客户端下载文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "ServicePullFileFromClient.Test";
        var saveFilePath = "SaveServicePullFileFromClient.Test";
        if (!File.Exists(filePath))//创建客户端的测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到客户端的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new FileOperator()//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//服务器本地保存路径
            ResourcePath = filePath,//请求客户端文件的资源路径
            Metadata = metadata,//传递到客户端的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            socketClient.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
        var result = await socketClient.GetDmtpFileTransferActor().PullFileAsync(fileOperator);

        ConsoleLogger.Default.Info("从客户端下载文件结束");
        socketClient.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 客户端从服务器下载文件。
    /// </summary>
    /// <param name="client"></param>
    private static async Task ClientPullFileFromService(TcpDmtpClient client)
    {
        ConsoleLogger.Default.Info("开始从服务器下载文件");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "ClientPullFileFromService.Test";
        var saveFilePath = "SaveClientPullFileFromService.Test";
        if (!File.Exists(filePath))//创建服务器端的测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//客户端本地保存路径
            ResourcePath = filePath,//请求文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
        var result = await client.GetDmtpFileTransferActor().PullFileAsync(fileOperator);

        ////关于断点续传
        ////在执行完PullFile(fileOperator)或PushFile(fileOperator)时。只要返回的结果不是Success。
        ////那么就意味着传输没有完成。
        ////而续传的机制就是，在执行传输之前，如果fileOperator.ResourceInfo为空，则开始新的传输。如果不为空，则尝试续传。
        ////而对于失败的传输，未完成的信息都在fileOperator.ResourceInfo中。
        ////所以我们可以使用一个变量（或字典）来存fileOperator.ResourceInfo的值。
        ////亦或者可以把ResourceInfo的值持久化。
        ////然后在重新发起请求传输值前，先对fileOperator.ResourceInfo做有效赋值。即可尝试断点传输。

        //byte[] cacheBytes;//这就是持久化后的数据。你可以将此数据写入到文件或数据库。
        //using (var byteBlock=new ByteBlock(1024*64))
        //{
        //    fileOperator.ResourceInfo.Save(byteBlock);

        //    cacheBytes = byteBlock.ToArray();
        //}

        ////然后想要续传的时候。先把缓存数据转为FileResourceInfo。
        //using (var byteBlock=new ByteBlock(cacheBytes))
        //{
        //    var resourceInfo = new FileResourceInfo(byteBlock);
        //    //然后把resourceInfo赋值给新建的FileOperator的ResourceInfo属性。
        //}

        ConsoleLogger.Default.Info("从服务器下载文件结束");
        client.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    /// <summary>
    /// 客户端上传文件到服务器。
    /// </summary>
    /// <param name="client"></param>
    private static async Task ClientPushFileFromService(TcpDmtpClient client)
    {
        ConsoleLogger.Default.Info("上传文件到服务器");

        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/
        var filePath = "ClientPushFileFromService.Test";
        var saveFilePath = "SaveClientPushFileFromService.Test";
        if (!File.Exists(filePath))//创建客户端的测试文件
        {
            using (var stream = File.OpenWrite(filePath))
            {
                stream.SetLength(FileLength);
            }
        }
        /****此处的逻辑是在程序运行目录下创建一个空内容，但是有长度的文件，用于测试****/

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add("1", "1");
        metadata.Add("2", "2");

        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = saveFilePath,//服务器本地保存路径
            ResourcePath = filePath,//客户端本地即将上传文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client.Logger.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed()}");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PushFileAsync
        var result = await client.GetDmtpFileTransferActor().PushFileAsync(fileOperator);

        ConsoleLogger.Default.Info("上传文件到服务器结束");
        client.Logger.Info(result.ToString());

        //删除测试文件。此逻辑在实际使用时不要有
        File.Delete(filePath);
        File.Delete(saveFilePath);
    }

    private static async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var client = await new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7789")
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "File";
               })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpFileTransfer();//必须添加文件传输插件

                   a.Add<MyPlugin>();

                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromSeconds(3))
                   .SetMaxFailCount(3);
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
                   a.UseDmtpFileTransfer()//必须添加文件传输插件
                   //.SetRootPath("C:\\新建文件夹")//设置RootPath
                   .SetMaxSmallFileLength(1024 * 1024);//设置小文件的最大限制长度
                   a.Add<MyPlugin>();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "File";//连接验证口令。
               });

        await service.SetupAsync(config);
        await service.StartAsync();
        service.Logger.Info("服务器成功启动");
        return service;
    }

    private static TcpDmtpClientFactory CreateClientFactory()
    {
        try
        {
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            ConsoleLogger.Default.Exception(ex);
        }
        var clientFactory = new TcpDmtpClientFactory()
        {
            MinCount = 5,
            MaxCount = 10,
            GetConfig = () => //配置辅助通信
            {
                return new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetDmtpOption(options =>
                {
                    options.VerifyToken = "File";
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpFileTransfer();
                });
            }
        };

        clientFactory.SetFindTransferIds((targetId) =>
        {
            //此处的操作不唯一，可能需要rpc实现。
            //其目的比较简单，就是获取到targetId对应的主客户端的所有传输客户端的Id集合。
            //这样就实现了多个客户端向多个客户端传输文件的目的。

            return new string[] { targetId };//此处为模拟结果。
        });
        return clientFactory;
    }
}

internal class MyPlugin : PluginBase, IDmtpFileTransferringPlugin, IDmtpFileTransferredPlugin, IDmtpRoutingPlugin
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
    public async Task OnDmtpFileTransferred(IDmtpActorObject client, FileTransferredEventArgs e)
    {
        //传输结束，但是不一定成功，甚至该方法都不一定会被触发，具体信息需要从e.Result判断状态。
        if (e.TransferType.IsPull())
        {
            this.m_logger.Info($"结束Pull文件，类型={e.TransferType}，文件名={e.ResourcePath}，结果={e.Result}");
        }
        else
        {
            this.m_logger.Info($"结束Push文件，类型={e.TransferType}，文件名={e.FileInfo.Name}，结果={e.Result}");
        }
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
    public async Task OnDmtpFileTransferring(IDmtpActorObject client, FileTransferringEventArgs e)
    {
        foreach (var item in e.Metadata.Keys)
        {
            Console.WriteLine($"Key={item},Value={e.Metadata[item]}");
        }
        e.IsPermitOperation = true;//每次传输都需要设置true，表示允许传输
        //有可能是上传，也有可能是下载

        if (e.TransferType.IsPull())
        {
            this.m_logger.Info($"请求Pull文件，类型={e.TransferType}，文件名={e.ResourcePath}");
        }
        else
        {
            this.m_logger.Info($"请求Push文件，类型={e.TransferType}，文件名={e.FileInfo.Name}");
        }
        await e.InvokeNext();
    }

    public async Task OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
    {
        e.IsPermitOperation = true;//允许路由
        this.m_logger.Info($"路由类型：{e.RouterType}");

        await e.InvokeNext();
    }
}