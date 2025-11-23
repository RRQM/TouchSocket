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
using TouchSocket.Sockets;

namespace TcpCommandLineConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        #region TcpCommandLine创建服务器
        var service = new TcpService();

        var config = new TouchSocketConfig();
        config.SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) }) //同时监听两个地址
              .SetTcpDataHandlingAdapter(() =>
              {
                  return new TerminatorPackageAdapter("\r\n");//命令行中使用\r\n结尾
              })
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  a.UseTcpSessionCheckClear(options =>
                  {
                      options.CheckClearType = CheckClearType.All;
                      options.Tick = TimeSpan.FromSeconds(60);
                  });

                  a.Add<MyCommandLinePlugin>();
              });

        //载入配置
        await service.SetupAsync(config);

        //启动
        await service.StartAsync();

        service.Logger.Info("服务器成功启动。");
        service.Logger.Info("使用：\"Add 10 20\"测试");
        service.Logger.Info("使用：\"MUL 10 20\"测试");
        service.Logger.Info("使用：\"Exc\"测试异常");
        #endregion
        Console.ReadKey();
    }
}

#region TcpCommandLine创建命令行插件
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
    public int MULCommand(ITcpSessionClient socketClient, int a, int b)
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
#endregion