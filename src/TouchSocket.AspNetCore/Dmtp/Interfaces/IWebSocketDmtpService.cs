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

using Microsoft.AspNetCore.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.AspNetCore;


/// <summary>
/// 定义一个接口，用于通过WebSocket提供Dmtp服务
/// </summary>
public interface IWebSocketDmtpService : IDmtpService, IConnectableService<WebSocketDmtpSessionClient>
{
    /// <summary>
    /// 转换客户端
    /// </summary>
    /// <param name="context">HTTP上下文，包含有关当前请求的所有信息</param>
    /// <returns>一个任务，表示异步操作的结果</returns>
    Task SwitchClientAsync(HttpContext context);
}