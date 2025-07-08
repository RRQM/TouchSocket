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

using System;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

/// <summary>
/// 提供用于配置Mqtt选项的扩展方法。
/// </summary>
public static class MqttConfigExtension
{
    /// <summary>
    /// Mqtt连接选项的依赖属性。
    /// </summary>
    public static readonly DependencyProperty<MqttConnectOptions> MqttConnectOptionsProperty = new DependencyProperty<MqttConnectOptions>("MqttConnectOptions", null);

    /// <summary>
    /// 设置Mqtt连接选项。
    /// </summary>
    /// <param name="config">TouchSocket配置。</param>
    /// <param name="options">用于配置Mqtt连接选项的操作。</param>
    /// <returns>更新后的TouchSocket配置。</returns>
    public static TouchSocketConfig SetMqttConnectOptions(this TouchSocketConfig config, Action<MqttConnectOptions> options)
    {
        var mqttConnectOptions = new MqttConnectOptions();
        options?.Invoke(mqttConnectOptions);
        config.SetValue(MqttConnectOptionsProperty, mqttConnectOptions);
        return config;
    }
}