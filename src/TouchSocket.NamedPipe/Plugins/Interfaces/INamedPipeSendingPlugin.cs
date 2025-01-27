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
/// 定义了一个接口，用于在命名管道中发送数据的插件。
/// 继承自IPlugin接口。
/// </summary>
[DynamicMethod]
public interface INamedPipeSendingPlugin : IPlugin
{
    /// <summary>
    /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
    /// 该方法允许插件在数据发送前执行自定义逻辑。
    /// </summary>
    /// <param name="client">表示命名管道会话的接口对象。</param>
    /// <param name="e">包含发送数据事件相关信息的事件参数对象。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    Task OnNamedPipeSending(INamedPipeSession client, SendingEventArgs e);
}