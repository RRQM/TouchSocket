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

namespace TouchSocket.SerialPorts;

/// <summary>
/// 定义一个串行连接建立前的操作接口
/// </summary>
[DynamicMethod]
public interface ISerialConnectingPlugin : IPlugin
{
    /// <summary>
    /// 在串行连接建立之前触发的事件处理程序
    /// 此方法允许在实际连接建立之前执行一些操作，比如数据验证或预处理
    /// </summary>
    /// <param name="client">串行会话客户端，表示与之通信的物理或虚拟串行端口</param>
    /// <param name="e">连接事件参数，包含有关连接事件的额外信息</param>
    /// <returns>异步任务，表示事件处理的异步操作</returns>
    Task OnSerialConnecting(ISerialPortSession client, ConnectingEventArgs e);
}