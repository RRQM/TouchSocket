using TouchSocket.Core;

namespace XUnitTestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch
            {
            }
            XUnitTestDemo.Start();
            Console.ReadKey();
        }
    }
}