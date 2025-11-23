// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Core;

/// <summary>
/// AOT相关成员类型定义。
/// </summary>
public static class AOT
{
    /// <summary>
    /// 序列化格式化器成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes SerializerFormatterMemberType = DynamicallyAccessedMemberTypes.All;


    /// <summary>
    /// 容器成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes Container = DynamicallyAccessedMemberTypes.PublicConstructors;

    /// <summary>
    /// 插件成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes PluginMemberType = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods;

    /// <summary>
    /// Rpc调用成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes RpcInvoke = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    /// <summary>
    /// 成员访问器成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes MemberAccessor = DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    /// <summary>
    /// 快速二进制格式化器成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes FastBinaryFormatter = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    /// <summary>
    /// Rpc注册成员类型。
    /// </summary>
    public const DynamicallyAccessedMemberTypes RpcRegister = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.NonPublicMethods;
}
