using UnityServerConsoleApp_2D.TouchServer;

namespace UnityServerConsoleApp_2D
{
    internal class Program
    {
        //适用于unity的package包在同级目录中
        static async Task Main(string[] args)
        {
            Touch_JsonWebSocket_2D touch_2d = new Touch_JsonWebSocket_2D();

            await touch_2d.StartService(7794);

            Console.ReadKey();

        }
    }
}
