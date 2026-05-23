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

using TouchSocket.Core;

namespace TouchSocket.CoAP;

/// <summary>
/// 定义 CoAP 消息接收时的插件接口。
/// </summary>
[DynamicMethod]
public interface ICoAPReceivedPlugin : IPlugin
{
    /// <summary>
    /// 在收到 CoAP 消息时触发。
    /// </summary>
    /// <param name="client">接收消息的 CoAP 会话。</param>
    /// <param name="e">包含接收到的 <see cref="CoAPMessage"/> 及远端端点信息的事件参数。</param>
    Task OnCoAPReceived(ICoAPSession client, CoAPMessageReceivedEventArgs e);
}
