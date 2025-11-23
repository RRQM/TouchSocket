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

using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个包含用户属性的 Mqtt 消息。
/// </summary>
public abstract class MqttUserPropertiesMessage : MqttMessage
{
    private List<MqttUserProperty> m_userProperties;

    /// <summary>
    /// 获取用户属性的只读列表。
    /// </summary>
    [NotNull]
    public IReadOnlyList<MqttUserProperty> UserProperties => this.m_userProperties ?? MqttUtility.EmptyUserProperties;

    /// <summary>
    /// 添加一个用户属性。
    /// </summary>
    /// <param name="name">属性名称。</param>
    /// <param name="value">属性值。</param>
    public void AddUserProperty(string name, string value)
    {
        this.AddUserProperty(new MqttUserProperty(name, value));
    }

    /// <summary>
    /// 添加一个用户属性。
    /// </summary>
    /// <param name="userProperty">用户属性。</param>
    public void AddUserProperty(MqttUserProperty userProperty)
    {
        this.m_userProperties ??= new List<MqttUserProperty>();
        this.m_userProperties.Add(userProperty);
    }
}