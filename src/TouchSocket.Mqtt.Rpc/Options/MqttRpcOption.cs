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

using System.Text.Json;

namespace TouchSocket.Mqtt.Rpc;

/// <summary>
/// MqttRpc 配置选项。
/// </summary>
public class MqttRpcOption
{
    /// <summary>
    /// 获取或设置 RPC 请求主题。默认为 <c>mqttrpc/req</c>。
    /// </summary>
    public string RequestTopic { get; set; } = "mqttrpc/req";

    /// <summary>
    /// 获取或设置 RPC 响应主题前缀。默认为 <c>mqttrpc/res</c>。
    /// 实际响应主题为 <c>{ResponseTopicPrefix}/{唯一标识}</c>。
    /// </summary>
    public string ResponseTopicPrefix { get; set; } = "mqttrpc/res";

    /// <summary>
    /// 获取或设置 RPC 请求/响应的 QoS 级别。默认为 <see cref="QosLevel.AtLeastOnce"/>。
    /// </summary>
    public QosLevel QosLevel { get; set; } = QosLevel.AtLeastOnce;

    /// <summary>
    /// 获取序列化选项。
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions();
}
