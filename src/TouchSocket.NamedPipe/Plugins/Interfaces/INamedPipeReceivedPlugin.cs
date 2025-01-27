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

namespace TouchSocket.NamedPipe;


/// <summary>
/// 定义了一个接口，用于处理命名管道接收到的数据
/// </summary>
[DynamicMethod]
public interface INamedPipeReceivedPlugin : IPlugin
{
    /// <summary>
    /// 在收到数据时触发
    /// </summary>
    /// <param name="client">发送数据的命名管道会话</param>
    /// <param name="e">包含接收到的数据的事件参数</param>
    /// <returns>一个任务，表示异步操作</returns>
    Task OnNamedPipeReceived(INamedPipeSession client, ReceivedDataEventArgs e);
}