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

namespace TouchSocket.SocketIo;

/// <summary>
/// 定义在 Socket.IO 客户端完成连接之前触发的插件接口。
/// </summary>
[DynamicMethod]
public interface ISocketIoConnectingPlugin : IPlugin
{
    /// <summary>
    /// 在 Socket.IO 客户端即将建立连接时调用，可用于验证或拦截连接。
    /// </summary>
    /// <param name="client">客户端对象。</param>
    /// <param name="e">Socket.IO 连接验证事件参数。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task OnSocketIoConnecting(ISocketIoSession client, SocketIoVerifyEventArgs e);
}