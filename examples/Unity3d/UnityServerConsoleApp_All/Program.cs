using UnityServerConsoleApp_All.TouchServer;

namespace UnityServerConsoleApp_All
{
    internal class Program
    {
        //适用于unity的package包在同级目录中
        static async Task Main(string[] args)
        {
            Touch_UDP touch_UDP = new Touch_UDP();
            Touch_TCP touch_TCP = new Touch_TCP();
            Touch_HttpDmtp touch_Dmtp = new Touch_HttpDmtp();
            Touch_WebSocket touch_WebSocket = new Touch_WebSocket();
            Touch_JsonWebSocket touch_JsonWeb = new Touch_JsonWebSocket();

            await touch_TCP.StartService(7789);
            await touch_Dmtp.StartService(7790);
            await touch_UDP.StartService(7791);
            await touch_WebSocket.StartService(7792);
            await touch_JsonWeb.StartService(7793);

            Console.ReadKey();

        }
    }
}
