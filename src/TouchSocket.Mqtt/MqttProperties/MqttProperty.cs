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
/// 表示Mqtt用户属性。
/// </summary>
public readonly record struct MqttUserProperty
{
    /// <summary>
    /// 初始化 <see cref="MqttUserProperty"/> 结构的新实例。
    /// </summary>
    /// <param name="name">用户属性的名称。</param>
    /// <param name="value">用户属性的值。</param>
    /// <exception cref="ArgumentNullException">当名称或值为 null 时引发。</exception>
    public MqttUserProperty(string name, string value)
    {
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    /// <summary>
    /// 获取用户属性的名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 获取用户属性的值。
    /// </summary>
    public string Value { get; }
}