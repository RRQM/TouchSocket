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
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 定义即将断开连接时的插件行为（仅主动断开时有效）。
/// </summary>
[DynamicMethod]
public interface IDmtpClosingPlugin : IPlugin
{
    /// <summary>
    /// 在即将断开连接时执行（仅主动断开时有效）。
    /// </summary>
    /// <param name="client">正在断开连接的客户端对象。</param>
    /// <param name="e">断开连接事件的相关参数。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    Task OnDmtpClosing(IDmtpActorObject client, ClosingEventArgs e);
}