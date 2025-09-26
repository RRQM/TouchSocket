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
/// 定义了一个ITcpReceivedPlugin接口，该接口继承自IPlugin接口。
/// 用于处理TCP接收数据的插件，提供了一种扩展机制，允许开发人员实现自定义的数据处理逻辑。
/// </summary>
[DynamicMethod]
public interface ITcpReceivedPlugin : IPlugin
{
    /// <summary>
    /// 在收到数据时触发
    /// </summary>
    /// <param name="client">触发事件的TCP会话客户端</param>
    /// <param name="e">包含接收到的数据和相关状态的事件参数</param>
    /// <returns>一个Task对象，表示异步操作的结果</returns>
    Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e);
}