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
/// 具有断开连接的插件接口
/// </summary>
[DynamicMethod]
public interface INamedPipeClosedPlugin : IPlugin
{
    /// <summary>
    /// 会话断开后触发
    /// </summary>
    /// <param name="client">断开连接的命名管道客户端会话</param>
    /// <param name="e">包含断开连接事件的数据</param>
    /// <returns>一个异步任务，表示操作的结果</returns>
    Task OnNamedPipeClosed(INamedPipeSession client, ClosedEventArgs e);
}