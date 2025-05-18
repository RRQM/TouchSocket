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

using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 全局环境设置
/// </summary>
public static class GlobalEnvironment
{
    /// <summary>
    /// 动态构建类型，默认使用IL
    /// </summary>
    public static DynamicBuilderType DynamicBuilderType { get; set; } = DynamicBuilderType.IL;

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER

    /// <summary>
    /// 是否支持动态代码
    /// </summary>
    public static bool IsDynamicCodeSupported => RuntimeFeature.IsDynamicCodeSupported;
#else
    /// <summary>
    /// 是否支持动态代码
    /// </summary>
    public static bool IsDynamicCodeSupported => true;
#endif

}