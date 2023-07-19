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
            TcpService service = new TcpService();
            service.Connecting = (client, e) => { };//有客户端正在连接
            service.Connected = (client, e) => { };//有客户端连接
            service.Disconnected = (client, e) => { };//有客户端断开连接
            service.Received = (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                service.Logger.Info($"服务器已从{client.ID}接收到信息：{mes}");

                client.Send(mes);//将收到的信息直接返回给发送方
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                
                .SetThreadCount(10)
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                })
                .ConfigureContainer(a =>
                {
                    a.AddLogger(new Mylog4netLogger());//添加Mylog4netLogger日志
                }))
                .Start();//启动
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }

    internal class Mylog4netLogger : ILog
    {
        public LogType LogType { get; set; } = LogType.Trace | LogType.Debug | LogType.Info;//需要什么类型，就叠加

        void ILog.Log(LogType logType, object source, string message, Exception exception)
        {
            //此处就是实际的日志输出

            if (this.LogType.HasFlag(logType))
            {
                log4net.ILog log = log4net.LogManager.GetLogger("Test");

                switch (logType)
                {
                    case LogType.Trace:
                        log.Debug(message, exception);
                        break;

                    case LogType.Debug:
                        log.Debug(message, exception);
                        break;

                    case LogType.Info:
                        log.Info(message, exception);
                        break;

                    case LogType.Warning:
                        log.Warn(message, exception);
                        break;

                    case LogType.Error:
                        log.Error(message, exception);
                        break;

                    case LogType.Critical:
                        log.Error(message, exception);
                        break;

                    case LogType.None:
                    default:
                        break;
                }
            }
        }
    }
}