using TouchSocket.Core;

namespace RpcPerformanceConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var consoleAction = new ConsoleAction("h|help|?");//设置帮助命令
            consoleAction.OnException += ConsoleAction_OnException;//订阅执行异常输出

            TouchSocketRpc.StartServer();

            var count = 100000;

            consoleAction.Add("3.1", "TouchSocketRpc测试Sum", () => TouchSocketRpc.StartSumClient(count));
            consoleAction.Add("3.2", "TouchSocketRpc测试GetBytes", () => TouchSocketRpc.StartGetBytesClient(count));
            consoleAction.Add("3.3", "TouchSocketRpc测试BigString", () => TouchSocketRpc.StartBigStringClient(count));

            consoleAction.ShowAll();

            await consoleAction.RunCommandLineAsync();

        }

        private static void ConsoleAction_OnException(Exception ex)
        {
            ConsoleLogger.Default.Exception(ex);
        }
    }
}