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
/// 定义了一个接口，用于通过TCP进行接收操作的插件。
/// 继承自IPlugin接口，表示这是一个插件的一部分，专注于接收TCP数据。
/// </summary>
[DynamicMethod]
public interface ITcpReceivingPlugin : IPlugin
{
    /// <summary>
    /// 在刚收到数据时触发，即在适配器之前。
    /// 该方法主要用于执行接收数据前的预处理操作。
    /// </summary>
    /// <param name="client">发送数据的客户端会话对象。</param>
    /// <param name="e">包含接收数据事件相关的参数。</param>
    /// <returns>一个Task对象，代表异步操作的结果。</returns>
    Task OnTcpReceiving(ITcpSession client, BytesReaderEventArgs e);
}