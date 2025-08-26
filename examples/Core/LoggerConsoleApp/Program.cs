// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Core;

namespace LoggerConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        TestFileLogger();
    }

    private static void TestConsoleLogger()
    {
        var logger = ConsoleLogger.Default;

        logger.LogLevel = LogLevel.Debug;

        logger.Info("Message");
        logger.Warning("Warning");
        logger.Error("Error");
    }

    private static void TestFileLogger()
    {
        var logger = new FileLogger()
        {
            CreateLogFolder = (logLevel) =>
            {
                return $"logs\\{DateTime.Now:[yyyy-MM-dd]}\\{logLevel}";
            },
            FileNameFormat = "",


        };
        logger.Info("Message");
        logger.Warning("Warning");
        logger.Error("Error");
    }

    private static void TestEasyLogger()
    {
        var logger = new EasyLogger(LoggerOutput);
        logger.Info("Message");
        logger.Warning("Warning");
        logger.Error("Error");
    }

    private static void LoggerOutput(string loggerString)
    {
        Console.WriteLine(loggerString);

        //或者如果是winform程序，可以直接输出到TextBox
    }
}

internal class MyLogger : ILog
{
    public LogLevel LogLevel { get; set; } = LogLevel.Debug;
    public string DateTimeFormat { get; set; }

    public void Log(LogLevel logLevel, object source, string message, Exception exception)
    {
        //此处可以自由实现逻辑。
    }
}
