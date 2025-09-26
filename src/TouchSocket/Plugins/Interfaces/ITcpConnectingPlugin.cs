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

namespace TouchSocket.Sockets;


/// <summary>
/// 定义了基于TCP连接前的插件接口，继承自IPlugin接口。
/// 该接口提供了特定于TCP连接操作的额外功能和要求。
/// </summary>
[DynamicMethod]
public interface ITcpConnectingPlugin : IPlugin
{
    /// <summary>
    /// 在即将完成连接时触发。
    /// </summary>
    /// <param name="client">正在连接的客户端会话对象。</param>
    /// <param name="e">包含连接信息的事件参数。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    Task OnTcpConnecting(ITcpSession client, ConnectingEventArgs e);
}