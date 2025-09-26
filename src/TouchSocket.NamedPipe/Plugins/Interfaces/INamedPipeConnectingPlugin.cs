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

namespace TouchSocket.NamedPipe;

/// <summary>
/// 具有预备连接的插件接口
/// </summary>
[DynamicMethod]
public interface INamedPipeConnectingPlugin : IPlugin
{
    /// <summary>
    /// 在即将完成连接时触发。
    /// 该方法用于处理命名管道在连接建立前的事件。
    /// </summary>
    /// <param name="client">表示命名管道会话的接口。</param>
    /// <param name="e">包含连接信息的事件参数。</param>
    /// <returns>一个表示异步操作的任务。</returns>
    Task OnNamedPipeConnecting(INamedPipeSession client, ConnectingEventArgs e);
}