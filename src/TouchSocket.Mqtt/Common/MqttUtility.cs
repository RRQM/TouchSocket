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

namespace TouchSocket.Mqtt;

/// <summary>
/// Mqtt 工具类。
/// </summary>
public static class MqttUtility
{
    /// <summary>
    /// Mqtt 协议名称常量。
    /// </summary>
    public const string MqttProtocolName = "MQTT";
    /// <summary>
    /// Mqtt 协议名称富脱utf-8字节空间。
    /// </summary>
    public static ReadOnlySpan<byte> MqttProtocolNameSpan => Encoding.UTF8.GetBytes("MQTT");

    /// <summary>
    /// 空的 Mqtt 用户属性列表。
    /// </summary>
    public static IReadOnlyList<MqttUserProperty> EmptyUserProperties { get; } = [];
}