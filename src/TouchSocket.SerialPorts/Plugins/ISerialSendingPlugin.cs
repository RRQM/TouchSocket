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
/// 串行发送插件接口，继承自通用插件接口IPlugin
/// </summary>
[DynamicMethod]
public interface ISerialSendingPlugin : IPlugin
{
    /// <summary>
    /// 在串行数据发送前触发的事件处理程序
    /// </summary>
    /// <param name="client">串行端口会话客户端</param>
    /// <param name="e">发送事件参数</param>
    /// <returns>异步任务</returns>
    Task OnSerialSending(ISerialPortSession client, SendingEventArgs e);
}