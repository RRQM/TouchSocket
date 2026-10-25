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

namespace TouchSocket.Mcp;

/// <summary>
/// 基于命名管道传输的 MCP 调用上下文，调用者为当前会话客户端。
/// </summary>
public sealed class McpNamedPipeCallContext : McpCallContextBase
{
    /// <summary>
    /// 初始化 <see cref="McpNamedPipeCallContext"/>。
    /// </summary>
    /// <param name="caller">当前命名管道会话客户端。</param>
    /// <param name="cancellationToken">连接关闭时取消的令牌。</param>
    public McpNamedPipeCallContext(McpNamedPipeSessionClient caller, CancellationToken cancellationToken)
        : base(caller, cancellationToken)
    {
    }
}
