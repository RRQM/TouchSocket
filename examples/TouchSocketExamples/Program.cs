using TouchSocket.Core;
using TouchSocketExamples.Sockets;
using TouchSocketExamplesBeta.Core;

namespace TouchSocketExamplesBeta
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var consoleAction = new ConsoleAction();
            consoleAction.OnException += ConsoleAction_OnException;
            consoleAction.Add("1",nameof(PluginManagerDemo),PluginManagerDemo.Start);
            consoleAction.Add("2",nameof(SimpleTcpDemo), SimpleTcpDemo.Start);

            consoleAction.ShowAll();
            while (true)
            {
                if (!consoleAction.Run(Console.ReadLine()))
                {
                    Console.WriteLine($"您输入的指令不正确，请输入:{consoleAction.HelpOrder}获取更多");
                }
            }
        }

        private static void ConsoleAction_OnException(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }
    }
}