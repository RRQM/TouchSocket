using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpCommandLineConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new TcpService();

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) }) //同时监听两个地址
                  .SetTcpDataHandlingAdapter(() =>
                  {
                      //return new TerminatorPackageAdapter(1024, "\r\n");//命令行中使用\r\n结尾
                      return new NormalDataHandlingAdapter();//亦或者省略\r\n，但此时调用方不能高速调用，会粘包
                  })
                  .ConfigureContainer(a =>
                  {
                      a.AddConsoleLogger();
                  })
                  .ConfigurePlugins(a =>
                  {
                      a.UseCheckClear()
                      .SetCheckClearType( CheckClearType.All)
                      .SetTick(TimeSpan.FromSeconds(60));

                      a.Add<MyCommandLinePlugin>();
                  });

            //载入配置
            service.Setup(config);

            //启动
            service.Start();

            service.Logger.Info("服务器成功启动。");
            service.Logger.Info("使用：“Add 10 20”测试");
            service.Logger.Info("使用：“MUL 10 20”测试");
            service.Logger.Info("使用：“Exc”测试异常");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 命令执行插件。方法必须以Command结尾。
    /// </summary>
    internal class MyCommandLinePlugin : TcpCommandLinePlugin
    {
        private readonly ILog m_logger;

        public MyCommandLinePlugin(ILog logger) : base(logger)
        {
            this.ReturnException = true;//表示执行异常的时候，是否返回异常信息
            this.m_logger = logger;
        }

        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int AddCommand(int a, int b)
        {
            this.m_logger.Info($"执行{nameof(AddCommand)}");
            return a + b;
        }

        /// <summary>
        /// 乘法，并且获取调用者信息
        /// </summary>
        /// <param name=""></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int MULCommand(ISocketClient socketClient, int a, int b)
        {
            this.m_logger.Info($"{socketClient.IP}:{socketClient.Port}执行{nameof(MULCommand)}");
            return a * b;
        }

        /// <summary>
        /// 测试异常
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void ExcCommand()
        {
            throw new Exception("我异常了");
        }
    }
}