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

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace TouchSocket.WebApi;

/// <summary>
/// 路由匹配状态
/// </summary>
public enum RouteMatchStatus
{
    /// <summary>
    /// 成功匹配
    /// </summary>
    Success,

    /// <summary>
    /// 路由未找到
    /// </summary>
    NotFound,

    /// <summary>
    /// 方法不允许
    /// </summary>
    MethodNotAllowed,

    /// <summary>
    /// OPTIONS请求,返回允许的方法列表
    /// </summary>
    Options
}