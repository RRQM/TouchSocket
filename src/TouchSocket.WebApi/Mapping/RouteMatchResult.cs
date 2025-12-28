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

using TouchSocket.Http;
using TouchSocket.Rpc;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace TouchSocket.WebApi;

/// <summary>
/// 路由匹配结果
/// </summary>
public readonly struct RouteMatchResult
{
    private RouteMatchResult(RouteMatchStatus status, RpcMethod rpcMethod, IEnumerable<HttpMethod> allowedMethods)
    {
        this.Status = status;
        this.RpcMethod = rpcMethod;
        this.AllowedMethods = allowedMethods;
    }

    /// <summary>
    /// 匹配状态
    /// </summary>
    public RouteMatchStatus Status { get; }

    /// <summary>
    /// 匹配的RPC方法
    /// </summary>
    public RpcMethod RpcMethod { get; }

    /// <summary>
    /// 允许的HTTP方法列表
    /// </summary>
    public IEnumerable<HttpMethod> AllowedMethods { get; }

    /// <summary>
    /// 创建成功的匹配结果
    /// </summary>
    public static RouteMatchResult Success(RpcMethod rpcMethod)
    {
        return new RouteMatchResult(RouteMatchStatus.Success, rpcMethod, null);
    }

    /// <summary>
    /// 创建路由未找到的结果
    /// </summary>
    public static RouteMatchResult NotFound()
    {
        return new RouteMatchResult(RouteMatchStatus.NotFound, null, null);
    }

    /// <summary>
    /// 创建方法不允许的结果
    /// </summary>
    public static RouteMatchResult MethodNotAllowed(IEnumerable<HttpMethod> allowedMethods)
    {
        return new RouteMatchResult(RouteMatchStatus.MethodNotAllowed, null, allowedMethods);
    }

    /// <summary>
    /// 创建OPTIONS请求的结果
    /// </summary>
    public static RouteMatchResult Options(IEnumerable<HttpMethod> allowedMethods)
    {
        return new RouteMatchResult(RouteMatchStatus.Options, null, allowedMethods);
    }
}
