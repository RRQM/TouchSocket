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

readonly struct Subscription : IEquatable<Subscription>
{
    public Subscription(string clientId, QosLevel qosLevel) : this(clientId)
    {
        this.QosLevel = qosLevel;
    }

    public Subscription(string clientId)
    {
        this.ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        this.QosLevel = default;
    }

    public string ClientId { get; }
    public QosLevel QosLevel { get; }

    public static bool operator !=(Subscription left, Subscription right)
    {
        return !(left == right);
    }

    public static bool operator ==(Subscription left, Subscription right)
    {
        return left.Equals(right);
    }

    public bool Equals(Subscription other)
    {
        return this.ClientId.Equals(other.ClientId);
    }

    public override bool Equals(object obj)
    {
        return obj is Subscription subscription && this.Equals(subscription);
    }

    public override int GetHashCode()
    {
        return this.ClientId.GetHashCode();
    }
}
