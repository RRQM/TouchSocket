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

namespace TouchSocket.Dmtp;

/// <summary>
/// 路由类型
/// </summary>
public readonly struct RouteType
{
    // 私有字段，存储路由类型的值
    private readonly string m_value;

    /// <summary>
    /// 路由类型的构造函数
    /// </summary>
    /// <param name="value">路由类型的字符串表示</param>
    /// <exception cref="ArgumentException">当value为<see langword="null"/>或空时抛出</exception>
    public RouteType(string value)
    {
        // 检查参数value是否为<see langword="null"/>或空，如果是则抛出ArgumentException异常
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"“{nameof(value)}”不能为 null 或空。", nameof(value));
        }

        // 将value转换为小写并去除首尾空格，然后赋值给m_value
        this.m_value = value.ToLower().Trim();
    }

    /// <summary>
    /// 判断两个RouteType对象是否相等
    /// </summary>
    /// <param name="a">第一个RouteType对象</param>
    /// <param name="b">第二个RouteType对象</param>
    /// <returns>如果两个对象相等返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public static bool operator ==(RouteType a, RouteType b)
    {
        return a.m_value == b.m_value;
    }

    /// <summary>
    /// 判断两个RouteType对象是否不相等
    /// </summary>
    /// <param name="a">第一个RouteType对象</param>
    /// <param name="b">第二个RouteType对象</param>
    /// <returns>如果两个对象不相等返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public static bool operator !=(RouteType a, RouteType b)
    {
        return a.m_value != b.m_value;
    }

    /// <summary>
    /// 重写Equals方法，用于比较两个RouteType对象是否相等
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>如果对象相等返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public override bool Equals(object obj)
    {
        return obj is RouteType type && this == type;
    }

    /// <summary>
    /// 重写GetHashCode方法，返回m_value的哈希码
    /// </summary>
    /// <returns>m_value的哈希码</returns>
    public override int GetHashCode()
    {
        return this.m_value.GetHashCode();
    }

    /// <summary>
    /// 重写ToString方法，返回m_value的值
    /// </summary>
    /// <returns>m_value的值</returns>
    public override string ToString() => this.m_value;

    /// <summary>
    /// 一个Ping探测路由包
    /// </summary>
    public static readonly RouteType Ping = new RouteType("Ping");

    /// <summary>
    /// 创建通道路由包
    /// </summary>
    public static readonly RouteType CreateChannel = new RouteType("CreateChannel");

    /// <summary>
    /// Rpc调用的路由包
    /// </summary>
    public static readonly RouteType Rpc = new RouteType("Rpc");

    /// <summary>
    /// 拉取文件的路由包
    /// </summary>
    public static readonly RouteType PullFile = new RouteType("PullFile");

    /// <summary>
    /// 推送文件的路由包
    /// </summary>
    public static readonly RouteType PushFile = new RouteType("PushFile");
}