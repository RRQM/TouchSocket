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

using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// KCP连接建立插件接口。当<see cref="KcpClient"/>或<see cref="KcpService"/>建立KCP会话时触发。
/// </summary>
[DynamicMethod]
public interface IKcpConnectedPlugin : IPlugin
{
    /// <summary>
    /// KCP会话建立时触发。
    /// </summary>
    /// <param name="client">触发事件的客户端对象（<see cref="KcpClient"/>或<see cref="KcpSessionClient"/>）。</param>
    /// <param name="e">连接事件参数。</param>
    Task OnKcpConnected(object client, ConnectedEventArgs e);
}

/// <summary>
/// KCP连接断开插件接口。当KCP会话关闭时触发。
/// </summary>
[DynamicMethod]
public interface IKcpClosedPlugin : IPlugin
{
    /// <summary>
    /// KCP会话关闭时触发。
    /// </summary>
    /// <param name="client">触发事件的客户端对象。</param>
    /// <param name="e">断开连接事件参数。</param>
    Task OnKcpClosed(object client, ClosedEventArgs e);
}

/// <summary>
/// KCP客户端收到应用层数据插件接口。
/// </summary>
[DynamicMethod]
public interface IKcpClientReceivedPlugin : IPlugin
{
    /// <summary>
    /// <see cref="KcpClient"/>收到KCP应用层数据时触发。
    /// </summary>
    /// <param name="client">触发事件的客户端对象。</param>
    /// <param name="e">包含接收数据的事件参数。</param>
    Task OnKcpReceived(KcpClient client, KcpReceivedEventArgs e);
}

/// <summary>
/// KCP服务端会话收到应用层数据插件接口。
/// </summary>
[DynamicMethod]
public interface IKcpSessionReceivedPlugin : IPlugin
{
    /// <summary>
    /// <see cref="KcpSessionClient"/>收到KCP应用层数据时触发。
    /// </summary>
    /// <param name="client">触发事件的会话客户端对象。</param>
    /// <param name="e">包含接收数据的事件参数。</param>
    Task OnKcpReceived(KcpSessionClient client, KcpReceivedEventArgs e);
}
