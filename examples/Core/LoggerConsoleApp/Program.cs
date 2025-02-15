using TouchSocket.Core;

namespace LoggerConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
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
                 FileNameFormat="",
                  
                 
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

    class MyLogger : ILog
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Debug;
        public string DateTimeFormat { get; set; }

        public void Log(LogLevel logLevel, object source, string message, Exception exception)
        {
            //此处可以自由实现逻辑。
        }
    }
}
