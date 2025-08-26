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

namespace RpcPerformanceConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var consoleAction = new ConsoleAction("h|help|?");//设置帮助命令
            consoleAction.OnException += ConsoleAction_OnException;//订阅执行异常输出

            TouchSocketRpc.StartServer();

            var count = 100000;

            consoleAction.Add("3.1", "TouchSocketRpc测试Sum", async () => await TouchSocketRpc.StartSumClient(count));
            consoleAction.Add("3.2", "TouchSocketRpc测试GetBytes", async () => await TouchSocketRpc.StartGetBytesClient(count));
            consoleAction.Add("3.3", "TouchSocketRpc测试BigString", async () => await TouchSocketRpc.StartBigStringClient(count));

            consoleAction.ShowAll();

            await consoleAction.RunCommandLineAsync();

        }

        private static void ConsoleAction_OnException(Exception ex)
        {
            ConsoleLogger.Default.Exception(ex);
        }
    }
}