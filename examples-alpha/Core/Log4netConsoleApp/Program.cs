using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Log4netConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = CreateService();
            Console.ReadKey();
        }

        private static TcpService CreateService()
        {
            var service = new TcpService();
            service.Received = async (client, e) =>
            {
                //从客户端收到信息
                var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
                service.Logger.Info($"服务器已从{client.Id}接收到信息：{mes}");

                await client.SendAsync(mes);//将收到的信息直接返回给发送方
            };

            service.SetupAsync(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                })
                .ConfigureContainer(a =>
                {
                    a.AddLogger(new Mylog4netLogger());//添加Mylog4netLogger日志
                }));
            service.StartAsync();//启动
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }

    internal class Mylog4netLogger : TouchSocket.Core.ILog
    {
        private readonly log4net.ILog m_logger;

        public Mylog4netLogger()
        {
            this.m_logger = log4net.LogManager.GetLogger("Test");
        }

        public LogLevel LogLevel { get; set; }

        public void Log(LogLevel logLevel, object source, string message, Exception exception)
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
}