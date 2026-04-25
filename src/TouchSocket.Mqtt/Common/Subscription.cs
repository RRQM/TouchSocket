//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//交流QQ群：234762506
// 感谢您的下载和使用
//------------------------------------------------------------------------------

namespace TouchSocket.Mqtt;

/// <summary>
/// 表示一个订阅项，包含订阅者的客户端标识和订阅的服务质量等级。
/// </summary>
public readonly struct Subscription : IEquatable<Subscription>
{
    /// <summary>
    /// 使用指定的客户端标识和服务质量等级初始化一个新的 <see cref="Subscription"/> 实例。
    /// </summary>
    /// <param name="clientId">订阅者的客户端标识。</param>
    /// <param name="qosLevel">订阅的服务质量等级，参见 <see cref="QosLevel"/>。</param>
    public Subscription(string clientId, QosLevel qosLevel) : this(clientId)
    {
        this.QosLevel = qosLevel;
    }
    /// <summary>
    /// 使用指定的客户端标识初始化一个新的 <see cref="Subscription"/> 实例。
    /// </summary>
    /// <param name="clientId">订阅者的客户端标识。</param>
    public Subscription(string clientId)
    {
        this.ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        this.QosLevel = default;
    }
    /// <summary>
    /// 获取订阅者的客户端标识。
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// 获取订阅的服务质量等级。
    /// </summary>
    public QosLevel QosLevel { get; }

    /// <inheritdoc/>
    public static bool operator !=(Subscription left, Subscription right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public static bool operator ==(Subscription left, Subscription right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc/>
    public bool Equals(Subscription other)
    {
        return this.ClientId.Equals(other.ClientId);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is Subscription subscription && this.Equals(subscription);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.ClientId.GetHashCode();
    }
}
