using TouchSocket.Core;
using TouchSocketExamplesBeta.Core;

namespace TouchSocketExamplesBeta
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleAction consoleAction = new ConsoleAction();
            consoleAction.OnException += ConsoleAction_OnException;
            consoleAction.Add("1","插件系统",PluginManagerDemo.Start);

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