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
/// 具有完成连接动作的插件接口
/// </summary>
[DynamicMethod]
public interface INamedPipeConnectedPlugin : IPlugin
{
    /// <summary>
    /// 客户端连接成功后触发
    /// </summary>
    /// <param name="client">表示当前会话的接口，可用于发送和接收数据</param>
    /// <param name="e">包含连接事件的信息</param>
    /// <returns>返回一个任务对象，表示异步操作的结果</returns>
    Task OnNamedPipeConnected(INamedPipeSession client, ConnectedEventArgs e);
}