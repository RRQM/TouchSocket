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

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了一个UDP发送插件接口，继承自IPlugin接口。
/// 该接口为实现UDP数据发送功能的插件提供了一套标准的方法和属性。
/// </summary>
[DynamicMethod]
public interface IUdpSendingPlugin : IPlugin
{
    /// <summary>
    /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
    /// 此方法用于在数据发送前执行额外的处理或检查。
    /// </summary>
    /// <param name="client">正在与之通信的UDP会话客户端。</param>
    /// <param name="e">包含发送事件相关数据的参数对象。</param>
    /// <returns>一个Task对象，代表异步操作的结果。</returns>
    Task OnUdpSending(IUdpSessionBase client, UdpSendingEventArgs e);
}