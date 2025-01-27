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

namespace TouchSocket.SerialPorts;


/// <summary>
/// 定义串行数据接收插件的接口。
/// 继承自IPlugin接口，特定于串行端口数据接收操作。
/// </summary>
[DynamicMethod]
public interface ISerialReceivedPlugin : IPlugin
{
    /// <summary>
    /// 当串行端口接收到数据时触发的异步事件处理方法。
    /// </summary>
    /// <param name="client">触发数据接收事件的串行端口会话对象。</param>
    /// <param name="e">包含接收数据事件相关信息的事件参数对象。</param>
    /// <returns>一个Task对象，标识异步操作的完成。</returns>
    Task OnSerialReceived(ISerialPortSession client, ReceivedDataEventArgs e);
}