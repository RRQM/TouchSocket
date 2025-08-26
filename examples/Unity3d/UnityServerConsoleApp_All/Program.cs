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

using UnityServerConsoleApp_All.TouchServer;

namespace UnityServerConsoleApp_All;

internal class Program
{
    //适用于unity的package包在同级目录中
    private static async Task Main(string[] args)
    {
        var touch_UDP = new Touch_UDP();
        var touch_TCP = new Touch_TCP();
        var touch_HttpDmtp = new Touch_HttpDmtp();
        var touch_TcpDmtp = new Touch_TcpDmtp();
        var touch_WebSocket = new Touch_WebSocket();
        var touch_JsonWeb = new Touch_JsonWebSocket();

        await touch_TCP.StartService(7789);
        await touch_HttpDmtp.StartService(7790);
        await touch_UDP.StartService(7791);
        await touch_WebSocket.StartService(7792);
        await touch_JsonWeb.StartService(7793);
        await touch_TcpDmtp.StartService(7794);

        Console.ReadKey();

    }
}
