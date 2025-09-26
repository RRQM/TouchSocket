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
/// 定义了一个命名管道接收插件接口，用于在收到数据时执行特定操作。
/// </summary>
[DynamicMethod]
public interface INamedPipeReceivingPlugin : IPlugin
{
    /// <summary>
    /// 在刚收到数据时触发，即在适配器之前。
    /// </summary>
    /// <param name="client">触发事件的命名管道会话客户端。</param>
    /// <param name="e">包含收到的数据及其他相关信息的事件参数。</param>
    /// <returns>一个任务对象，表示异步操作。</returns>
    Task OnNamedPipeReceiving(INamedPipeSession client, BytesReaderEventArgs e);
}