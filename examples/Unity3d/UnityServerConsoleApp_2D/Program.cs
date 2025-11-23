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

using UnityServerConsoleApp_2D.TouchServer;

namespace UnityServerConsoleApp_2D;

internal class Program
{
    //适用于unity的package包在同级目录中
    private static async Task Main(string[] args)
    {
        var touch_2d = new Touch_JsonWebSocket_2D();

        await touch_2d.StartService(7794);

        Console.ReadKey();

    }
}
