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

namespace TouchSocket.Mqtt;

/// <summary>
/// 定义了一个插件接口，用于处理接收到的Mqtt消息。
/// </summary>
[DynamicMethod]
public interface IMqttReceivedPlugin : IPlugin
{
    /// <summary>
    /// 当接收到Mqtt消息时调用此方法。
    /// </summary>
    /// <param name="client">客户端对象。</param>
    /// <param name="e">包含Mqtt消息的事件参数。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task OnMqttReceived(IMqttSession client, MqttReceivedEventArgs e);
}
