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

namespace TouchSocket.Mcp;

/// <summary>
/// 基于 HTTP 传输的 MCP 调用上下文接口。
/// </summary>
/// <remarks>
/// 此接口定义了在基于 HTTP 的 MCP 调用中所需的上下文信息，包括 HTTP 上下文和 HTTP 会话客户端。
/// 实现这个接口的类应该提供对这些属性的访问，以便在 MCP 调用过程中使用。
/// issue:https://github.com/RRQM/TouchSocket/issues/137
/// </remarks>
public interface IMcpHttpCallContext : IMcpCallContext
{
    /// <summary>
    /// 获取当前 HTTP 上下文，包含 <see cref="HttpContext.Request"/> 和 <see cref="HttpContext.Response"/>。
    /// </summary>
    HttpContext HttpContext { get; }

    /// <summary>
    /// 获取 HTTP 会话客户端。
    /// </summary>
    IHttpSessionClient HttpSessionClient { get; }
}
