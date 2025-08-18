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

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Log4netConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();
        Console.ReadKey();
    }

    private static async Task<TcpService> CreateService()
    {
        var service = new TcpService();
        service.Received = async (client, e) =>
        {
            //从客户端收到信息
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            service.Logger.Info($"服务器已从{client.Id}接收到信息：{mes}");

            await client.SendAsync(mes);//将收到的信息直接返回给发送方
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts(7789)
             .ConfigureContainer(a =>
             {
                 a.AddLogger(logger =>
                 {
                     logger.AddConsoleLogger();
                     logger.AddFileLogger(fileLogger =>
                     {
                         fileLogger.MaxSize = 1024 * 1024;
                         fileLogger.LogLevel = LogLevel.Debug;
                     });

                     logger.AddLogger(new Mylog4netLogger());//添加Mylog4netLogger日志
                 });
             })
             .ConfigurePlugins(a =>
             {
                 //a.Add();//此处可以添加插件
             }));
        await service.StartAsync();//启动
        service.Logger.Info("服务器成功启动");
        return service;
    }
}

internal class Mylog4netLogger : LoggerBase
{
    private readonly log4net.ILog m_logger;

    public Mylog4netLogger()
    {
        this.m_logger = log4net.LogManager.GetLogger("Test");
    }

    protected override void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
    {
        //此处就是实际的日志输出

        switch (logLevel)
        {
            case LogLevel.Trace:
                this.m_logger.Debug(message, exception);
                break;

            case LogLevel.Debug:
                this.m_logger.Debug(message, exception);
                break;

            case LogLevel.Info:
                this.m_logger.Info(message, exception);
                break;

            case LogLevel.Warning:
                this.m_logger.Warn(message, exception);
                break;

            case LogLevel.Error:
                this.m_logger.Error(message, exception);
                break;

            case LogLevel.Critical:
                this.m_logger.Error(message, exception);
                break;

            case LogLevel.None:
            default:
                break;
        }
    }
}