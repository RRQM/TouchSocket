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

using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 定义了一个插件接口，用于处理会话关闭后的操作
/// </summary>
[DynamicMethod]
public interface IDmtpClosedPlugin : IPlugin
{
    /// <summary>
    /// 会话断开后触发
    /// </summary>
    /// <param name="client">触发事件的客户端对象</param>
    /// <param name="e">断开事件的参数</param>
    /// <returns>一个异步任务，表示操作的完成</returns>
    Task OnDmtpClosed(IDmtpActorObject client, ClosedEventArgs e);
}