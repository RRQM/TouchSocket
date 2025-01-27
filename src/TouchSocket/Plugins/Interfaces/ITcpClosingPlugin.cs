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

using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了即将断开TCP连接时的插件接口。
/// 该接口仅在主动断开连接时有效。
/// </summary>
[DynamicMethod]
public interface ITcpClosingPlugin : IPlugin
{
    /// <summary>
    /// 处理TCP连接即将断开的情况（仅在主动断开连接时有效）。
    /// 此方法主要用于执行断开连接前的清理工作。
    /// </summary>
    /// <param name="client">发起断开连接请求的客户端会话对象。</param>
    /// <param name="e">断开连接事件的相关信息。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    Task OnTcpClosing(ITcpSession client, ClosingEventArgs e);
}