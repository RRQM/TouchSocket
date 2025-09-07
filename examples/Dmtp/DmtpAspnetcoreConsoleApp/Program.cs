//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpAspnetcoreConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //WebSocketDmtpClient连接
        var websocketDmtpClient = new WebSocketDmtpClient();
        await websocketDmtpClient.SetupAsync(new TouchSocketConfig()
             .SetDmtpOption(options=>
             {
                 options.VerifyToken = "Dmtp";
             })
             .SetRemoteIPHost("ws://localhost:5174/WebSocketDmtp"));
        await websocketDmtpClient.ConnectAsync();
        Console.WriteLine("WebSocketDmtpClient连接成功");

        //HttpDmtpClient连接
        var httpDmtpClient = new HttpDmtpClient();
        await httpDmtpClient.SetupAsync(new TouchSocketConfig()
             .SetDmtpOption(options=>
             {
                 options.VerifyToken = "Dmtp";
             })
             .SetRemoteIPHost("http://127.0.0.1:5174"));
        await httpDmtpClient.ConnectAsync();
        Console.WriteLine("HttpDmtpClient连接成功");

        Console.ReadKey();
    }
}